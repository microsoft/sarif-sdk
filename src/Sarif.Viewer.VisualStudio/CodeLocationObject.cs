// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.Sarif.Viewer
{
    public abstract class CodeLocationObject : NotifyPropertyChangedObject
    {
        private Region _region;
        protected ResultTextMarker _lineMarker;
        protected string _filePath;
        protected string _uriBaseId;

        internal virtual ResultTextMarker LineMarker
        {
            get
            {
                // Not all locations have regions. Don't try to mark the locations that don't.
                if (_lineMarker == null && Region != null)
                {
                    _lineMarker = new ResultTextMarker(SarifViewerPackage.ServiceProvider, Region, FilePath);
                }

                return _lineMarker;
            }
            set
            {
                _lineMarker = value;
            }
        }

        public Region Region
        {
            get
            {
                return _region;
            }
            set
            {
                if (value != _region)
                {
                    _region = value;
                    NotifyPropertyChanged("Region");
                }
            }
        }

        public virtual string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                if (value != _filePath)
                {
                    _filePath = value;

                    if (this.LineMarker != null)
                    {
                        this.LineMarker.FullFilePath = _filePath;
                    }

                    NotifyPropertyChanged("FilePath");
                }
            }
        }

        internal virtual string UriBaseId
        {
            get
            {
                return _uriBaseId;
            }
            set
            {
                if (value != _uriBaseId)
                {
                    _uriBaseId = value;

                    if (this.LineMarker != null)
                    {
                        this.LineMarker.UriBaseId = _uriBaseId;
                    }

                    NotifyPropertyChanged("UriBaseId");
                }
            }
        }

        public virtual string DefaultSourceHighlightColor
        {
            get
            {
                return ResultTextMarker.DEFAULT_SELECTION_COLOR;
            }
        }

        public virtual string SelectedSourceHighlightColor
        {
            get
            {
                return ResultTextMarker.DEFAULT_SELECTION_COLOR;
            }
        }

        // This is a custom type descriptor which enables the SARIF properties
        // to be displayed in the Properties window.
        internal ICustomTypeDescriptor TypeDescriptor
        {
            get
            {
                return new CodeLocationObjectTypeDescriptor(this);
            }
        }

        public void NavigateTo(bool usePreviewPane = true)
        {
            LineMarker?.NavigateTo(usePreviewPane);
        }

        public void ApplyDefaultSourceFileHighlighting()
        {
            // Remove hover marker
            LineMarker?.RemoveHighlightMarker();

            // Add default marker instead
            LineMarker?.AddHighlightMarker(DefaultSourceHighlightColor);
        }

        /// <summary>
        /// A method for handling the event when this object is selected
        /// </summary>
        public void ApplySelectionSourceFileHighlighting()
        {
            // Remove previous highlighting and replace with hover color
            LineMarker?.RemoveHighlightMarker();
            LineMarker?.AddHighlightMarker(SelectedSourceHighlightColor);
        }

        private IVsTextView GetTextViewFromFrame(IVsWindowFrame frame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the document view from the window frame, then get the text view
            object docView;
            int hr = frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out docView);
            if ((hr != 0 && hr != 1) || docView == null)
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

        /// <summary>
        /// An overridden method for reacting to the event of a document window
        /// being opened
        /// </summary>
        internal void AttachToDocument(string documentName, long docCookie, IVsWindowFrame frame)
        {
            // For these cases, this event has nothing to do with this item
            if (frame == null || LineMarker.IsTracking(docCookie) || string.Compare(documentName, FilePath, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return;
            }

            AttachToDocumentWorker(frame, docCookie, LineMarker);
        }


        /// <summary>
        /// Check that current <paramref name="marker"/> point to correct line position 
        /// and attach it to <paramref name="docCookie"/> for track changes.
        /// </summary>
        private void AttachToDocumentWorker(IVsWindowFrame frame, long docCookie, ResultTextMarker marker)
        {
            var sourceLocation = marker.GetSourceLocation();
            int line = sourceLocation.StartLine;

            // Coerce the line numbers so we don't go out of bound. However, if we have to
            // coerce the line numbers, then we won't perform highlighting because most likely
            // we will highlight the wrong line. The idea here is to just go to the top or bottom
            // of the file as our "best effort" to be closest where it thinks it should be
            if (line <= 0)
            {
                line = 1;
            }

            IVsTextView textView = GetTextViewFromFrame(frame);
            if (textView != null)
            {
                // Locate the specific line/column position in the text view and go there
                IVsTextLines textLines;
                textView.GetBuffer(out textLines);
                if (textLines != null)
                {
                    int lastLine;
                    int length;
                    int hr = textLines.GetLastLineIndex(out lastLine, out length);
                    if (hr != 0)
                    {
                        return;
                    }

                    // our source code lines are 1-based, and the VS API source code lines are 0-based

                    lastLine = lastLine + 1;

                    // Same thing here, coerce the line number if it's going out of bound
                    if (line > lastLine)
                    {
                        line = lastLine;
                    }
                }

                // Call a bunch of functions to get the WPF text view so we can perform the highlighting only
                // if we haven't yet
                IWpfTextView wpfTextView = GetWpfTextView(textView);
                if (wpfTextView != null)
                {
                    AttachMarkerToTextView(wpfTextView, docCookie, marker,
                        line, sourceLocation.StartColumn, line + (sourceLocation.EndLine - sourceLocation.StartLine), sourceLocation.EndColumn);
                }
            }
        }

        /// <summary>
        /// Helper method for getting a IWpfTextView from a IVsTextView object
        /// </summary>
        /// <param name="textView"></param>
        /// <returns></returns>
        private IWpfTextView GetWpfTextView(IVsTextView textView)
        {
            IWpfTextViewHost textViewHost = null;
            IVsUserData userData = textView as IVsUserData;
            if (userData != null)
            {
                Guid guid = Microsoft.VisualStudio.Editor.DefGuidList.guidIWpfTextViewHost;
                object wpfTextViewHost = null;
                userData.GetData(ref guid, out wpfTextViewHost);
                textViewHost = wpfTextViewHost as IWpfTextViewHost;
            }

            if (textViewHost == null)
            {
                return null;
            }
            return textViewHost.TextView;
        }

        /// <summary>
        /// Highlight the source code on a particular line
        /// </summary>
        private static void AttachMarkerToTextView(IWpfTextView textView, long docCookie, ResultTextMarker marker,
            int line, int column, int endLine, int endColumn)
        {
            // If for some reason the start line is not correct, just skip the highlighting
            ITextSnapshot textSnapshot = textView.TextSnapshot;
            if (line > textSnapshot.LineCount)
            {
                return;
            }

            Span spanToColor;
            int markerStart, markerEnd = 0;

            try
            {
                // Fix up the end line number if it's inconsistent
                if (endLine <= 0 || endLine < line)
                {
                    endLine = line;
                }

                bool coerced = false;

                // Calculate the start and end marker bound. Adjust for the column values if
                // the values don't make sense. Make sure we handle the case of empty file correctly
                ITextSnapshotLine startTextLine = textSnapshot.GetLineFromLineNumber(Math.Max(line - 1, 0));
                ITextSnapshotLine endTextLine = textSnapshot.GetLineFromLineNumber(Math.Max(endLine - 1, 0));
                if (column <= 0 || column >= startTextLine.Length)
                {
                    column = 1;
                    coerced = true;
                }

                // Calculate the end marker bound. Perform coersion on the values if they aren't consistent
                if (endColumn <= 0 && endColumn >= endTextLine.Length)
                {
                    endColumn = endTextLine.Length;
                    coerced = true;
                }

                // If we are highlighting just one line and the column values don't make
                // sense or we corrected one or more of them, then simply mark the
                // entire line
                if (endLine == line && (coerced || column >= endColumn))
                {
                    column = 1;
                    endColumn = endTextLine.Length;
                }

                // Create a span with the calculated markers
                markerStart = startTextLine.Start.Position + column - 1;
                markerEnd = endTextLine.Start.Position + endColumn;
                spanToColor = Span.FromBounds(markerStart, markerEnd);

                marker.AddTracking(textView, textSnapshot, docCookie, spanToColor);
            }
            catch (Exception e)
            {
                // Log the exception and move ahead. We don't want to bubble this or fail.
                // We just don't color the problem line.
                Debug.Print(e.Message);
            }
        }
    }
}
