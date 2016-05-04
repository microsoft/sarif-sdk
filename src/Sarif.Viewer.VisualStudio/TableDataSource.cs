// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.Sarif.Viewer
{
    class SarifTableDataSource : ITableDataSource
    {
        private static SarifTableDataSource _instance;
        private readonly List<SinkManager> _managers = new List<SinkManager>();
        private static Dictionary<string, SarifSnapshot> _snapshots = new Dictionary<string, SarifSnapshot>();

        [Import]
        private ITableManagerProvider TableManagerProvider { get; set; } = null;


        [ImportMany]
        IEnumerable<ITableControlEventProcessorProvider> TableControlEventProcessorProviders { get; set; } = null;

        private SarifTableDataSource()
        {           
            var compositionService = ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            compositionService.DefaultCompositionService.SatisfyImportsOnce(this);

            if (TableManagerProvider == null)
            {
                TableManagerProvider = compositionService.GetService<ITableManagerProvider>();
            }

            if (TableControlEventProcessorProviders == null)
            {
                TableControlEventProcessorProviders = new[] { compositionService.GetService<ITableControlEventProcessorProvider>() };
            }

            var manager = TableManagerProvider.GetTableManager(StandardTables.ErrorsTable);
            manager.AddSource(this, StandardTableColumnDefinitions.DetailsExpander,
                                    StandardTableColumnDefinitions.ErrorSeverity, StandardTableColumnDefinitions.ErrorCode,
                                    StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.BuildTool,
                                    StandardTableColumnDefinitions.ErrorRank, StandardTableColumnDefinitions.ErrorCategory,
                                    StandardTableColumnDefinitions.Text, StandardTableColumnDefinitions.DocumentName, 
                                    StandardTableColumnDefinitions.Line, StandardTableColumnDefinitions.Column);


//            var errorList = ServiceProvider.GlobalProvider.GetService(typeof(SVsErrorList)) as IVsErrorList;

        }

        public static SarifTableDataSource Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SarifTableDataSource();

                return _instance;
            }
        }

        #region ITableDataSource members
        public string SourceTypeIdentifier
        {
            get { return StandardTableDataSources.ErrorTableDataSource; }
        }

        public string Identifier
        {
            get { return Guids.GuidVSPackageString; }
        }

        public string DisplayName
        {
            get { return Constants.VSIX_NAME; }
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            return new SinkManager(this, sink);
        }
        #endregion

        public void AddSinkManager(SinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (_managers)
            {
                _managers.Add(manager);
            }
        }

        public void RemoveSinkManager(SinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (_managers)
            {
                _managers.Remove(manager);
            }
        }

        public void UpdateAllSinks()
        {
            lock (_managers)
            {
                foreach (var manager in _managers)
                {
                    manager.UpdateSink(_snapshots.Values);
                }
            }
        }

        public void AddErrors(IEnumerable<SarifError> errors)
        {
            if (errors == null || !errors.Any())
                return;

            CleanAllErrors();

            var cleanErrors = errors.Where(e => e != null && !string.IsNullOrEmpty(e.FileName));

            foreach (var error in cleanErrors.GroupBy(t => t.FileName))
            {
                var snapshot = new SarifSnapshot(error.Key, error);
                _snapshots[error.Key] = snapshot;
            }

            UpdateAllSinks();
        }

        public void CleanErrors(IEnumerable<string> files)
        {
            foreach (string file in files)
            {
                if (_snapshots.ContainsKey(file))
                {
                    _snapshots[file].Dispose();
                    _snapshots.Remove(file);
                }
            }

            lock (_managers)
            {
                foreach (var manager in _managers)
                {
                    manager.RemoveSnapshots(files);
                }
            }

            UpdateAllSinks();
        }

        public void CleanAllErrors()
        {
            foreach (string file in _snapshots.Keys)
            {
                var snapshot = _snapshots[file];
                if (snapshot != null)
                {
                    snapshot.Dispose();
                }
            }

            _snapshots.Clear();

            lock (_managers)
            {
                foreach (var manager in _managers)
                {
                    manager.Clear();
                }
            }
        }

        public void BringToFront()
        {
            SarifViewerPackage.Dte.ExecuteCommand("View.ErrorList");        
        }

        public bool HasErrors()
        {
            return _snapshots.Count > 0;
        }

        public bool HasErrors(string fileName)
        {
            return _snapshots.ContainsKey(fileName);
        }
    }
}
