using System;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace LLMCopilot
{
    public enum ResponseLanguage
    { 
        English,
        Chinese,
    }
    public enum LlmApiKind
    {
        OpenAI,
        Ollama
    }

    public class OptionPageGrid : DialogPage
    {
        private string baseUrl = "http://localhost:11434";
        private LlmApiKind apiType = LlmApiKind.OpenAI;
        private string completeModel = "deepseek-coder:6.7b";
        private string chatModel = "deepseek-coder:6.7b";
        private bool enableAutoComplete = false;
        private string fim_begin = "<｜fim▁begin｜>";
        private string fim_end = "<｜fim▁end｜>";
        private string fim_hole = "<｜fim▁hole｜>";
        private ResponseLanguage language = ResponseLanguage.English;
        private string access_token = "NO_KEY";
        private int chat_ctx_size = 4096;
        private int complete_ctx_size = 2048;

        public event EventHandler SettingsChanged;

        [Category("LLMCopilot")]
        [DisplayName("Base URL")]
        [Description("API Base URL.")]
        public string BaseUrl
        {
            get { return baseUrl; }
            set { baseUrl = value; }
        }

        [Category("LLMCopilot")]
        [DisplayName("LLM API Type")]
        [Description("Supported endpoint APIs are OpenAI-compatible or Ollama.")]
        public LlmApiKind LlmAPiType
        {
            get { return apiType; }
            set { apiType = value; }
        }

        [Category("LLMCopilot")]
        [DisplayName("LLM response Language")]
        [Description("Language used in LLM's response(if available)")]
        public ResponseLanguage Language
        {
            get { return language; }
            set { language = value; }
        }

        protected void OnSettingsChanged()
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (!IsValidUrl(BaseUrl))
            {
                // 如果 URL 无效，显示错误消息并阻止页面关闭
                VsShellUtilities.ShowMessageBox(
                    this.Site,
                    "The URL provided is invalid, please try again.",
                    "IVALID URL",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                e.ApplyBehavior = ApplyKind.Cancel;
                return;
            }

            if (string.IsNullOrEmpty(this.AccessToken))
            {
                VsShellUtilities.ShowMessageBox(
                    this.Site,
                    "API access token cannot be empty.",
                    "Invalid API Token",
                    OLEMSGICON.OLEMSGICON_WARNING,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                e.ApplyBehavior = ApplyKind.Cancel;
                return;
            }

            OnSettingsChanged();
            base.OnApply(e);
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result)
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }

        [Category("LLMCopilot")]
        [DisplayName("Code Complete Model")]
        [Description("LLM model name used for code complete")]
        public string CompleteModel
        {
            get { return completeModel; }
            set { 
                completeModel = value;
            }
        }

        [Category("LLMCopilot")]
        [DisplayName("complete ctx length")]
        [Description("Context length for complete model")]
        public int CompleteCtxSize
        {
            get { return complete_ctx_size; }
            set
            {
                complete_ctx_size = value;
            }
        }

        [Category("LLMCopilot")]
        [DisplayName("Chat model")]
        [Description("LLM model name used for chat")]
        public string ChatModel
        {
            get { return chatModel; }
            set { 
                chatModel = value;
            }
        }

        [Category("LLMCopilot")]
        [DisplayName("chat ctx length")]
        [Description("Context length for chat model")]
        public int ChatCtxSize
        {
            get { return chat_ctx_size; }
            set
            {
                chat_ctx_size = value;
            }
        }

        [Category("LLMCopilot")]
        [DisplayName("Enable Auto Complete")]
        [Description("Auto complete code when you typing")]
        public bool EnableAutoComplete
        {
            get { return enableAutoComplete; }
            set
            {
                enableAutoComplete = value;
            }
        }

        [Category("LLMCopilot")]
        [DisplayName("Fim begin token")]
        [Description("Fill in the middle begin Token for code complete LLM model You have selected")]
        public string FimBegin
        {
            get { return fim_begin; }
            set
            {
                fim_begin = value;
            }
        }

        [Category("LLMCopilot")]
        [DisplayName("Fim end token")]
        [Description("Fill in the middle end Token for code complete LLM model You have selected")]
        public string FimEnd
        {
            get { return fim_end; }
            set
            {
                fim_end = value;
            }
        }

        [Category("LLMCopilot")]
        [DisplayName("Fim hole token")]
        [Description("Fill in the middle hole Token for code complete LLM model You have selected")]
        public string FimHole
        {
            get { return fim_hole; }
            set
            {
                fim_hole = value;
            }
        }

        [Category("LLMCopilot")]
        [DisplayName("Reverse Proxy Access Token")]
        [Description("Bearer access token used for reverse proxy to connect to your LLM API")]
        public string AccessToken
        {
            get { return access_token; }
            set { access_token = value; }
        }
    }
}
