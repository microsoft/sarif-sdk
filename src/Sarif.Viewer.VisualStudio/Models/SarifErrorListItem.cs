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
        private ToolModel _tool;
        private RuleModel _rule;
        private string _selectedTab;
        private AnnotatedCodeLocationCollection _locations;
        private AnnotatedCodeLocationCollection _relatedLocations;
        private ObservableCollection<AnnotatedCodeLocationCollection> _codeFlows;
        private ObservableCollection<StackCollection> _stacks;
        private ObservableCollection<FixModel> _fixes;

        internal SarifErrorListItem()
        {
            this._locations = new AnnotatedCodeLocationCollection(String.Empty);
            this._relatedLocations = new AnnotatedCodeLocationCollection(String.Empty);
            this._codeFlows = new ObservableCollection<AnnotatedCodeLocationCollection>();
            this._stacks = new ObservableCollection<StackCollection>();
            this._fixes = new ObservableCollection<FixModel>();
        }

        public SarifErrorListItem(Run run, Result result) : this()
        {
            IRule rule;
            run.TryGetRule(result.RuleId, out rule);
            this.Message = result.GetMessageText(rule, concise: false);
            this.ShortMessage = result.GetMessageText(rule, concise: true);
            this.FileName = result.GetPrimaryTargetFile();
            this.Category = result.GetCategory();
            this.Region = result.GetPrimaryTargetRegion();

            if (this.Region != null)
            {
                this.LineNumber = this.Region.StartLine;
                this.ColumnNumber = this.Region.StartColumn;
            }

            this.Tool = run.Tool.ToToolModel();
            this.Rule = rule.ToRuleModel(result.RuleId);

            if (result.Locations != null)
            {
                foreach (Location location in result.Locations)
                {
                    this.Locations.Add(location.ToAnnotatedCodeLocationModel());
                }
            }

            if (result.RelatedLocations != null)
            {
                foreach (AnnotatedCodeLocation annotatedCodeLocation in result.RelatedLocations)
                {
                    this.RelatedLocations.Add(annotatedCodeLocation.ToAnnotatedCodeLocationModel());
                }
            }

            if (result.CodeFlows != null)
            {
                foreach (CodeFlow codeFlow in result.CodeFlows)
                {
                    this.CodeFlows.Add(codeFlow.ToAnnotatedCodeLocationCollection());
                }
            }

            if (result.Stacks != null)
            {
                foreach (Stack stack in result.Stacks)
                {
                    this.Stacks.Add(stack.ToStackCollection());
                }
            }

            if (result.Fixes != null)
            {
                foreach (Fix fix in result.Fixes)
                {
                    this.Fixes.Add(fix.ToFixModel());
                }
            }
        }

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
                _fileName = value;
                NotifyPropertyChanged("FileName");
            }
        }

        public bool RegionPopulated { get; set; }

        public string ShortMessage { get; set; }

        public string Message { get; set; }

        public SnapshotSpan Span { get; set; }

        public int LineNumber { get; set; }

        public int ColumnNumber { get; set; }

        public string Category { get; set; }

        public ResultLevel Level { get; set; }

        public string HelpLink { get; set; }

        public ToolModel Tool
        {
            get
            {
                return this._tool;
            }
            set
            {
                this._tool = value;
                NotifyPropertyChanged("Tool");
            }
        }

        public RuleModel Rule
        {
            get
            {
                return this._rule;
            }
            set
            {
                this._rule = value;
                NotifyPropertyChanged("Rule");
            }
        }

        public string SelectedTab
        {
            get
            {
                return this._selectedTab;
            }
            set
            {
                this._selectedTab = value;

                // If a new tab is selected, remove all the the markers for the
                // previous tab.
                this.RemoveMarkers();
            }
        }

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

        public ObservableCollection<FixModel> Fixes
        {
            get
            {
                return this._fixes;
            }
        }

        public bool HasDetails
        {
            get
            {
                return this.Locations.Count > 0 || this.RelatedLocations.Count > 0 || this.CodeFlows.Count > 0 || this.Stacks.Count > 0;
            }
        }

        internal void RemoveMarkers()
        {
            LineMarker.RemoveMarker();

            foreach (AnnotatedCodeLocationModel location in this.Locations)
            {
                location.LineMarker.RemoveMarker();
            }

            foreach (AnnotatedCodeLocationModel location in this.RelatedLocations)
            {
                location.LineMarker.RemoveMarker();
            }

            foreach (AnnotatedCodeLocationCollection locationCollection in this.CodeFlows)
            {
                foreach (AnnotatedCodeLocationModel location in locationCollection)
                {
                    location.LineMarker.RemoveMarker();
                }
            }

            foreach (StackCollection stackCollection in this.Stacks)
            {
                foreach (StackFrameModel stackFrame in stackCollection)
                {
                    stackFrame.LineMarker.RemoveMarker();
                }
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
                    if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
                    {
                        Debug.Assert(Region != null);
                    }

                    m_lineMarker = new ResultTextMarker(SarifViewerPackage.ServiceProvider, Region, FileName);
                }

                return m_lineMarker;
            }
            set
            {
                m_lineMarker = value;
            }
        }

        internal void RemapFilePath(string originalPath, string remappedPath)
        {
            if (this.FileName != null && this.FileName.Equals(originalPath, StringComparison.OrdinalIgnoreCase))
            {
                this.FileName = remappedPath;
            }

            foreach (AnnotatedCodeLocationModel location in this.Locations)
            {
                if (location.FilePath.Equals(originalPath, StringComparison.OrdinalIgnoreCase))
                {
                    location.FilePath = remappedPath;
                }
            }

            foreach (AnnotatedCodeLocationModel location in this.RelatedLocations)
            {
                if (location.FilePath.Equals(originalPath, StringComparison.OrdinalIgnoreCase))
                {
                    location.FilePath = remappedPath;
                }
            }

            foreach (AnnotatedCodeLocationCollection locationCollection in this.CodeFlows)
            {
                foreach (AnnotatedCodeLocationModel location in locationCollection)
                {
                    if (location.FilePath.Equals(originalPath, StringComparison.OrdinalIgnoreCase))
                    {
                        location.FilePath = remappedPath;
                    }
                }
            }

            foreach (StackCollection stackCollection in this.Stacks)
            {
                foreach (StackFrameModel stackFrame in stackCollection)
                {
                    if (stackFrame.FilePath.Equals(originalPath, StringComparison.OrdinalIgnoreCase))
                    {
                        stackFrame.FilePath = remappedPath;
                    }
                }
            }
        }

        internal void AttachToDocument(string documentName, long docCookie, IVsWindowFrame pFrame)
        {
            LineMarker.AttachToDocument(documentName, (long)docCookie, pFrame);

            foreach (AnnotatedCodeLocationModel location in this.Locations)
            {
                location.LineMarker.AttachToDocument(documentName, (long)docCookie, pFrame);
            }

            foreach (AnnotatedCodeLocationModel location in this.RelatedLocations)
            {
                location.LineMarker.AttachToDocument(documentName, (long)docCookie, pFrame);
            }

            foreach (AnnotatedCodeLocationCollection locationCollection in this.CodeFlows)
            {
                foreach (AnnotatedCodeLocationModel location in locationCollection)
                {
                    location.LineMarker.AttachToDocument(documentName, (long)docCookie, pFrame);
                }
            }

            foreach (StackCollection stackCollection in this.Stacks)
            {
                foreach (StackFrameModel stackFrame in stackCollection)
                {
                    stackFrame.LineMarker.AttachToDocument(documentName, (long)docCookie, pFrame);
                }
            }
        }

        internal void DetachFromDocument(long docCookie)
        {
            LineMarker.DetachFromDocument((long)docCookie);

            foreach (AnnotatedCodeLocationModel location in this.Locations)
            {
                location.LineMarker.DetachFromDocument((long)docCookie);
            }

            foreach (AnnotatedCodeLocationModel location in this.RelatedLocations)
            {
                location.LineMarker.DetachFromDocument((long)docCookie);
            }

            foreach (AnnotatedCodeLocationCollection locationCollection in this.CodeFlows)
            {
                foreach (AnnotatedCodeLocationModel location in locationCollection)
                {
                    location.LineMarker.DetachFromDocument((long)docCookie);
                }
            }

            foreach (StackCollection stackCollection in this.Stacks)
            {
                foreach (StackFrameModel stackFrame in stackCollection)
                {
                    stackFrame.LineMarker.DetachFromDocument((long)docCookie);
                }
            }
        }
    }
}