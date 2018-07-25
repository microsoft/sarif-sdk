// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.ComponentModel.Design;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.Sarif.Viewer
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
    [Guid(SarifViewerPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideEditorExtension(typeof(SarifEditorFactory), ".sarif", 128)]
    [ProvideToolWindow(typeof(SarifToolWindow), Style = VsDockStyle.Tabbed, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
    [ProvideService(typeof(SLoadSarifLogService))]
    public sealed class SarifViewerPackage : Package
    {
        public static DTE2 Dte;
        public static IServiceProvider ServiceProvider;

        private SarifEditorFactory _sarifEditorFactory;

        /// <summary>
        /// OpenSarifFileCommandPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "b97edb99-282e-444c-8f53-7de237f2ec5e";

        public static bool IsUnitTesting { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenLogFileCommands"/> class.
        /// </summary>
        public SarifViewerPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.

            Dte = GetGlobalService(typeof(DTE)) as DTE2;
            ServiceProvider = this;
        }

        /// <summary>
        /// Returns the instance of the SARIF tool window.
        /// </summary>
        public static SarifToolWindow SarifToolWindow
        {
            get
            {
                SarifViewerPackage package = SarifViewerPackage.ServiceProvider as SarifViewerPackage;

                SarifToolWindow toolWindow = package?.FindToolWindow(typeof(SarifToolWindow), 0, true) as SarifToolWindow;

                return toolWindow;
            }
        }

        public static System.Configuration.Configuration AppConfig { get; private set; }

        public T GetService<S, T>()
            where S : class
            where T : class
        {
            try
            {
                return (T)this.GetService(typeof(S));
            }
            catch (Exception)
            {
                // If anything went wrong, just ignore it
            }
            return null;
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            OpenLogFileCommands.Initialize(this);
            base.Initialize();

            ServiceCreatorCallback callback = new ServiceCreatorCallback(CreateService);
            ((IServiceContainer)this).AddService(typeof(SLoadSarifLogService), callback, true);

            string path = Assembly.GetExecutingAssembly().Location;
            var configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = Path.Combine(Path.GetDirectoryName(path), "App.config");
            AppConfig = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

#if DEBUG
            string telemetryKey = SarifViewerPackage.AppConfig.AppSettings.Settings["TelemetryInstrumentationKey_Debug"].Value;
#else
            string telemetryKey = SarifViewerPackage.AppConfig.AppSettings.Settings["TelemetryInstrumentationKey_Release"].Value;
#endif

            TelemetryConfiguration configuration = new TelemetryConfiguration()
            {
                InstrumentationKey = telemetryKey
            };
            TelemetryProvider.Initialize(configuration);
            TelemetryProvider.WriteEvent(TelemetryEvent.ViewerExtensionLoaded);

            _sarifEditorFactory = new SarifEditorFactory();
            RegisterEditorFactory(_sarifEditorFactory);

            CodeAnalysisResultManager.Instance.Register();
            SarifToolWindowCommand.Initialize(this);
            ErrorList.ErrorListCommand.Initialize(this);
        }

        #endregion

        private object CreateService(IServiceContainer container, Type serviceType)
        {
            return (typeof(SLoadSarifLogService) == serviceType) ? new LoadSarifLogService() : null;
        }
    }
}
