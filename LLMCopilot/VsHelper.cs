using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Task = System.Threading.Tasks.Task;
using System.Threading;
using Thread = System.Threading.Thread;

namespace LLMCopilot
{
    public static class VsHelpers
    {
        public static bool IsSending { get; set; } = false;
        private static readonly Thread _completionThread;
        private static readonly BlockingCollection<Func<CancellationToken, Task>> _taskQueue = new BlockingCollection<Func<CancellationToken, Task>>();
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static readonly object _lock = new object();

        static VsHelpers()
        {
            _completionThread = new Thread(Run);
            _completionThread.IsBackground = true;
            _completionThread.Start();
        }

        private static void Run()
        {
            foreach (var task in _taskQueue.GetConsumingEnumerable())
            {
                try
                {
                    var cts = new CancellationTokenSource();
                    lock (_lock)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource = cts;
                    }
                    task(cts.Token).GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    // Task was canceled, continue with next task
                }
            }
        }

        public static void EnqueueTask(Func<CancellationToken, Task> task)
        {
            lock (_lock)
            {
                _cancellationTokenSource.Cancel(); // 取消之前的所有任务
                while (_taskQueue.Count > 0)
                {
                    _taskQueue.Take();
                }
                _taskQueue.Add(task); // 添加新任务
            }
        }

        public static async Task<IWpfTextView> GetActiveTextViewAsync(IAsyncServiceProvider serviceProvider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var textManager = await serviceProvider.GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager;
            textManager.GetActiveView(1, null, out IVsTextView vTextView);
            IVsUserData userData = vTextView as IVsUserData;
            if (userData == null)
                return null;

            Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
            userData.GetData(ref guidViewHost, out object holder);
            IWpfTextViewHost viewHost = (IWpfTextViewHost)holder;
            return viewHost?.TextView;
        }

        public static async Task<string> GetActiveDocumentFileNameAsync(IAsyncServiceProvider serviceProvider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var textManager = await serviceProvider.GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager;
            textManager.GetActiveView(1, null, out IVsTextView vTextView);

            if (vTextView != null)
            {
                IVsTextLines textLines;
                vTextView.GetBuffer(out textLines);

                if (textLines is IPersistFileFormat persistFileFormat)
                {
                    persistFileFormat.GetCurFile(out string fullPath, out uint _);
                    return Path.GetFileName(fullPath);
                }
            }

            return null;
        }

        public static async Task<IVsTextLines> GetActiveTextLinesAsync(IAsyncServiceProvider serviceProvider)
        {
            var textView = await GetActiveTextViewAsync(serviceProvider);
            var textManager = await serviceProvider.GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager;
            textManager.GetActiveView(1, null, out IVsTextView vTextView);
            if (vTextView != null)
            {
                IVsTextLines textLines;
                vTextView.GetBuffer(out textLines);
                return textLines;
            }
            return null;
        }

        public static string RemoveCommonSuffixPrefix(string A, string B)
        {
            B = B.TrimStart();
            int minLen = Math.Min(A.Length, B.Length);

            int commonLength = 0;
            for (int i = 1; i <= minLen; i++)
            {
                if (A.Substring(A.Length - i) == B.Substring(0, i))
                {
                    commonLength = i;
                }
            }

            if (commonLength > 0)
            {
                return A.Substring(0, A.Length - commonLength);
            }

            return A;
        }

        public static string GetPrefixLines(IWpfTextView textView, int n)
        {
            if (textView == null)
            {
                return null;
            }

            var caretPosition = textView.Caret.Position.BufferPosition;
            var snapshot = caretPosition.Snapshot;

            var currentLine = snapshot.GetLineFromPosition(caretPosition);
            var currentLineText = currentLine.GetText().Substring(0, caretPosition.Position - currentLine.Start.Position);

            var startLineIndex = Math.Max(0, currentLine.LineNumber - (n - 1));

            var sb = new System.Text.StringBuilder();

            for (int i = startLineIndex; i < currentLine.LineNumber; i++)
            {
                var line = snapshot.GetLineFromLineNumber(i);
                sb.AppendLine(line.GetText());
            }

            // Append the current line up to the caret position
            sb.Append(currentLineText);

            return sb.ToString();
        }

        public static string GetSuffixLines(IWpfTextView textView, int n)
        {
            if (textView == null)
            {
                return null;
            }

            var caretPosition = textView.Caret.Position.BufferPosition;
            var snapshot = caretPosition.Snapshot;

            var currentLine = snapshot.GetLineFromPosition(caretPosition);
            var currentLineText = currentLine.GetText().Substring(caretPosition.Position - currentLine.Start.Position);

            var endLineIndex = Math.Min(snapshot.LineCount - 1, currentLine.LineNumber + (n - 1));

            var sb = new System.Text.StringBuilder();

            // Append the current line from caret position to the end
            sb.AppendLine(currentLineText);

            for (int i = currentLine.LineNumber + 1; i <= endLineIndex; i++)
            {
                var line = snapshot.GetLineFromLineNumber(i);
                sb.AppendLine(line.GetText());
            }

            return sb.ToString();
        }

        public static string GetSourceCodeType(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                case ".py":
                    return "python";
                case ".cpp":
                case ".c":
                case ".h":
                    return "cpp";
                case ".cs":
                    return "csharp";
                case ".js":
                    return "javascript";
                case ".html":
                case ".htm":
                    return "html";
                case ".css":
                    return "css";
                case ".java":
                    return "java";
                case ".ts":
                    return "typescript";
                case ".json":
                    return "json";
                case ".xml":
                    return "xml";
                case ".sql":
                    return "sql";
                case ".rb":
                    return "ruby";
                case ".php":
                    return "php";
                case ".swift":
                    return "swift";
                case ".go":
                    return "go";
                case ".rs":
                    return "rust";
                case ".kt":
                case ".kts":
                    return "kotlin";
                case ".sh":
                    return "bash";
                case ".bat":
                    return "batch";
                case ".md":
                    return "markdown";
                case ".r":
                    return "r";
                case ".pl":
                    return "perl";
                case ".lua":
                    return "lua";
                // Add more cases as needed
                default:
                    return "plaintext"; // Default to plaintext for unknown types
            }
        }
        
        public static string StopAtSimilarLine(string stream, string line)
        {
                        // 拆分第一个字符串，考虑不同操作系统的换行符
            string[] lines = stream.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            // 第二个字符串只取拆分后，不为全是空白或换行符的行
            string[] filteredLines = line.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();
            if (filteredLines.Length == 0)
            {
                return stream;
            }

            line = filteredLines[0];
            line = line.Trim();
            bool lineIsBracketEnding = IsBracketEnding(line);

            StringBuilder result = new StringBuilder();

            foreach (string nextLine in lines)
            {
                if (lineIsBracketEnding && line.Trim() == nextLine.Trim())
                {
                    break;
                }

                bool lineQualifies = nextLine.Length > 4 && line.Length > 4;
                if (lineQualifies && ComputeDistance(nextLine.Trim(), line) / (double)line.Length < 0.1)
                {
                    break;
                }
                result.AppendLine(nextLine);
            }

            return result.ToString();
        }

        private static bool IsBracketEnding(string line)
        {
            char[] bracketEnding = { ')', ']', '}', ';' };
            return line.Trim().Any(c => bracketEnding.Contains(c));
        }

        private static int ComputeDistance(string str1, string str2)
        {
            int[,] dp = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
            {
                dp[i, 0] = i;
            }

            for (int j = 0; j <= str2.Length; j++)
            {
                dp[0, j] = j;
            }

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
                }
            }

            return dp[str1.Length, str2.Length];
        }

        public static void OpenChatWindow()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = LLMCopilotProvider.Package.FindToolWindow(typeof(LLMChatWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        public static void CodeCompleteCommand()
        {
            EnqueueTask(async (cancellationToken) =>
            {
                await CodeCompleteCommandAsync(cancellationToken);
            });
        }

        public static async Task CodeCompleteCommandAsync(CancellationToken cancellationToken)
        {
            IsSending = true;

            try
            {
                await LLMCopilotProvider.EnsurePackageLoadedAsync();

                OllamaApiClient ollamaApiClient = null;
                ChatClient openAIChat = null;

                var apiKind = OllamaHelper.Instance.Options.LlmAPiType;

                if (apiKind == LlmApiKind.OpenAI)
                {
                    openAIChat = LlmClientFactory.CreateOpenAIChatClient();
                }
                else 
                {
                    ollamaApiClient = LlmClientFactory.CreateOllamaClient();
                }

                var Package = LLMCopilotProvider.Package;

                RequestOptions reqOps = OllamaHelper.Instance.CompRequestOptions;
                int nPrefixLines = OllamaHelper.EstimatePrefixLinesByCtx(reqOps.NumCtx);
                int nSuffixLines = OllamaHelper.EstimateSuffixLinesByCtx(reqOps.NumCtx);

                var textView = await VsHelpers.GetActiveTextViewAsync(Package);

                string PrefixCode = VsHelpers.GetPrefixLines(textView, nPrefixLines);
                string SuffixCode = VsHelpers.GetSuffixLines(textView, nSuffixLines);

                var options = OllamaHelper.Instance.Options;
               
                string template = $"{options.FimBegin}{PrefixCode}{options.FimHole}{SuffixCode}{options.FimEnd}";

                GenerateCompletionRequest req = new GenerateCompletionRequest
                {
                    Model = ollamaApiClient != null ? ollamaApiClient.SelectedModel : OllamaHelper.Instance.Options.CompleteModel,
                    Prompt = template,
                    Options = reqOps,
                    Raw = true
                };

                var OldCaretPosition = textView.Caret.Position.BufferPosition;

                string comp_text = string.Empty;

                if (apiKind == LlmApiKind.OpenAI)
                {
                    List<ChatMessage> chatMessageList = new List<ChatMessage>();
                    chatMessageList.Add(new SystemChatMessage(req.Prompt));

                    ChatCompletion resp = await openAIChat.CompleteChatAsync(chatMessageList, null, cancellationToken);

                    comp_text = resp.Content[0].Text;
                }
                else
                {
                    var resp = await ollamaApiClient.GetCompletion(req, cancellationToken);
                    
                    comp_text = resp.Response;
                }

                var NewCaretPosition = textView.Caret.Position.BufferPosition;
                if (NewCaretPosition.CompareTo(OldCaretPosition) != 0)
                {
                    return;
                }

                if (string.IsNullOrEmpty(comp_text))
                {
                    return;
                }

                comp_text = RemoveCommonSuffixPrefix(comp_text, SuffixCode);
                // 在 UI 线程上创建和更新 Adornment
                textView.VisualElement.Dispatcher.Invoke(() =>
                {
                    LLMAdornmentFactory.CreateAdornment(textView);
                    var adornment = LLMAdornmentFactory.GetCurrentAdornment();
                    if (adornment != null)
                    {
                        adornment.UpdatePrediction(comp_text);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                //do nothing
                //LLMErrorHandler.WriteLog("Code completion was canceled.");
            }
            catch (Exception ex)
            {
                LLMErrorHandler.HandleException(ex);
            }
            finally
            {
                IsSending = false;
            }
        }
    }
}
