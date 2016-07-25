// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Sarif.Viewer.Views;
using Microsoft.Sarif.Viewer.ViewModels;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.ErrorList
{
    [Export(typeof(ITableControlEventProcessorProvider))]
    [ManagerType(StandardTables.ErrorsTable)]
    [DataSourceType(StandardTableDataSources.ErrorTableDataSource)]
    [DataSource(Guids.GuidVSPackageString)]
    [Name(Name)]
    [Order(Before = "Default")]
    public class SarifTableControlEventProcessorProvider : ITableControlEventProcessorProvider
    {
        internal const string Name = "Sarif Table Event Processor";

        public SarifTableControlEventProcessorProvider()
        {
        }

        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        public ITableControlEventProcessor GetAssociatedEventProcessor(IWpfTableControl tableControl)
        {
            return new EventProcessor() { EditorAdaptersFactoryService = EditorAdaptersFactoryService };
        }

        private class EventProcessor : TableControlEventProcessorBase
        {
            public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

            public override void PreprocessNavigate(ITableEntryHandle entryHandle, TableEntryNavigateEventArgs e)
            {
                int index;
                ITableEntriesSnapshot snapshot;
                if (!entryHandle.TryGetSnapshot(out snapshot, out index))
                {
                    return;
                }

                var sarifSnapshot = snapshot as SarifSnapshot;
                if (sarifSnapshot == null)
                {
                    return;
                }

                e.Handled = true;

                SarifErrorListItem sarifError = sarifSnapshot.GetItem(index);

                // Navigate to the source file of the first location for the defect.
                if (sarifError.Locations != null && sarifError.Locations.Count > 0)
                {
                    sarifError.Locations[0].OnSelectKeyEvent();
                }

                if (sarifError.HasDetails)
                {
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = sarifError;
                }
                else
                {
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = null;
                }
            }
        }
    }
}