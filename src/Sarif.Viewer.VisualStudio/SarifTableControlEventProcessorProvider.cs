// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Sarif.Viewer
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
                DeselectItems(snapshot);

                SarifError sarifError = sarifSnapshot.GetItem(index);

                IVsWindowFrame frame;
                if (!CodeAnalysisResultManager.Instance.TryNavigateTo(sarifError, out frame))
                {
                    return;
                }

                if (sarifError.HasDetails)
                {
                    OpenOrReplaceVerticalContent(frame, sarifError);
                }
                else
                {
                    // TODO
                    //CloseVerticalContent(frame, error);
                }
            }

            private void OpenOrReplaceVerticalContent(IVsWindowFrame frame, SarifError error)
            {
                IVsTextView textView = GetTextViewFromFrame(frame);
                if (textView == null)
                {
                    return;
                }

                CodeLocations codeLocations = new CodeLocations();
                codeLocations.SetItems(error.Annotations);

                // TODO remove
                var type = textView.GetType();
                var mi = type.GetMethod(
                    "ShowAdditionalContent",
                    new Type[] { typeof(FrameworkElement), typeof(String)});
                 
                mi.Invoke(textView, new object[] { codeLocations, "Code Analysis Details" });
            }

            private IVsTextView GetTextViewFromFrame(IVsWindowFrame frame)
            {
                // Get the document view from the window frame, then get the text view
                object docView;
                int hr = frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView);
                if (hr != 0 || docView == null)
                {
                    return null;
                }

                IVsCodeWindow codeWindow = docView as IVsCodeWindow;
                IVsTextView textView;
                codeWindow.GetLastActiveView(out textView);
                if (textView == null)
                {
                    codeWindow.GetPrimaryView(out textView);
                }

                return textView;
            }

            private void SelectItem(SarifError item)
            {
                // TODO
            }

            private void DeselectItems(ITableEntriesSnapshot snapshot)
            {
                // TODO
            }

            public override void PreprocessMouseRightButtonUp(ITableEntryHandle entry, MouseButtonEventArgs e)
            {
                base.PreprocessMouseRightButtonUp(entry, e);
            }
        }
    }
}