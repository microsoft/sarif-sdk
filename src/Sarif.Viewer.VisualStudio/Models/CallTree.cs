// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace Microsoft.Sarif.Viewer.Models
{
    public class CallTree : NotifyPropertyChangedObject
    {
        CallTreeNode _selectedItem;
        DelegateCommand<TreeView> _selectPreviousCommand;
        DelegateCommand<TreeView> _selectNextCommand;

        private ObservableCollection<CallTreeNode> _topLevelNodes;

        public CallTree(IList<CallTreeNode> topLevelNodes)
        {
            TopLevelNodes = new ObservableCollection<CallTreeNode>(topLevelNodes);
        }

        public ObservableCollection<CallTreeNode> TopLevelNodes
        {
            get
            {
                return _topLevelNodes;
            }
            set
            {
                _topLevelNodes = value;

                // Set this object as the CallTree for the child nodes.
                if (_topLevelNodes != null)
                {
                    for (int i = 0; i < _topLevelNodes.Count; i++)
                    {
                        _topLevelNodes[i].CallTree = this;
                    }
                }
            }
        }


        public CallTreeNode SelectedItem
        {
            get
            {
                return this._selectedItem;
            }
            set
            {
                this._selectedItem = value;
                this.NotifyPropertyChanged(nameof(SelectedItem));
                
                // Remove the existing highlighting.
                if (_selectedItem != null)
                {
                    _selectedItem.ApplyDefaultSourceFileHighlighting();
                }
            }
        }

        private int GetIndexInCallTreeNodeList(IList<CallTreeNode> list, CallTreeNode givenNode)
        {
            int index = 0;
            if (list == null)
            {
                throw new ArgumentNullException("List is null.");
            }
            else
            {
                foreach (CallTreeNode listNode in list)
                {
                    if (listNode == givenNode)
                    {
                        return index;
                    }
                    else
                    {
                        index++;
                    }
                }
            }

            /* this exception should never be thrown, as this method should only be called when 
             * givenNode is already known to be a member of list 
             */
            throw new IndexOutOfRangeException("Given node was not in the list.");
        }

        private CallTreeNode FindNextNotCall(CallTreeNode currentNode)
        {
            CallTreeNode currentParent = currentNode.Parent;
            if (currentParent == null)
            {
                int currentNodeIndexInCallTreeNodeList = GetIndexInCallTreeNodeList(this.TopLevelNodes, currentNode);
                if (currentNodeIndexInCallTreeNodeList >= this.TopLevelNodes.Count - 1)
                {
                    // no next exists, return null and will revert back to currently selected node in original FindNext method
                    return null;
                }
                else
                {
                    return this.TopLevelNodes[currentNodeIndexInCallTreeNodeList + 1];
                }
            }
            else
            {
                int currentNodeIndexInCallTreeNodeList = GetIndexInCallTreeNodeList(currentParent.Children, currentNode);
                if (currentNodeIndexInCallTreeNodeList >= currentParent.Children.Count - 1)
                {
                    return FindNextNotCall(currentParent);
                }
                else
                {
                    return currentParent.Children[currentNodeIndexInCallTreeNodeList + 1];
                }
            }
        }

        internal CallTreeNode FindNext()
        {
            if (this.SelectedItem.Location.Kind == CodeAnalysis.Sarif.AnnotatedCodeLocationKind.Call && this.SelectedItem.Children.Count > 0)
            {
                return this.SelectedItem.Children[0];
            }
            else
            {
                CallTreeNode next = FindNextNotCall(this.SelectedItem);
                if (next == null)
                {
                    // no next exists, current remains selected
                    return this.SelectedItem;
                }
                else
                {
                    return next; 
                }
            }
        }

        // go to parent, find self, find previous/next, make sure not to roll off
        internal CallTreeNode FindPrevious()
        {
            IList<CallTreeNode> currentParentChildren;
            if (this.SelectedItem.Parent == null)
            {
                currentParentChildren = this.TopLevelNodes;
            }
            else
            {
                currentParentChildren = this.SelectedItem.Parent.Children;
            }

            CallTreeNode previous = null;
            foreach (CallTreeNode siblingNode in currentParentChildren)
            {
                if (siblingNode == this.SelectedItem)
                {
                        if (previous == null)
                        {
                            if (this.SelectedItem.Parent == null)
                            {
                                // no previous exists, current node stays selected
                                return this.SelectedItem;
                            }
                            else
                            {
                                return this.SelectedItem.Parent;
                            }
                        }
                        else
                        {
                            if (previous.Location.Kind == CodeAnalysis.Sarif.AnnotatedCodeLocationKind.Call && previous.Children.Count > 0)
                            {
                                return previous.Children[previous.Children.Count - 1];
                            }
                            else
                            {
                                return previous;
                            }
                        }
                }
                else
                {
                    previous = siblingNode;
                }
            }

            // default, should not occur
            return this.SelectedItem;
        }

        public DelegateCommand<TreeView> SelectPreviousCommand
        {
            get
            {
                if (_selectPreviousCommand == null)
                {
                    _selectPreviousCommand = new DelegateCommand<TreeView>(treeView => {
                        System.Windows.Controls.TreeView control = treeView as System.Windows.Controls.TreeView;
                        CallTree model = control.DataContext as CallTree;
                        model.SelectedItem = FindPrevious();
                    });
                }

                return _selectPreviousCommand;
            }
        }


        public DelegateCommand<TreeView> SelectNextCommand
        {
            get
            {
                if (_selectNextCommand == null)
                {
                    _selectNextCommand = new DelegateCommand<TreeView>(treeView => {
                        System.Windows.Controls.TreeView control = treeView as System.Windows.Controls.TreeView;
                        CallTree model = control.DataContext as CallTree;
                        model.SelectedItem = FindNext();
                    });
                }

                return _selectNextCommand;
            }
        }
    }
}