// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Sarif.Viewer.Interop
{
    public class SarifViewerInterop
    {
        private static readonly string ViewerAssemblyFileName = "Microsoft.Sarif.Viewer";
        private static readonly string ViewerServiceInterfaceName = "SLoadSarifLogService";
        private bool? _isViewerExtensionInstalled;
        private bool? _isViewerExtensionLoaded;
        private Assembly _viewerExtensionAssembly;
        private AssemblyName _viewerExtensionAssemblyName;
        private Version _viewerExtensionVersion;

        /// <summary>
        /// Gets the Visual Studio shell instance object.
        /// </summary>
        public IVsShell VsShell { get; private set; }

        /// <summary>
        /// Gets the unique identifier string of the SARIF Viewer package.
        /// </summary>
        public static readonly string ViewerExtensionGuidString = "b97edb99-282e-444c-8f53-7de237f2ec5e";

        /// <summary>
        /// Gets the unique identifier of the SARIF Viewer package.
        /// </summary>
        public static readonly Guid ViewerExtensionGuid = new Guid(ViewerExtensionGuidString);

        #region Private properties for lazy initialization
        private Assembly ViewerExtensionAssembly
        {
            get
            {
                if (_viewerExtensionAssembly == null)
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    _viewerExtensionAssembly = assemblies.Where(a => a.GetName().Name == ViewerAssemblyFileName).FirstOrDefault();
                }

                return _viewerExtensionAssembly;
            }
        }

        private AssemblyName ViewerExtensionAssemblyName
        {
            get
            {
                return _viewerExtensionAssemblyName ?? (_viewerExtensionAssemblyName = ViewerExtensionAssembly.GetName());
            }
        }

        private Version ViewerExtensionVersion
        {
            get
            {
                return _viewerExtensionVersion ?? (_viewerExtensionVersion = ViewerExtensionAssemblyName.Version);
            }
        }
        #endregion

        private SarifViewerInterop() { }

        /// <summary>
        /// Initializes a new instance of the SarifViewerInterop class.
        /// </summary>
        /// <param name="vsShell"></param>
        public SarifViewerInterop(IVsShell vsShell)
        {
            VsShell = vsShell ?? throw new ArgumentNullException(nameof(vsShell));
        }

        /// <summary>
        /// Gets a value indicating whether the SARIF Viewer extension is installed.
        /// </summary>
        public bool IsViewerExtensionInstalled
        {
            get
            {
                return _isViewerExtensionInstalled ?? (bool)(_isViewerExtensionInstalled = IsExtensionInstalled());
            }
        }

        /// <summary>
        /// Gets a value indicating whether the SARIF Viewer extension is loaded.
        /// </summary>
        public bool IsViewerExtensionLoaded
        {
            get
            {
                return _isViewerExtensionLoaded ?? (bool)(_isViewerExtensionLoaded = IsExtensionLoaded());
            }
        }

        /// <summary>
        /// Opens the specified SARIF log file in the SARIF Viewer extension.
        /// </summary>
        /// <param name="path">The path of the log file.</param>
        public async Task OpenSarifLogAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!IsViewerExtensionInstalled)
            {
                return;
            }

            if (!IsViewerExtensionLoaded)
            {
                LoadViewerExtension();
            }

            // Get the service interface type
            Type[] types = ViewerExtensionAssembly.GetTypes();
            Type sarifLoadServiceInterface = types.Where(t => t.Name == ViewerServiceInterfaceName).FirstOrDefault();

            if (sarifLoadServiceInterface != null)
            {
                // Get a service reference
                dynamic sarifLoadService = await ServiceProvider.GetGlobalServiceAsync(sarifLoadServiceInterface);

                if (sarifLoadService != null)
                {
                    // Call the service API
                    sarifLoadService.LoadSarifLog(path);
                }
            }
        }

        /// <summary>
        /// Loads the SARIF Viewer extension.
        /// </summary>
        /// <returns>The extension package that has been loaded.</returns>
        public IVsPackage LoadViewerExtension()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Guid serviceGuid = ViewerExtensionGuid;
            IVsPackage package = null; ;

            if (IsViewerExtensionInstalled)
            {
                VsShell.LoadPackage(ref serviceGuid, out package);
            }

            return package;
        }

        private bool IsExtensionInstalled()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Guid serviceGuid = ViewerExtensionGuid;
            int result;

            if (0 == VsShell.IsPackageInstalled(ref serviceGuid, out result) && result == 1)
            {
                return true;
            }

            return false;
        }

        private bool IsExtensionLoaded()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Guid serviceGuid = ViewerExtensionGuid;
            IVsPackage package;

            if (0 == VsShell.IsPackageLoaded(ref serviceGuid, out package) && package != null)
            {
                return true;
            }

            return false;
        }
    }
}
