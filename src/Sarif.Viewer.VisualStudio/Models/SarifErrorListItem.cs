// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.Sarif.Viewer.Models;
using System;
using Microsoft.Sarif.Viewer.Sarif;

namespace Microsoft.Sarif.Viewer
{
    public class SarifErrorListItem : NotifyPropertyChangedObject
    {
        private string _fileName;
        private AnnotatedCodeLocationCollection _locations;
        private AnnotatedCodeLocationCollection _relatedLocations;
        private ObservableCollection<AnnotatedCodeLocationCollection> _codeFlows;
        private ObservableCollection<StackCollection> _stacks;

        public SarifErrorListItem(Run run, Result result)
        {
            IRule rule;
            run.TryGetRule(result.RuleId, out rule);
            this.RuleId = result.RuleId;
            this.RuleName = rule?.Name;
            this.ToolName = run.GetToolName();
            this.Message = result.GetMessageText(rule, concise: false);
            this.FileName = result.GetPrimaryTargetFile();
            this.Category = result.GetCategory();
            this.Region = result.GetPrimaryTargetRegion();

            if (this.Region != null)
            {
                this.LineNumber = this.Region.StartLine;
                this.ColumnNumber = this.Region.StartColumn;
            }

            this._locations = new AnnotatedCodeLocationCollection(String.Empty);
            this._relatedLocations = new AnnotatedCodeLocationCollection(String.Empty);
            this._codeFlows = new ObservableCollection<AnnotatedCodeLocationCollection>();
            this._stacks = new ObservableCollection<StackCollection>();
        }

        public string ToolName { get; set; }

        public string MimeType { get; set; }

        public Region Region { get; set; }

        public string FileName
        {
            get
            {
                return _fileName;
            }

            set
            {
                if (value == _fileName) { return; }
                UpdateAnnotationFilePaths(_fileName, value);
                _fileName = value;
            }
        }

        public bool RegionPopulated { get; set; }

        public string Message { get; set; }

        public SnapshotSpan Span { get; set; }

        public int LineNumber { get; set; }

        public int ColumnNumber { get; set; }

        public string Category { get; set; }

        public ResultLevel Level { get; set; }

        public string RuleId { get; set; }

        public string RuleName { get; set; }

        public string HelpLink { get; set; }

        public AnnotatedCodeLocationCollection Locations
        {
            get
            {
                return this._locations;
            }
        }

        public AnnotatedCodeLocationCollection RelatedLocations
        {
            get
            {
                return this._relatedLocations;
            }
        }

        public ObservableCollection<AnnotatedCodeLocationCollection> CodeFlows
        {
            get
            {
                return this._codeFlows;
            }
        }

        public ObservableCollection<StackCollection> Stacks
        {
            get
            {
                return this._stacks;
            }
        }

        public bool HasDetails
        {
            get
            {
                return this.Locations.Count > 0 || this.RelatedLocations.Count > 0 || this.CodeFlows.Count > 0 || this.Stacks.Count > 0;
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
            return Message;
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