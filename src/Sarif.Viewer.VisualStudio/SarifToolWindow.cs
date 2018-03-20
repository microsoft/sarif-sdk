// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Runtime.InteropServices;
using Microsoft.Sarif.Viewer.Views;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("ab561bcc-e01d-4781-8c2e-95a9170bfdd5")]
    public class SarifToolWindow : ToolWindowPane, IToolWindow
    {
        private ITrackSelection _trackSelection;
        private SelectionContainer _selectionContainer;

        internal SarifToolWindowControl Control
        {
            get
            {
                return Content as SarifToolWindowControl;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SarifToolWindow"/> class.
        /// </summary>
        public SarifToolWindow() : base(null)
        {
            this.Caption = "SARIF Explorer";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            Content = new SarifToolWindowControl();
            Control.DataContext = null;

            ResetSelection();
        }

        public void Show()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ((IVsWindowFrame)Frame).Show();
        }

        private ITrackSelection TrackSelection
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (_trackSelection == null)
                {
                    _trackSelection = GetService(typeof(STrackSelection)) as ITrackSelection;
                }

                return _trackSelection;
            }
        }

        /// <summary>
        /// Updates the Properties window with the public properties of the selection objects.
        /// </summary>
        public void ApplySelection()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ITrackSelection track = TrackSelection;
            if (track != null)
            {
                track.OnSelectChange((ISelectionContainer)_selectionContainer);
            }
        }

        /// <summary>
        /// Replaces the collection of objects who's values are displayed in the Properties window.
        /// </summary>
        /// <param name="items"></param>
        public void UpdateSelectionList(params object[] items)
        {
            _selectionContainer = new SelectionContainer(true, false);
            _selectionContainer.SelectableObjects = items;
            _selectionContainer.SelectedObjects = items;
            ApplySelection();
        }

        /// <summary>
        /// Reset the Properties window to display the properties of the selected Error List item.
        /// </summary>
        public void ResetSelection()
        {
            UpdateSelectionList(Control?.DataContext);
        }
    }
}
