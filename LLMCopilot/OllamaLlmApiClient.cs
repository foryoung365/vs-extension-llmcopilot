using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OllamaSharp.Streamer;

namespace LLMCopilot
{
    /// <summary>
    /// Ollama API client adapter that implements ILlmApiClient.
    /// Wraps the existing OllamaSharp client.
    /// </summary>
    public class OllamaLlmApiClient : ILlmApiClient
    {
        private readonly OllamaApiClient _ollamaClient;
        private string _selectedModel;

        public string SelectedModel
        {
            get => _selectedModel;
            set => _selectedModel = value;
        }

        public OllamaLlmApiClient(string baseUrl, string defaultModel, string accessToken = null)
        {
            _ollamaClient = new OllamaApiClient(baseUrl);
            _selectedModel = defaultModel;
            _ollamaClient.SelectedModel = defaultModel;
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                _ollamaClient.SetAuthorizationHeader(accessToken);
            }
        }

        public void SetAuthorizationHeader(string token)
        {
            _ollamaClient.SetAuthorizationHeader(token);
        }

        public async Task<IEnumerable<string>> ListModelsAsync(CancellationToken cancellationToken = default)
        {
            var models = await _ollamaClient.ListLocalModels(cancellationToken);
            return models.Select(m => m.Name);
        }

        public async Task<IDictionary<string, string>> ShowModelInformationAsync(
            string model,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var info = await _ollamaClient.ShowModelInformation(model, cancellationToken);

                var dict = new Dictionary<string, string>
                {
                    ["ModelName"] = model
                };

                if (info != null)
                {
                    if (!string.IsNullOrEmpty(info.License))
                        dict["License"] = info.License;

                    if (!string.IsNullOrEmpty(info.Modelfile))
                        dict["Modelfile"] = info.Modelfile;

                    if (!string.IsNullOrEmpty(info.Parameters))
                        dict["Parameters"] = info.Parameters;

                    if (!string.IsNullOrEmpty(info.Template))
                        dict["Template"] = info.Template;
                }

                return dict;
            }
            catch
            {
                return new Dictionary<string, string> { ["ModelName"] = model };
            }
        }

    }
}
