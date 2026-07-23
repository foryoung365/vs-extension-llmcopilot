using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LLMCopilot
{
    /// <summary>
    /// Represents a message in a conversation with role and content.
    /// </summary>
    public class LlmMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }

        public LlmMessage() { }

        public LlmMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }
    }

    /// <summary>
    /// Represents a chat response stream chunk.
    /// </summary>
    public class LlmChatResponseChunk
    {
        /// <summary>
        /// The message component of the response.
        /// </summary>
        public LlmMessage Message { get; set; }

        /// <summary>
        /// Indicates whether the chat interaction is completed.
        /// </summary>
        public bool Done { get; set; }
    }

    /// <summary>
    /// Abstracts LLM API operations for Ollama.
    /// </summary>
    public interface ILlmApiClient
    {
        /// <summary>
        /// Gets or sets the name of the model to use for requests.
        /// </summary>
        string SelectedModel { get; set; }

        /// <summary>
        /// Sets the Authorization header for HTTP requests.
        /// </summary>
        /// <param name="token">The bearer token to set</param>
        void SetAuthorizationHeader(string token);

        /// <summary>
        /// Lists all available local models.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel operations</param>
        /// <returns>Collection of available model names</returns>
        Task<IEnumerable<string>> ListModelsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets information about a specific model.
        /// </summary>
        /// <param name="model">Model name to get info for</param>
        /// <param name="cancellationToken">Token to cancel operations</param>
        /// <returns>Model information as a dictionary, or null if not available</returns>
        Task<IDictionary<string, string>> ShowModelInformationAsync(
            string model, 
            CancellationToken cancellationToken = default);
    }
}
