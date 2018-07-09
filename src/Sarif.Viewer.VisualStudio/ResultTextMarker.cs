// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Tags;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// This class represents an instance of "highlighed" line in the editor, holds necessary Shell objects and logic 
    /// to managed lifecycle and appearance.
    /// </summary>
    public class ResultTextMarker
    {
        public const string DEFAULT_SELECTION_COLOR = "CodeAnalysisWarningSelection"; // Yellow
        public const string KEYEVENT_SELECTION_COLOR = "CodeAnalysisKeyEventSelection"; // Light yellow
        public const string LINE_TRACE_SELECTION_COLOR = "CodeAnalysisLineTraceSelection"; //Gray
        public const string HOVER_SELECTION_COLOR = "CodeAnalysisCurrentStatementSelection"; // Yellow with red border

        private Region m_region; 
        private IServiceProvider m_serviceProvider;
        private TrackingTagSpan<TextMarkerTag> m_marker;
        private SimpleTagger<TextMarkerTag> m_tagger;
        private ITrackingSpan m_trackingSpan;
        private IWpfTextView m_textView;
        private long? m_docCookie;

        public string FullFilePath { get; set; }
        public string UriBaseId { get; set; }
        public string Color { get; set; }

        public event EventHandler RaiseRegionSelected;

        /// <summary>
        /// fullFilePath may be null for global issues.
        /// </summary>
        public ResultTextMarker(IServiceProvider serviceProvider, Region region, string fullFilePath)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (region == null)
            {
                throw new ArgumentNullException(nameof(region));
            }

            m_serviceProvider = serviceProvider;
            m_region = region;
            FullFilePath = fullFilePath;
            Color = DEFAULT_SELECTION_COLOR;
        }

        internal IVsWindowFrame NavigateTo(bool usePreviewPane)
        {
            // Fall back to the file and line number

            if (!File.Exists(this.FullFilePath))
            {
                if (!CodeAnalysisResultManager.Instance.TryRebaselineAllSarifErrors(this.UriBaseId, this.FullFilePath))
                {
                    return null;
                }
            }

            IVsWindowFrame windowFrame = SdkUIUtilities.OpenDocument(SarifViewerPackage.ServiceProvider, this.FullFilePath, usePreviewPane);
            if (windowFrame != null)
            {
                IVsTextView textView = GetTextViewFromFrame(windowFrame);
                if (textView == null)
                {
                    return null;
                }

                var sourceLocation = this.GetSourceLocation();

                // Navigate the caret to the desired location. Text span uses 0-based indexes
                TextSpan ts;
                ts.iEndLine = ts.iStartLine = sourceLocation.StartLine - 1;
                ts.iEndIndex = ts.iStartIndex = Math.Max(sourceLocation.StartColumn - 1, 0);

                textView.EnsureSpanVisible(ts);
                textView.SetSelection(ts.iStartLine, ts.iStartIndex, ts.iEndLine, ts.iEndIndex);
            }
            return windowFrame;
        }

        /// <summary>
        /// Get source location of current marker (tracking code place). 
        /// </summary>
        /// <returns>
        /// This is clone of stored source location with actual source code coordinates.
        /// </returns>
        public Region GetSourceLocation()
        {
            Region sourceLocation = new Region()
            {
                ByteOffset = m_region.ByteOffset,
                StartColumn = m_region.StartColumn,
                EndColumn = m_region.EndColumn,
                StartLine = m_region.StartLine,
                EndLine = m_region.EndLine
            };
                
            SaveCurrentTrackingData(sourceLocation);
            return sourceLocation;
        }

        /// <summary>
        /// Save current tracking data to stored source location. 
        /// If user will change, save, close and after that open document which has tracking data 
        /// this class will not loose place where code exists.
        /// </summary>
        public void SaveCurrentTrackingData()
        {
            SaveCurrentTrackingData(m_region);
        }

        /// <summary>
        /// Clear all markers and tracking classes
        /// </summary>
        public void Clear()
        {
            if (m_marker != null)
            {
                RemoveHighlightMarker();
            }

            if (IsTracking())
            {
                RemoveTracking();
            }

            m_tagger = null;
        }

        /// <summary>
        /// Select current tracking text with <paramref name="highlightColor"/>. 
        /// If highlightColor is null than code will be selected with color from <seealso cref="Color"/>.
        /// If the mark doesn't support tracking changes, then we simply ignore this condition (addresses VS crash 
        /// reported in Bug 476347 : Code Analysis clicking error report C6244 causes VS2012 to crash).  
        /// Tracking changes helps to ensure that we nativate to the right line even if edits to the file
        /// have occured, but even if that behavior doesn't work right, it is better to 
        /// simply return here (before the fix this code threw an exception which terminated VS).
        /// </summary>
        /// <param name="highlightColor">Color</param>
        public void AddHighlightMarker(string highlightColor)
        {
            if (!IsTracking())
            {
                return;
            }

            m_marker = m_tagger.CreateTagSpan(m_trackingSpan, new TextMarkerTag(highlightColor ?? Color));
        }

        /// <summary>
        /// Add tracking for text in <paramref name="span"/> for document with id <paramref name="docCookie"/>.
        /// </summary>
        public void AddTracking(IWpfTextView wpfTextView, ITextSnapshot textSnapshot, long docCookie, Span span)
        {
            Debug.Assert(docCookie >= 0);
            Debug.Assert(!IsTracking(), "This marker already tracking changes.");
            m_docCookie = docCookie;
            CreateTracking(wpfTextView, textSnapshot, span);
            SubscribeToCaretEvents(wpfTextView);
        }

        /// <summary>
        /// Remove selection for tracking text
        /// </summary>
        public void RemoveHighlightMarker()
        {
            if (m_tagger != null && m_marker != null)
            {
                m_tagger.RemoveTagSpan(m_marker);
            }
            m_marker = null;
        }

        /// <summary>
        /// Check if current class track changes for document <paramref name="docCookie"/>
        /// </summary>
        public bool IsTracking(long docCookie)
        {
            return m_docCookie.HasValue && m_docCookie.Value == docCookie && m_trackingSpan != null;
        }

        public void DetachFromDocument(long docCookie)
        {
            if (this.IsTracking(docCookie))
            {
                this.Clear();
            }
        }

        /// <summary>
        /// Determines if a document can be associated with this ResultTextMarker.
        /// </summary>
        public bool CanAttachToDocument(string documentName, long docCookie, IVsWindowFrame frame)
        {
            // For these cases, this event has nothing to do with this item
            if (frame == null || this.IsTracking(docCookie) || string.Compare(documentName, this.FullFilePath, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// An overridden method for reacting to the event of a document window
        /// being opened
        /// </summary>
        public void AttachToDocument(string documentName, long docCookie, IVsWindowFrame frame)
        {
            // For these cases, this event has nothing to do with this item
            if (CanAttachToDocument(documentName, docCookie, frame))
            {
                AttachToDocumentWorker(frame, docCookie);
            }
        }

        private IVsTextView GetTextViewFromFrame(IVsWindowFrame frame)
        {
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
        /// Check that current <paramref name="marker"/> point to correct line position 
        /// and attach it to <paramref name="docCookie"/> for track changes.
        /// </summary>
        private void AttachToDocumentWorker(IVsWindowFrame frame, long docCookie)
        {
            var sourceLocation = this.GetSourceLocation();
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
                    AttachMarkerToTextView(wpfTextView, docCookie, this,
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

    private void RemoveTracking()
        {
            if (m_trackingSpan != null)
            {
                // TODO: Find a way to delete TrackingSpan
                m_marker = m_tagger.CreateTagSpan(m_trackingSpan, new TextMarkerTag(Color));
                RemoveHighlightMarker();
                m_trackingSpan = null;
                m_tagger = null;
                m_docCookie = null;
            }
        }

        private void CreateTracking(IWpfTextView textView, ITextSnapshot textSnapshot, Span span)
        {
            if (m_trackingSpan != null)
                return;

            m_textView = textView;

            if (m_tagger == null)
            {
                IComponentModel componentModel = (IComponentModel)m_serviceProvider.GetService(typeof(SComponentModel));
                ISarifLocationProviderFactory sarifLocationProviderFactory = componentModel.GetService<ISarifLocationProviderFactory>();

                // Get a SimpleTagger over the buffer to color
                m_tagger = sarifLocationProviderFactory.GetTextMarkerTagger(m_textView.TextBuffer);
            }

            // Add the marker
            if (m_tagger != null)
            {
                // The list of colors for TextMarkerTag are defined in Platform\Text\Impl\TextMarkerAdornment\TextMarkerProviderFactory.cs
                m_trackingSpan = textSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);
            }
        }

        private bool IsValidMarker()
        {
            return (m_marker != null &&
                    m_marker.Span != null &&
                    m_marker.Span.TextBuffer != null &&
                    m_marker.Span.TextBuffer.CurrentSnapshot != null);
        }

        private void SaveCurrentTrackingData(Region sourceLocation)
        {
            try
            {
                if (!IsTracking())
                {
                    return;
                }

                ITextSnapshot textSnapshot = m_trackingSpan.TextBuffer.CurrentSnapshot;
                SnapshotPoint startPoint = m_trackingSpan.GetStartPoint(textSnapshot);
                SnapshotPoint endPoint = m_trackingSpan.GetEndPoint(textSnapshot);

                var startLine = startPoint.GetContainingLine();
                var endLine = endPoint.GetContainingLine();

                var textLineStart = m_textView.GetTextViewLineContainingBufferPosition(startPoint);
                var textLineEnd = m_textView.GetTextViewLineContainingBufferPosition(endPoint);

                sourceLocation.StartColumn = startLine.Start.Position - textLineStart.Start.Position;
                sourceLocation.EndColumn = endLine.End.Position - textLineEnd.Start.Position;
                sourceLocation.StartLine = startLine.LineNumber + 1;
                sourceLocation.EndLine = endLine.LineNumber + 1;
            }
            catch (InvalidOperationException)
            {
                // Editor throws InvalidOperationException in some cases - 
                // We act like tracking isn't turned on if this is thrown to avoid
                // taking all of VS down.
            }
        }

        private bool IsTracking()
        {
            return m_docCookie.HasValue && IsTracking(m_docCookie.Value);
        }

        private void SubscribeToCaretEvents(IWpfTextView textView)
        {
            if (textView != null)
            {
                textView.Caret.PositionChanged += CaretPositionChanged;
                textView.LayoutChanged += ViewLayoutChanged;
            }
        }

        private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (m_textView != null)
            {
                // If a new snapshot wasn't generated, then skip this layout
                if (e.NewViewState.EditSnapshot != e.OldViewState.EditSnapshot)
                {
                    UpdateAtCaretPosition(m_textView.Caret.Position);
                }
            }
        }

        private void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            UpdateAtCaretPosition(e.NewPosition);
        }

        private void UpdateAtCaretPosition(CaretPosition caretPoisition)
        {
            // Check if the current caret position is within our region. If it is, raise the RegionSelected event.
            if (m_trackingSpan.GetSpan(m_trackingSpan.TextBuffer.CurrentSnapshot).Contains(caretPoisition.Point.GetPoint(m_trackingSpan.TextBuffer, PositionAffinity.Predecessor).Value))
            {
                OnRaiseRegionSelected(new EventArgs());
            }
        }

        protected virtual void OnRaiseRegionSelected(EventArgs e)
        {
            EventHandler handler = RaiseRegionSelected;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
