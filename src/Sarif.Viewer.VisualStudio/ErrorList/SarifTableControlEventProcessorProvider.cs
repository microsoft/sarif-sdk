// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.ComponentModel.Composition;
using System.Linq;
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
using System.Windows.Controls;

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

            public override void PreprocessSelectionChanged(TableSelectionChangedEventArgs e)
            {
                SarifErrorListItem sarifResult;
                ListView errorList = (ListView)e.SelectionChangedEventArgs.Source;

                if (errorList.SelectedItems.Count != 1)
                {
                    // There's more, or less, than one selected item. Clear the SARIF Explorer.
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = null;
                    return;
                }

                ITableEntryHandle entryHandle = errorList.SelectedItems[0] as ITableEntryHandle;

                if (!TryGetSarifResult(entryHandle, out sarifResult))
                {
                    // The selected item is not a SARIF result. Clear the SARIF Explorer.
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = null;
                    return;
                }

                // Navigate to the source file of the first location for the defect.
                if (sarifResult.Locations?.Count > 0)
                {
                    sarifResult.Locations[0].OnSelectKeyEvent();
                }

                if (sarifResult.HasDetails)
                {
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = sarifResult;
                }
                else
                {
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = null;
                }

                base.PreprocessSelectionChanged(e);
            }

            /// <summary>
            /// Handles the double-click Error List event.
            /// Displays the SARIF Explorer tool window. 
            /// Does not bind the selected item to the Tool Window. The binding is done by PreprocessSelectionChanged.
            /// </summary>
            public override void PreprocessNavigate(ITableEntryHandle entryHandle, TableEntryNavigateEventArgs e)
            {
                SarifErrorListItem sarifResult;

                if (!TryGetSarifResult(entryHandle, out sarifResult))
                {
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = null;
                    return;
                }

                e.Handled = true;

                if (sarifResult.HasDetails)
                {
                    SarifViewerPackage.SarifToolWindow.Show();
                }
            }

            bool TryGetSarifResult(ITableEntryHandle entryHandle, out SarifErrorListItem sarifResult)
            {
                ITableEntriesSnapshot entrySnapshot;
                int entryIndex;
                sarifResult = default(SarifErrorListItem);

                if (entryHandle.TryGetSnapshot(out entrySnapshot, out entryIndex))
                {
                    var snapshot = entrySnapshot as SarifSnapshot;

                    if (snapshot != null)
                    {
                        sarifResult = snapshot.GetItem(entryIndex);
                    }
                }

                return sarifResult != null;
            }
        }
    }
}