using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace LLMCopilot
{
    public static class LLMCopilotProvider
    {
        public static async Task EnsurePackageLoadedAsync()
        {
            if (Package != null)
            {
                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var shell = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsShell)) as IVsShell;
            if (shell != null)
            {
                var packageGuid = new Guid(LLMCopilotPackage.PackageGuidString);
                shell.LoadPackage(ref packageGuid, out IVsPackage package);
            }
        }

        public static AsyncPackage Package { get; set; }
    }

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(LLMCopilotPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(LLMChatWindow))]
    [ProvideOptionPage(typeof(OptionPageGrid),
    "LLMCopilot", "General", 0, 0, true)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]  // 当没有打开解决方案时也加载包
    public sealed class LLMCopilotPackage : AsyncPackage
    {
        /// <summary>
        /// LLMCopilotPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "f416f114-4489-4a9e-bc3a-fd1afc259ce9";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            LLMCopilotProvider.Package = this;
            await OllamaHelper.Instance.InitModelCtx();
            await ExplainCommand.InitializeAsync(this);
            await LLMChatWindowCommand.InitializeAsync(this);
            await CodeCompleteCommand.InitializeAsync(this);
            await FindBugCommand.InitializeAsync(this);
            await OptimizeCodeCommand.InitializeAsync(this);
            await AddCommentCommand.InitializeAsync(this);
            await UnitTestCommand.InitializeAsync(this);
            await SettingsCommand.InitializeAsync(this);
        }

        #endregion
    }
}
