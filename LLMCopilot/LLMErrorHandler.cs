using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.IO;

namespace LLMCopilot
{
    public static class LogHelper
    {
        public static void Log([CallerMemberName] string memberName = "")
        {
            LLMErrorHandler.WriteLog(memberName);
        }
    }

    public static class LLMErrorHandler
    {
        private static IServiceProvider serviceProvider;

        public static string FormatException(Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Exception Type: {ex.GetType().FullName}");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Source: {ex.Source}");
            sb.AppendLine($"TargetSite: {ex.TargetSite}");
            sb.AppendLine("Data:");
            foreach (var key in ex.Data.Keys)
            {
                sb.AppendLine($"{key}: {ex.Data[key]}");
            }
            sb.AppendLine($"StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                sb.AppendLine("---- Inner Exception ----");
                sb.AppendLine(FormatException(ex.InnerException)); // Recursively append the inner exception
            }

            return sb.ToString();
        }

        public static void Initialize(IServiceProvider provider)
        {
            serviceProvider = provider;
        }

        public static void HandleException(Exception exception)
        {
            var message = $"An exception occurred: {FormatException(exception)}\n";

            //// 弹出对话框
            //VsShellUtilities.ShowMessageBox(
            //    serviceProvider,
            //    message,
            //    "错误",
            //    OLEMSGICON.OLEMSGICON_CRITICAL,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

            LLMErrorHandler.WriteLog(message);
        }

        public static void WriteLog(string log)
        {
            // 格式化文件名以包含日期
            string logFileName = $"LLMCopilot_{DateTime.Now:yyyy-MM-dd}.log";
            string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), logFileName);
            log += $"---------{DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n";
            // 写入文件
            File.AppendAllText(logFilePath, log);
        }
    }
}
