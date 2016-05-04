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
        private static readonly ConditionalWeakTable<IVsTextView, StrongBox<int>> cookieMap =
            new ConditionalWeakTable<IVsTextView, StrongBox<int>>();

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
                    //return;
                }

                if (sarifError.HasDetails)
                {
                    OpenOrReplaceVerticalContent(frame, sarifError);
                }
                else
                {
                    CloseVerticalContent(frame, sarifError);
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
                codeLocations.CurrentSarifError = error;

                // TODO: this needs to be a public API
                var type = textView.GetType();
                var mi = type.GetMethod(
                    "ShowAdditionalContent",
                    new Type[] { typeof(FrameworkElement), typeof(String)});
                 
                int cookie = (int)mi.Invoke(textView, new object[] { codeLocations, "Code Analysis Details" });

                cookieMap.Remove(textView);
                cookieMap.Add(textView, new StrongBox<int>(cookie));
            }

            private void CloseVerticalContent(IVsWindowFrame frame, SarifError item)
            {
                IVsTextView textView = GetTextViewFromFrame(frame);
                if (textView == null)
                {
                    return;
                }

                StrongBox<int> cookie;
                if (cookieMap.TryGetValue(textView, out cookie))
                {
                    // TODO: this needs to be a public API
                    var type = textView.GetType();
                    var mi = type.GetMethod(
                        "HideVerticalContent",
                        new Type[] { typeof(int) });

                    mi.Invoke(textView, new object[] { cookie.Value });

                    cookieMap.Remove(textView);
                }
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