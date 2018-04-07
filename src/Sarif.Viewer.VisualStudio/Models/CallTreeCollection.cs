// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Models
{
    public class CallTreeCollection : ObservableCollection<CallTree>
    {
        private int _verbosity;
        private DelegateCommand _expandAllCommand;
        private DelegateCommand _collapseAllCommand;
        private DelegateCommand _intelligentExpandCommand;

        public CallTreeCollection()
        {
        }

        public int Verbosity
        {
            get
            {
                return _verbosity;
            }
            set
            {
                if (_verbosity != value)
                {
                    _verbosity = value;

                    CodeFlowLocationImportance importance;
                    if (_verbosity >= 200)
                    {
                        importance = CodeFlowLocationImportance.Unimportant;
                    }
                    else if (_verbosity >= 100)
                    {
                        importance = CodeFlowLocationImportance.Important;
                    }
                    else
                    {
                        importance = CodeFlowLocationImportance.Essential;
                    }

                    SetVerbosity(importance);

                    SelectVisibleNode();
                }
            }
        }

        public DelegateCommand ExpandAllCommand
        {
            get
            {
                if (_expandAllCommand == null)
                {
                    _expandAllCommand = new DelegateCommand(() => {
                        ExpandAll();
                    });
                }

                return _expandAllCommand;
            }
            set
            {
                _expandAllCommand = value;
            }
        }

        public DelegateCommand CollapseAllCommand
        {
            get
            {
                if (_collapseAllCommand == null)
                {
                    _collapseAllCommand = new DelegateCommand(() =>
                    {
                        CollapseAll();
                    });
                }

                return _collapseAllCommand;
            }
            set
            {
                _collapseAllCommand = value;
            }
        }

        public DelegateCommand IntelligentExpandCommand
        {
            get
            {
                if (_intelligentExpandCommand == null)
                {
                    _intelligentExpandCommand = new DelegateCommand(() =>
                    {
                        IntelligentExpand();
                    });
                }

                return _intelligentExpandCommand;
            }
            set
            {
                _intelligentExpandCommand = value;
            }
        }

        internal void ExpandAll()
        {
            foreach (CallTree callTree in this)
            {
                callTree.ExpandAll();
            }
        }

        internal void CollapseAll()
        {
            foreach (CallTree callTree in this)
            {
                callTree.CollapseAll();
            }
        }

        internal void IntelligentExpand()
        {
            foreach (CallTree callTree in this)
            {
                CallTreeNode selectedItem = callTree.SelectedItem;

                callTree.IntelligentExpand();

                callTree.SelectedItem = selectedItem;
            }
        }

        internal void SetVerbosity(CodeFlowLocationImportance importance)
        {
            foreach (CallTree callTree in this)
            {
                callTree.SetVerbosity(importance);
            }
        }

        internal void SelectVisibleNode()
        {
            foreach (CallTree callTree in this)
            {
                if (callTree.SelectedItem != null && callTree.SelectedItem.Visibility != System.Windows.Visibility.Visible)
                {
                    callTree.SelectedItem = callTree.FindPrevious();
                }
            }
        }
    }
}
