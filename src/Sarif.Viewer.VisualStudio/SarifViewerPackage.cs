// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

using EnvDTE;
using EnvDTE80;

using Microsoft.VisualStudio.Shell;

namespace SarifViewer
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(SarifViewerPackage.PackageGuidString)]
    public sealed class SarifViewerPackage : Package
    {
        public static DTE2 Dte;

        /// <summary>
        /// OpenSarifFileCommandPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "b97edb99-282e-444c-8f53-7de237f2ec5e";

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSarifFileCommand"/> class.
        /// </summary>
        public SarifViewerPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.

            Dte = GetGlobalService(typeof(DTE)) as DTE2;
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            OpenSarifFileCommand.Initialize(this);
            base.Initialize();
        }

        #endregion
    }
}
