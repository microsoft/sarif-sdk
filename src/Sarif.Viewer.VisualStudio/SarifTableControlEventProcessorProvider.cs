// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Utilities;

namespace SarifViewer
{
    [Export(typeof(ITableControlEventProcessorProvider))]
    [ManagerType(StandardTables.ErrorsTable)]
    [DataSourceType(StandardTableDataSources.ErrorTableDataSource)]
    [DataSource(Guids.guidVSPackageString)]
    [Name(Name)]
    [Order(Before = "Default")]
    public class SarifTableControlEventProcessorProvider : ITableControlEventProcessorProvider
    {
        internal const string Name = "Sarif Table Event Processor";

        public SarifTableControlEventProcessorProvider()
        {
            Console.WriteLine("victory");
        }

        public ITableControlEventProcessor GetAssociatedEventProcessor(IWpfTableControl tableControl)
        {
            return new EventProcessor();
        }

        private class EventProcessor : TableControlEventProcessorBase
        {
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
                sarifSnapshot.TryNavigateTo(index, e.IsPreview);
            }
        }
    }
}
