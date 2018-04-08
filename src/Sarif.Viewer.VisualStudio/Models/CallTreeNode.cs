// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Sarif;

namespace Microsoft.Sarif.Viewer.Models
{
    public class CallTreeNode : CodeLocationObject
    {
        private CodeFlowLocation _location;
        private CallTree _callTree;
        private CallTreeNode _parent;
        private bool _isExpanded;
        private Visibility _visbility;

        [Browsable(false)]
        public CodeFlowLocation Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;

                if (value?.Location?.PhysicalLocation != null)
                {
                    // If the backing CodeFlowLocation has a PhysicalLocation, set the 
                    // Region property. If it has a FileLocation, set the FilePath.
                    // The FilePath and Region properties are used to navigate to the
                    // source location and highlight the line.
                    Region = value.Location.PhysicalLocation.Region;

                    if (value.Location.PhysicalLocation.FileLocation?.Uri != null)
                    {
                        FilePath = value.Location.PhysicalLocation.FileLocation.Uri.ToPath();
                    }
                }
                else
                {
                    FilePath = null;
                    Region = null;
                }
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    NotifyPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public Visibility Visibility
        {
            get
            {
                return _visbility;
            }
            set
            {
                if (value != _visbility)
                {
                    _visbility = value;
                    NotifyPropertyChanged(nameof(Visibility));
                }
            }
        }

        /// <summary>
        /// Returns the location string formatted for Visual Studio.
        /// e.g. myfile.c (24,10)
        /// </summary>
        [Browsable(false)]
        public string LocationDisplayString
        {
            get
            {
                string text = String.Empty;

                if (!String.IsNullOrEmpty(FilePath))
                {
                    text = Path.GetFileName(FilePath) + " ";
                }

                if (Location?.Location?.PhysicalLocation?.Region != null)
                {
                    text += Location.Location.PhysicalLocation.Region.FormatForVisualStudio();
                }

                return text;
            }
        }

        internal override ResultTextMarker LineMarker
        {
            get
            {
                // Not all locations have regions. Don't try to mark the locations that don't.
                if (_lineMarker == null && Region != null)
                {
                    _lineMarker = new ResultTextMarker(SarifViewerPackage.ServiceProvider, Region, FilePath);
                    _lineMarker.RaiseRegionSelected += RegionSelected;
                }

                return _lineMarker;
            }
            set
            {
                _lineMarker = value;
            }
        }

        /// <summary>
        /// Called when the source code region of this node is
        /// selected in the editor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegionSelected(object sender, EventArgs e)
        {
            // Select this item in the CallTree to bring the source and call tree in sync.
            if (CallTree != null && this.Visibility == Visibility.Visible)
            {
                CallTree.SelectedItem = this;
            }
        }

        [Browsable(false)]
        public override string DefaultSourceHighlightColor
        {
            get
            {
                if (this.Location.Importance == CodeFlowLocationImportance.Essential)
                {
                    return ResultTextMarker.KEYEVENT_SELECTION_COLOR;
                }
                else
                {
                    return ResultTextMarker.LINE_TRACE_SELECTION_COLOR;
                }
            }
        }

        [Browsable(false)]
        public override string SelectedSourceHighlightColor
        {
            get
            {
                return ResultTextMarker.HOVER_SELECTION_COLOR;
            }
        }

        [Browsable(false)]
        public List<CallTreeNode> Children { get; set; }

        [Browsable(false)]
        public CallTree CallTree
        {
            get
            {
                return _callTree;
            }
            set
            {
                _callTree = value;

                // If there are any children, set their call tree too.
                if (Children != null)
                {
                    for (int i = 0; i < Children.Count; i++)
                    {
                        Children[i].CallTree = _callTree;
                    }
                }
            }
        }

        [Browsable(false)]
        public CallTreeNode Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;

                // Set our call tree to our new parent's call tree.
                if (_parent != null)
                {
                    CallTree = _parent.CallTree;
                }
            }
        }

        public int? Step
        {
            get
            {
                return Location?.Step;
            }
        }

        [Category("Location")]
        [DisplayName("Source file")]
        public string SourceFile
        {
            get
            {
                Uri sourceUrl = Location?.Location?.PhysicalLocation?.FileLocation?.Uri;

                if (sourceUrl != null)
                {
                    return Path.GetFileName(sourceUrl.LocalPath);
                }

                return null;
            }
        }

        [Category("Location")]
        [DisplayName("Start line")]
        public int? StartLine
        {
            get
            {
                return Location?.Location?.PhysicalLocation?.Region?.StartLine;
            }
        }

        [Category("Location")]
        [DisplayName("End line")]
        public int? EndLine
        {
            get
            {
                return Location?.Location?.PhysicalLocation?.Region?.EndLine;
            }
        }

        [Category("Location")]
        [DisplayName("Start column")]
        public int? StartColumn
        {
            get
            {
                return Location?.Location?.PhysicalLocation?.Region?.StartColumn;
            }
        }

        [Category("Location")]
        [DisplayName("End column")]
        public int? EndColumn
        {
            get
            {
                return Location?.Location?.PhysicalLocation?.Region?.EndColumn;
            }
        }

        public CodeFlowLocationImportance? Importance
        {
            get
            {
                return Location?.Importance;
            }
        }

        public string Message
        {
            get
            {
                return Location?.Location?.Message.Text;
            }
        }

        public string Snippet
        {
            get
            {
                return Location?.Location?.PhysicalLocation?.Region?.Snippet.Text;
            }
        }

        public Dictionary<string, string> Properties
        {
            get
            {
                Dictionary<string, string> properties = new Dictionary<string, string>();

                if (Location?.PropertyNames != null)
                {
                    foreach (string key in Location.PropertyNames)
                    {
                        properties.Add(key, Location.GetProperty<object>(key).ToString());
                    }
                }

                return properties;
            }
        }

        internal void ExpandAll()
        {
            this.IsExpanded = true;

            if (Children != null)
            {
                foreach (CallTreeNode child in Children)
                {
                    child.ExpandAll();
                }
            }
        }

        internal void CollapseAll()
        {
            this.IsExpanded = false;

            if (Children != null)
            {
                foreach (CallTreeNode child in Children)
                {
                    child.CollapseAll();
                }
            }
        }

        internal void IntelligentExpand()
        {
            if (Location?.Importance == CodeFlowLocationImportance.Essential)
            {
                CallTreeNode current = this;

                while (current != null)
                {
                    current.IsExpanded = true;
                    current = current.Parent;
                }
            }
            else
            {
                IsExpanded = false;
            }

            if (Children != null)
            {
                foreach (CallTreeNode child in Children)
                {
                    child.IntelligentExpand();
                }
            }
        }

        internal void SetVerbosity(CodeFlowLocationImportance importance)
        {
            Visibility visibility = Visibility.Visible;
            CodeFlowLocationImportance myImportance = (Location?.Importance).GetValueOrDefault(CodeFlowLocationImportance.Unimportant);

            switch (importance)
            {
                case CodeFlowLocationImportance.Essential:
                    if (myImportance != CodeFlowLocationImportance.Essential)
                    {
                        visibility = Visibility.Collapsed;
                    }
                    break;
                case CodeFlowLocationImportance.Important:
                    if (myImportance == CodeFlowLocationImportance.Unimportant)
                    {
                        visibility = Visibility.Collapsed;
                    }
                    break;
                default:
                    visibility = Visibility.Visible;
                    break;
            }

            if (visibility == Visibility.Visible)
            {
                CallTreeNode current = this;

                while (current != null)
                {
                    current.Visibility = Visibility.Visible;
                    current = current.Parent;
                }
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }

            if (Children != null)
            {
                foreach (CallTreeNode child in Children)
                {
                    child.SetVerbosity(importance);
                }
            }
        }
    }
}
