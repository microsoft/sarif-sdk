// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Utilities;

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

            /// <summary>
            /// Handles the single-click Error List event.
            /// Binds the selected item to the Tool Window. 
            /// Does not show the tool window if it is not already open. Displaying of the tool window is handed by PreprocessNavigate.
            /// </summary>
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

                // Set the current sarif error in the manager so we track code locations.
                CodeAnalysisResultManager.Instance.CurrentSarifResult = sarifResult;

                if (sarifResult.HasDetails)
                {
                    // Setting the DataContext to be null first forces the TabControl to select the appropriate tab.
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = null;
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = sarifResult;
                }
                else
                {
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = null;
                }

                if (sarifResult.Locations?.Count > 0)
                {
                    sarifResult.Locations[0].ApplyDefaultSourceFileHighlighting();
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

                // Navigate to the source file of the first location for the defect.
                if (sarifResult.Locations?.Count > 0)
                {
                    sarifResult.Locations[0].NavigateTo(false);
                    sarifResult.Locations[0].ApplyDefaultSourceFileHighlighting();
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