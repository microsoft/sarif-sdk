using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Sarif.Viewer.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Sarif.Viewer.LoadServiceSample
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(VSPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : AsyncPackage
    {
        /// <summary>
        /// VSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "d709e9c6-9018-4c20-a6ba-1180987c0d41";

        /// <summary>
        /// Initializes a new instance of the <see cref="VSPackage"/> class.
        /// </summary>
        public VSPackage()
        {
        }

        /// <summary>
        /// Opens the specified SARIF log file in the SARIF Viewer extension.
        /// </summary>
        /// <param name="path">The path of the log file.</param>
        internal async Task OpenSarifLog(string path)
        {
            IVsShell shell = GetService(typeof(SVsShell)) as IVsShell;

            if (shell != null)
            {
                SarifViewerInterop sarifViewerInterop = new SarifViewerInterop(shell);

                if (!sarifViewerInterop.IsViewerExtensionInstalled)
                {
                    VsShellUtilities.ShowMessageBox(
                            this,
                            "The SARIF Viewer extension is not installed.",
                            null,
                            OLEMSGICON.OLEMSGICON_INFO,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }

                if (!sarifViewerInterop.IsViewerExtensionLoaded)
                {
                    sarifViewerInterop.LoadViewerExtension();
                }

                await sarifViewerInterop.OpenSarifLogAsync(path);
            }
        }

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
            await OpenSarifLogCommand.InitializeAsync(this);
        }

        #endregion
    }
}
