// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Sarif.Viewer
{
    public class SarifError
    {
        public SarifError(string fileName)
        {
            FileName = fileName;
            Annotations = new ObservableCollection<AnnotatedCodeLocationModel>();
        }

        public string Tool { get; set; }

        public string MimeType { get; set; }

        public bool RegionPopulated { get; set; }

        public Region Region { get; set; }

        private string m_fileName;

        public string FileName
        {
            get
            {
                return m_fileName;
            }

            set
            {
                if (value == m_fileName) { return; }
                UpdateAnnotationFilePaths(m_fileName, value);
                m_fileName = value;
            }
        }

        private void UpdateAnnotationFilePaths(string current, string updated)
        {
            if (Annotations == null) { return; }

            foreach (AnnotatedCodeLocationModel codeLocation in Annotations)
            {
                if (codeLocation.FilePath == current)
                {
                    codeLocation.FilePath = updated;
                }
            }
        }

        public string ShortMessage { get; set; }

        public string FullMessage { get; set; }

        public SnapshotSpan Span { get; set; }

        public int LineNumber { get; set; }

        public int ColumnNumber { get; set; }

        public string Category { get; set; }

        public ResultLevel Level { get; set; }

        public string RuleId { get; set; }

        public string RuleName { get; set; }

        public string HelpLink { get; set; }

        public bool HasDetails
        {
            get
            {
                return this.Annotations.Count > 1 || !string.IsNullOrEmpty(this.Annotations[0].Message);
            }
        }

        public ObservableCollection<AnnotatedCodeLocationModel> Annotations { get; internal set;}

        internal void RemoveMarkers()
        {
            this.LineMarker.RemoveMarker();
            foreach (AnnotatedCodeLocationModel annotatedCodeLocation in Annotations)
            {
                annotatedCodeLocation.LineMarker.RemoveMarker();
            }
        }

        public override string ToString()
        {
            return ShortMessage;
        }

        ResultTextMarker m_lineMarker;
        public ResultTextMarker LineMarker
        {
            get
            {
                if (m_lineMarker == null)
                {
                    Debug.Assert(Region != null);
                    m_lineMarker = new ResultTextMarker(SarifViewerPackage.ServiceProvider, Region, FileName);
                }

                return m_lineMarker;
            }
            set
            {
                m_lineMarker = value;
            }
        }

        internal void AttachToDocument(string documentName, long docCookie, IVsWindowFrame pFrame)
        {
            LineMarker.AttachToDocument(documentName, (long)docCookie, pFrame);
            foreach (AnnotatedCodeLocationModel annotatedCodeLocation in Annotations)
            {
                annotatedCodeLocation.LineMarker.AttachToDocument(documentName, (long)docCookie, pFrame);
            }
        }

        internal void DetachFromDocument(long docCookie)
        {
            LineMarker.DetachFromDocument((long)docCookie);
            foreach (AnnotatedCodeLocationModel annotatedCodeLocation in Annotations)
            {
                annotatedCodeLocation.LineMarker.DetachFromDocument((long)docCookie);
            }
        }
    }
}