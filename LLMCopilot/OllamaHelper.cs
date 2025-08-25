using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Controls;
using System.Windows;
using OllamaSharp.Models;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using Microsoft.VisualStudio;
using System.Net.Http.Headers;

namespace LLMCopilot
{
    public sealed class OllamaHelper
    {
        public string CodeCompleteTemplate = $"<|fim▁begin|>{0}<|fim▁hole|>{1}<|fim▁end|>";

        private static readonly Lazy<OllamaHelper> lazy = new Lazy<OllamaHelper>(() => new OllamaHelper());

        public static OllamaHelper Instance { get { return lazy.Value; } }

        private static readonly int defaultContext = 4096;
        private static readonly int defaultCodeLineLength = 80;
        private static readonly double PrefixCodeLinePercent = 0.8;
        private static readonly double SuffixCodeLinePercent = 1 - PrefixCodeLinePercent;
        private static readonly double defaultContextUsage = 0.8;

        public RequestOptions CompRequestOptions { get; private set; }
        public RequestOptions ChatRequestOptions { get; private set; }

        private string[] stop;

        public OptionPageGrid Options { get; private set; }


        private OllamaHelper()
        {
            var package = LLMCopilotProvider.Package;
            Options = (OptionPageGrid)package.GetDialogPage(typeof(OptionPageGrid));

            stop = new string[]{
                Options.FimBegin,
                Options.FimHole,
                Options.FimEnd,
                "//",
                "<｜end▁of▁sentence｜>",
                "\n\n",
                "\r\n\r\n",
                "/src/","#- coding: utf-8",
                "```",
                "\nclass",
                "\nnamespace",
                "\nvoid"
                };

            CompRequestOptions = new RequestOptions {
                NumCtx = Options.CompleteCtxSize,
                NumPredict = 128,
                Stop = stop,
                Temperature = 0.3f
            };

            ChatRequestOptions = new RequestOptions
            {
                NumCtx = Options.ChatCtxSize,
                NumPredict = 1024,
                Temperature = 0.7f
            };

            Options.SettingsChanged += Options_SettingsChanged;
        }

        ~OllamaHelper()
        {
            Options.SettingsChanged -= Options_SettingsChanged;
        }

        public string GetExplainCodeTemplate(string code, string file)
        {
            string code_type = VsHelpers.GetSourceCodeType(file);

            string templateRU = $@"## Инструкция
Обобщите приведенный ниже код (с акцентом на его ключевую функциональность).

## Выбранный код
```{code_type}
{code}
```

## Задача
Обобщите код на высоком уровне (включая цель и назначение) с акцентом на его ключевую функциональность.

## Ответ

";
            string templateEN = $@"## Instructions
Summarize the code below (emphasizing its key functionality).

## Selected Code
```{code_type}
{code}
```

## Task
Summarize the code at a high level (including goal and purpose) with an emphasis on its key functionality.

## Response

";
            string templateCN = $@"## 说明
总结下面的代码（强调其关键功能）。

## 选定代码
```{code_type}
{code}
```

## 任务
高层次地总结代码（包括目标和目的），并着重介绍其关键功能。

## 回答

";

            return
                  Options.Language == ResponseLanguage.English ? templateEN :
                  Options.Language == ResponseLanguage.Chinese ? templateCN :
                  Options.Language == ResponseLanguage.Russian ? templateRU :
                  throw new NotSupportedException("Specified language is not supported");
        }

        public string GetFindBugTemplate(string code, string file)
        {
            string code_type = VsHelpers.GetSourceCodeType(file);

            string templateRU = $@"## Инструкции
Что может быть не так в приведённом ниже коде?
Рассматривайте только те ошибки, которые могут привести к некорректному поведению программы.
Язык программирования — {code_type}.

## Выбранный код
```{code_type}
{code}
```

## Задача
Опишите, что может быть не так в коде.
Рассматривайте только ошибки, которые могут привести к некорректной работе.
По возможности предложите варианты исправления.
Учитывайте, что в коде может и не быть ошибок.
Включайте примеры и фрагменты кода (в формате Markdown), где это уместно.

## Анализ

";
            string templateEN = $@"## Instructions
What could be wrong with the code below?
Only consider defects that would lead to incorrect behavior.
The programming language is {code_type}.

## Selected Code
```{code_type}
{code}
```

## Task
Describe what could be wrong with the code?
Only consider defects that would lead to incorrect behavior.
Provide potential fix suggestions where possible.
Consider that there might not be any problems with the code.
Include code snippets(using Markdown) and examples where appropriate.

## Analysis

";
            string templateCN = $@"
## 说明
下面的代码可能有什么问题？
只考虑会导致不正确行为的缺陷。
编程语言是{code_type}。

## 选定代码
```{code_type}
{code}
```

## 任务
描述代码可能有什么问题？
只考虑会导致不正确行为的缺陷。
在可能的情况下提供潜在的修复建议。
考虑代码可能没有任何问题的情况。
在适当的情况下包含代码片段（使用Markdown）和示例。

## 分析

";

            return
                Options.Language == ResponseLanguage.English ? templateEN :
                Options.Language == ResponseLanguage.Chinese ? templateCN :
                Options.Language == ResponseLanguage.Russian ? templateRU :
                throw new NotSupportedException("Specified language is not supported");
        }

        public string GetOptimizeCodeTemplate(string code, string file)
        {
            string code_type = VsHelpers.GetSourceCodeType(file);
            string templateEN = $@"## Instructions
How could the readability and performance of the code below be improved?
The programming language is {code_type}.
Consider overall readability, performance and idiomatic constructs.

## Selected Code
```{code_type}
{code}
```

## Task
How could the readability and performance of the code be improved?
The programming language is {code_type}.
Consider overall readability, performance and idiomatic constructs.
Provide potential improvements suggestions where possible.
Consider that the code might be perfect and no improvements are possible.
Include code snippets (using Markdown) and examples where appropriate.
The code snippets must contain valid {code_type} code.

## Readability and Performance Improvements

";
            string templateCN = $@"## 说明
如何提高下面代码的可读性和性能？
编程语言是{code_type}。
考虑整体可读性、性能和惯用构造。

## 选定代码
```{code_type}
{code}
```

## 任务
如何提高代码的可读性和性能？
编程语言是{code_type}。
考虑整体可读性、性能和惯用构造。
在可能的情况下提供潜在的改进建议。
考虑代码可能已经完美，没有改进的空间。
在适当的情况下包含代码片段（使用Markdown）和示例。
代码片段必须包含有效的{code_type}代码。

## 可读性和性能改进

";

            return Options.Language == ResponseLanguage.English ? templateEN : templateCN;
        }

        public string GetUnitTestTemplate(string code, string file)
        {
            string code_type = VsHelpers.GetSourceCodeType(file);
            string templateEN = $@"## Instructions
Write a unit test for the code below.

## Selected Code
```{code_type}
{code}
```

## Task
Write a unit test that contains test cases for the happy path and for all edge cases.
The programming language is {code_type}.

## Response

";
            string templateCN = $@"## 说明
为下面的代码编写单元测试。

## 选定代码
```{code_type}
{code}
```

## 任务
编写一个包含正常情况和所有边缘情况的单元测试。
编程语言是{code_type}。

## 回答

";

            return Options.Language == ResponseLanguage.English ? templateEN : templateCN;
        }

        public string GetAddCommentTemplate(string code, string file)
        {
            string code_type = VsHelpers.GetSourceCodeType(file);
            string templateEN = $@"## Instructions
Document the code on function/method/class level.
Avoid line comments.
The programming language is {code_type}.

## Code
```{code_type}
{code}
```

## Documented Code

";
            string templateCN = $@"## 说明
在函数/方法/类级别上对代码进行文档化。
避免行内注释。
编程语言是{code_type}。

## 代码
```{code_type}
{code}
```

## 文档化的代码

";

            return Options.Language == ResponseLanguage.English ? templateEN : templateCN;
        }

        public static int EstimateTokensByChars(string str)
        {
            return str.Length / 4;
        }

        public static int EstimateCharsByTokens(int nTokens)
        {
            return Convert.ToInt32(Convert.ToDouble(nTokens) * defaultContextUsage * 4);
        }


        public static int EstimatePrefixLinesByCtx(int? nCtx)
        {
            if (!nCtx.HasValue)
            {
                // 如果 nCtx 没有值，则返回一个默认值，例如0
                return 0;
            }

            return Convert.ToInt32(EstimateCharsByTokens(nCtx.Value) * PrefixCodeLinePercent) / defaultCodeLineLength;
        }

        public static int EstimateSuffixLinesByCtx(int? nCtx)
        {
            if (!nCtx.HasValue)
            {
                // 如果 nCtx 没有值，则返回一个默认值，例如0
                return 0;
            }

            return Convert.ToInt32(EstimateCharsByTokens(nCtx.Value) * SuffixCodeLinePercent) / defaultCodeLineLength;
        }

        private void Options_SettingsChanged(object sender, EventArgs e)
        {
            this.SetOllamaOptions();
        }

        private void SetOllamaOptions()
        {
            stop[0] = Options.FimBegin;
            stop[1] = Options.FimEnd;
            stop[2] = Options.FimHole;
            CompRequestOptions.Stop = stop;
            CompRequestOptions.NumCtx = Options.CompleteCtxSize;
            ChatRequestOptions.NumCtx = Options.ChatCtxSize;
            
            //Task.Run(async () => await this.InitModelCtx());
        }

        public async Task InitModelCtx()
        {
            try
            {
                var client = OllamaClientFactory.CreateClient();
                var chatModelInfo = await client.ShowModelInformation(Options.ChatModel);
                var compModelInfo = await client.ShowModelInformation(Options.CompleteModel);

                Func<string, string, int> GetCtx = (string parameters, string model) =>
                {
                    int num_ctx = defaultContext; 
                    if (!string.IsNullOrEmpty(parameters))
                    {
                        var match = Regex.Match(parameters, @"PARAMETER\s+num_ctx\s+(\d+)");
                        if (match.Success && match.Groups.Count > 1)
                        {
                            int.TryParse(match.Groups[1].Value, out num_ctx);
                        }
                    }
                    return num_ctx;
                };

                int chatCtx = GetCtx(chatModelInfo.Parameters, Options.ChatModel);
                ChatRequestOptions.NumCtx = chatCtx;

                //int CompCtx = GetCtx(compModelInfo.Parameters, Options.CompleteModel);
                //CompRequestOptions.NumCtx = CompCtx;
            }
            catch (Exception ex)
            {
                LLMErrorHandler.HandleException(ex);
            }

        }

    }

    public static class EventManager
    {
        public static event EventHandler<CommandExecutedEventArgs> CodeCommandExecuted;
        public static event EventHandler<CmdEventArgs> CmdEventsHandler;

        public static void OnCodeCommandExecuted(string selectedText)
        {
            CodeCommandExecuted?.Invoke(null, new CommandExecutedEventArgs(selectedText));
        }

        public static void OnCmdEventHandler(CmdEventType cmdType)
        {
            CmdEventsHandler?.Invoke(null, new CmdEventArgs(cmdType));
        }
    }

    public class CommandExecutedEventArgs : EventArgs
    {
        public string SelectedText { get; }

        public CommandExecutedEventArgs(string selectedText)
        {
            SelectedText = selectedText;
        }
    }

    public enum CmdEventType
    {
        ClearMessages,
        ListModels
    }

    public class CmdEventArgs : EventArgs
    {
        public CmdEventType CmdType { get; }

        public CmdEventArgs(CmdEventType cmdType)
        {
            CmdType = cmdType;
        }
    }

    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserTemplate { get; set; }
        public DataTemplate AssistantTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var message = item as MyMessage;
            if (message != null)
            {
                return message.Role == ChatRole.User ? UserTemplate : AssistantTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }

    public class OllamaClientFactory
    {
        public static OllamaApiClient CreateClient()
        {
            var options = OllamaHelper.Instance.Options;
            var ollamaApiClient = new OllamaApiClient(options.BaseUrl);
            ollamaApiClient.SelectedModel = options.CompleteModel;
            ollamaApiClient.SetAuthorizationHeader(options.AccessToken);
            
            return ollamaApiClient;
        }

        public static Chat CreateChat(Action<ChatResponseStream> streamer)
        {
            var ollamaApiClient = CreateClient();
            var options = OllamaHelper.Instance.Options;
            ollamaApiClient.SelectedModel = options.ChatModel;
            var chat = new Chat(ollamaApiClient, streamer, OllamaHelper.Instance.ChatRequestOptions);

            return chat;
        }
    }
}
