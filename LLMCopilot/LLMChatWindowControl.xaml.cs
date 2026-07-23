using OllamaSharp;
using OllamaSharp.Models.Chat;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MdXaml;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Text;

namespace LLMCopilot
{
    public class MyMessage : INotifyPropertyChanged
    {
        Message _message;
        public string Content
        {
            get => _message.Content;
            set
            {
                if (_message.Content != value)
                {
                    _message.Content = value;
                    OnPropertyChanged();
                }
            }
        }

        public ChatRole? Role
        {
            get => _message.Role;
            private set { }
        }

        public MyMessage(Message message)
        {
            _message = message;
        }

        public MyMessage(ChatRole? role, string content)
        {
            _message = new Message(role, content);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    /// <summary>
    /// Interaction logic for LLMChatWindowControl.
    /// </summary>
    public partial class LLMChatWindowControl : UserControl
    {
        private ObservableCollection<MyMessage> _messages = new ObservableCollection<MyMessage>();
        public ObservableCollection<MyMessage> Messages => _messages;
        private Chat ChatOllama { get; set; }
        private ChatClient ChatOpenAI { get; set; }

        private StringBuilder _messageCache = new StringBuilder(); // 用于缓存数据
        private LlmApiKind _apiKind = LlmApiKind.Ollama;

        private CancellationTokenSource _cancelTokenSource = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LLMChatWindowControl"/> class.
        /// </summary>
        public LLMChatWindowControl()
        {
            var _ = new MdXaml.MarkdownScrollViewer();//DO NOT Delete This!! fix can't find mdxaml dll
            this.InitializeComponent();
            this.DataContext = this;
            MessagesScrollViewer.PreviewMouseWheel += MessagesScrollViewer_PreviewMouseWheel;
            //this.MessageItemsControl.ItemsSource = _messages;
            this.Unloaded += LLMChatWindowControl_Unloaded;
            this.Loaded += LLMChatWindowControl_loaded;

            
        }

        private void OnChatResponseReceived(ChatResponseStream response)
        {
            this.OnChatResponseReceivedString(response.Message?.Content, response.Done);
        }

        private void OnChatResponseReceivedString(string responseChunk, bool responseDone)
        {
            bool containsKeyword = false;

            if (!String.IsNullOrEmpty(responseChunk))
            {
                // 将新内容添加到缓存
                _messageCache.Append(responseChunk);

                string[] delimeters = { "\n", ",", "，", ".", "。", ":", "：", ";", "；", "\t" };

                containsKeyword = delimeters.Any(keyword => responseChunk.Contains(keyword));
            }

            // 检查是否需要更新消息列表
            if (_messageCache.Length > 64 || containsKeyword || responseDone)
            {
                AppendOrUpdateLastMessage(_messageCache.ToString());
                _messageCache.Clear(); // 清空缓存
            }
        }

        private void ClearChatHistory_Click(object sender, RoutedEventArgs e)
        {
            _messages.Clear();
            
            //
            // OLlama Chat keeps its own internal collection of messages.
            //
            this.ChatOllama = null;
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _cancelTokenSource?.Cancel();

            SettingsCommand.Instance.Execute(this, EventArgs.Empty);

            this.ChatOllama = null;
            this.ChatOpenAI = null;
        }

        private void ListModels_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    string modelNames;
                    
                    if (this._apiKind == LlmApiKind.OpenAI)
                    {
                        OpenAIModelClient modelClient = LlmClientFactory.CreateOpenAIModelClient();
                        
                        var models = await modelClient.GetModelsAsync();
                        modelNames = string.Join("  \n", models.Value.Select(m => m.Id));
                    }
                    else
                    {
                        var client = LlmClientFactory.CreateOllamaClient();
                        var models = await client.ListLocalModels();
                        modelNames = string.Join("  \n", models.Select(m => m.Name));
                    }

                    AddMessage(ChatRole.System, $"Available models:  \n{modelNames}");
                }
                catch (Exception ex)
                {
                    this.OnChatResponseReceivedString("[Exception occured. See Documents\\LLMCopilot*.log]", true);
                    LLMErrorHandler.HandleException(ex);
                }
            });
        }

        private void OnExplainCodeCommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            // 处理事件，执行某些操作
            Dispatcher.Invoke(async () => {
                // 在 UI 线程上更新 UI 或执行操作
                await SendChatMessageAsync(e.SelectedText, ChatRole.System);
                // 或者执行其他操作
            });
        }

        private void CreateLlmChatClientIf()
        {
            if (this._apiKind == LlmApiKind.OpenAI)
            {
                if (this.ChatOpenAI == null)
                {
                    this.ChatOpenAI = LlmClientFactory.CreateOpenAIChatClient();
                }

                return;
            }

            if (this.ChatOllama == null)
            {
                this.ChatOllama = LlmClientFactory.CreateOlamaChat(this.OnChatResponseReceived);
            }
        }

        private void LLMChatWindowControl_loaded(object sender, RoutedEventArgs e)
        {
            EventManager.CodeCommandExecuted += OnExplainCodeCommandExecuted;

            // Detect API type.
            var options = OllamaHelper.Instance.Options;
            this._apiKind = options.LlmAPiType;
            this.ChatOpenAI = null;
            this.ChatOllama = null;
        }

        private void LLMChatWindowControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _cancelTokenSource?.Cancel();

            EventManager.CodeCommandExecuted -= OnExplainCodeCommandExecuted;
            //_messages.Clear();
            KeepLastTenMessages();
        }

        private void AppendOrUpdateLastMessage(string content)
        {
            // UI 线程上执行
            Dispatcher.Invoke(() =>
            {
                if (_messages.Any() && _messages.Last().Role == ChatRole.Assistant)
                {
                    var lastMessage = _messages.Last();
                    lastMessage.Content += content;
                }
                else
                {
                    _messages.Add(new MyMessage(ChatRole.Assistant, content));
                }

                ScrollToBottom();  // 确保每次更新后滚动到底部
            });
        }

        private void ScrollToBottom()
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                if (VisualTreeHelper.GetChildrenCount(MessagesScrollViewer) > 0)
                {
                    MessagesScrollViewer.ScrollToEnd();
                }
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void AddMessage(ChatRole role, string Content)
        {
            Dispatcher.Invoke(() =>
            {
                MyMessage userMessage = new MyMessage(role, Content);
                _messages.Add(userMessage);
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessageAsync();
        }

        public void KeepLastTenMessages()
        {
            if (_messages.Count > 10)
            {
                int removeCount = _messages.Count - 10;
                for (int i = 0; i < removeCount; i++)
                {
                    _messages.RemoveAt(0);
                }
            }

            // TODO BUGBUG
            // For Ollama Chat, one needs to delete the active chat, then create a new empty one,
            // and then copy the remaining 10 messages over into that chat.
            this.ChatOllama = null;
        }

        private async void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                SendMessageAsync();
            }
        }

        private void MessageTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (MessageTextBox.Text == "Press Ctrl+Enter to Send Message")
            {
                MessageTextBox.Text = "";
                MessageTextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void MessageTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                MessageTextBox.Text = "Press Ctrl+Enter to Send Message";
                MessageTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        public async Task SendChatMessageAsync(string text, ChatRole role)
        {
            TimeSpan LlmTimeoutOpenAI = TimeSpan.FromSeconds(30);
            TimeSpan LlmTimeoutOllama = TimeSpan.FromSeconds(60);

            if (!string.IsNullOrWhiteSpace(text) && !VsHelpers.IsSending)
            {
                VsHelpers.IsSending = true;
                SendButton.Content = "Answering...";
                SendButton.IsEnabled = false;
                CancelButton.IsEnabled = true;

                AddMessage(role, text);

                ScrollToBottom();

                _cancelTokenSource = new CancellationTokenSource();

                try
                {
                    await Task.Run(async () =>
                    {
                        this.CreateLlmChatClientIf();

                        if (this._apiKind == LlmApiKind.OpenAI && this.ChatOpenAI != null)
                        {
                            // Use OpenAI client
                            _cancelTokenSource.CancelAfter(LlmTimeoutOpenAI); // 设置超时时间为30秒

                            // Convert MyMessage to ChatMessage

                            bool seenUserMessage = false;
                            var openAIChatMessages = new List<ChatMessage>();

                            foreach (MyMessage msg in this._messages)
                            {
                                ChatMessage chatMessage;

                                if (msg.Role == ChatRole.System)
                                {
                                    chatMessage = new SystemChatMessage(msg.Content);
                                }
                                else if (msg.Role == ChatRole.Assistant)
                                {
                                    chatMessage = new AssistantChatMessage(msg.Content);
                                }
                                else
                                {
                                    chatMessage = new UserChatMessage(msg.Content);
                                    seenUserMessage = true;
                                }
                                
                                openAIChatMessages.Add(chatMessage);
                            }

                            if (openAIChatMessages.Count == 0)
                            {
                                throw new ApplicationException("Why openAIChatMessages.Count is zeo?");
                            }

                            //
                            // Some models don't like it than there is no USER input in the prompt.
                            //
                            if (!seenUserMessage)
                            {
                                openAIChatMessages.Add(new UserChatMessage("."));
                            }

                            var streamingResponse = this.ChatOpenAI.CompleteChatStreamingAsync(openAIChatMessages, null, _cancelTokenSource.Token);
                            var asyncEnumerator = streamingResponse.GetAsyncEnumerator(_cancelTokenSource.Token);

                            try
                            {
                                while (await asyncEnumerator.MoveNextAsync())
                                {
                                    StreamingChatCompletionUpdate update = asyncEnumerator.Current;
                                    
                                    if (update.ContentUpdate?.Count > 0)
                                    {
                                        _cancelTokenSource.CancelAfter(LlmTimeoutOpenAI);

                                        var chunkText = new System.Text.StringBuilder();

                                        for (int i = 0; i < update.ContentUpdate.Count; i++)
                                        {
                                            chunkText.Append(update.ContentUpdate[i].Text);
                                        }
                                        
                                        this.OnChatResponseReceivedOpenAI(
                                            new LlmChatResponseChunk
                                            {
                                                Message = new LlmMessage { Role = "assistant", Content = chunkText.ToString() },
                                                Done = false
                                            });
                                    }
                                }
                            }
                            finally
                            {
                                await asyncEnumerator.DisposeAsync();
                            }

                            this.OnChatResponseReceivedOpenAI(
                                new LlmChatResponseChunk
                                {
                                    Message = new LlmMessage { Role = "assistant", Content = string.Empty },
                                    Done = true
                                });
                        }
                        else
                        {
                            // Use Ollama client
                            if (ChatOllama != null)
                            {
                                _cancelTokenSource.CancelAfter(LlmTimeoutOllama);

                                await ChatOllama.Send(text, _cancelTokenSource.Token);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    if (!_cancelTokenSource.Token.IsCancellationRequested)
                    {
                        this.OnChatResponseReceivedString("[Exception occured. See Documents\\LLMCopilot*.log]", true);
                    }
                    else
                    {
                        this.OnChatResponseReceivedString("[Operation timed out]", true);
                    }

                    // 其他异常处理
                    LLMErrorHandler.HandleException(ex);
                }

                // 确保在任何情况下都重置状态
                VsHelpers.IsSending = false;
                SendButton.Content = "Send";
                SendButton.IsEnabled = true;
                CancelButton.IsEnabled = false;

                ScrollToBottom();
            }
        }

        /// <summary>
        /// Callback for OpenAI chat streaming responses.
        /// </summary>
        private void OnChatResponseReceivedOpenAI(LlmChatResponseChunk response)
        {
            this.OnChatResponseReceivedString(response.Message.Content, response.Done);
        }

        private async Task SendMessageAsync()
        {
            string text = MessageTextBox.Text;
            MessageTextBox.Clear();
            SendChatMessageAsync(text, ChatRole.User);
        }


        private void MessagesScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancelTokenSource?.Cancel();
        }
    }

    public class CapitalizeFirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChatRole role)
            {
                string roleString = role.ToString();
                return char.ToUpper(roleString[0]) + roleString.Substring(1);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
