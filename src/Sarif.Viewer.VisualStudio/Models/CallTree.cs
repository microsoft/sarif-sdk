// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Models
{
    public class CallTree : NotifyPropertyChangedObject
    {
        CallTreeNode _selectedItem;
        DelegateCommand<TreeView> _selectPreviousCommand;
        DelegateCommand<TreeView> _selectNextCommand;

        private IToolWindow toolWindow;
        private ObservableCollection<CallTreeNode> _topLevelNodes;

        public CallTree(IList<CallTreeNode> topLevelNodes, IToolWindow toolWindow)
        {
            this.toolWindow = toolWindow;
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
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != value)
                {
                    // Remove the existing highlighting.
                    if (_selectedItem != null)
                    {
                        _selectedItem.ApplyDefaultSourceFileHighlighting();
                    }

                    _selectedItem = value;

                    // Navigate to the source of the selected node and highlight the region.
                    if (_selectedItem != null)
                    {
                        // Update the VS Properties window with the properties of the selected CallTreeNode.
                        toolWindow.UpdateSelectionList(_selectedItem.TypeDescriptor);

                        // Navigate to the source file of the selected CallTreeNode.
                        _selectedItem.NavigateTo();
                        _selectedItem.ApplySelectionSourceFileHighlighting();
                    }
                }

                this.NotifyPropertyChanged(nameof(SelectedItem));
            }
        }

        internal static bool TryGetIndexInCallTreeNodeList(IList<CallTreeNode> list, CallTreeNode givenNode, out int index)
        {
            index = -1;

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    CallTreeNode listNode = list[i];
                    if (listNode == givenNode)
                    {
                        index = i;
                        break;
                    }
                }
            }

            return index != -1;
        }

        internal CallTreeNode FindNext(CallTreeNode currentNode, bool includeChildren)
        {
            if (currentNode == null)
            {
                return null;
            }

            // For Call nodes, find the first visible child.
            CallTreeNode nextNode;
            if (includeChildren && TryGetFirstItem(currentNode.Children, out nextNode))
            {
                return nextNode;
            }

            // For all other nodes or Call nodes without a visible child, find the next visible sibling.
            CallTreeNode currentParent = currentNode.Parent;
            IList<CallTreeNode> nodeList;
 
            if (currentParent == null)
            {
                nodeList = this.TopLevelNodes;
            }
            else
            {
                nodeList = currentParent.Children;
            }

            if (TryGetNextSibling(nodeList, currentNode, out nextNode))
            {
                return nextNode;
            }

            // Walk up the tree trying to find the next node.
            return FindNext(currentParent, false);
        }

        internal CallTreeNode FindPrevious(CallTreeNode currentNode, bool includeChildren)
        {
            if (currentNode == null)
            {
                return null;
            }

            CallTreeNode previousNode;

            // Find the next visible sibling.
            CallTreeNode currentParent = currentNode.Parent;
            IList<CallTreeNode> nodeList;

            if (currentParent == null)
            {
                nodeList = this.TopLevelNodes;
            }
            else
            {
                nodeList = currentParent.Children;
            }

            if (TryGetPreviousSibling(nodeList, currentNode, out previousNode))
            {
                CallTreeNode previousNodeChild;
                if (includeChildren && TryGetLastItem(previousNode.Children, out previousNodeChild))
                {
                    return previousNodeChild;

                }
                else
                {
                    return previousNode;
                }
            }
            else if (currentParent != null && currentParent.Visibility == System.Windows.Visibility.Visible)
            {
                return currentParent;
            }

            // Walk up the tree trying to find the previous node.
            return FindPrevious(currentParent, false);
        }

        internal static bool TryGetNextSibling(IList<CallTreeNode> items, CallTreeNode currentItem, out CallTreeNode nextSibling)
        {
            nextSibling = null;

            int currentIndex;
            if (TryGetIndexInCallTreeNodeList(items, currentItem, out currentIndex))
            {
                for (int i = currentIndex + 1; i < items.Count; i++)
                {
                    CallTreeNode nextNode = items[i];
                    if (nextNode.Visibility == System.Windows.Visibility.Visible)
                    {
                        nextSibling = nextNode;
                        break;
                    }
                }
            }

            return nextSibling != null;
        }

        internal static bool TryGetPreviousSibling(IList<CallTreeNode> items, CallTreeNode currentItem, out CallTreeNode previousSibling)
        {
            previousSibling = null;

            int currentIndex;
            if (TryGetIndexInCallTreeNodeList(items, currentItem, out currentIndex))
            {
                for (int i = currentIndex - 1; i >= 0; i--)
                {
                    CallTreeNode previousNode = items[i];
                    if (previousNode.Visibility == System.Windows.Visibility.Visible)
                    {
                        previousSibling = previousNode;
                        break;
                    }
                }
            }

            return previousSibling != null;
        }

        internal static bool TryGetFirstItem(IList<CallTreeNode> items, out CallTreeNode firstItem)
        {
            firstItem = null;

            if (items != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    CallTreeNode nextNode = items[i];
                    if (nextNode.Visibility == System.Windows.Visibility.Visible)
                    {
                        firstItem = nextNode;
                        break;
                    }
                }
            }
            return firstItem != null;
        }

        internal static bool TryGetLastItem(IList<CallTreeNode> items, out CallTreeNode lastItem)
        {
            lastItem = null;

            if (items != null)
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    CallTreeNode nextNode = items[i];
                    if (nextNode.Visibility == System.Windows.Visibility.Visible)
                    {
                        lastItem = nextNode;
                        break;
                    }
                }
            }

            return lastItem != null;
        }

        internal CallTreeNode FindNext()
        {
            CallTreeNode next = FindNext(this.SelectedItem, true);
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

        // go to parent, find self, find previous/next, make sure not to roll off
        internal CallTreeNode FindPrevious()
        {
            CallTreeNode previous = FindPrevious(this.SelectedItem, true);
            if (previous == null)
            {
                // no previous exists, current remains selected
                return this.SelectedItem;
            }
            else
            {
                return previous;
            }
        }

        public DelegateCommand<TreeView> SelectPreviousCommand
        {
            get
            {
                if (_selectPreviousCommand == null)
                {
                    _selectPreviousCommand = new DelegateCommand<TreeView>(treeView =>
                    {
                        TreeView control = treeView as TreeView;
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
                    _selectNextCommand = new DelegateCommand<TreeView>(treeView =>
                    {
                        TreeView control = treeView as TreeView;
                        CallTree model = control.DataContext as CallTree;
                        model.SelectedItem = FindNext();
                    });
                }

                return _selectNextCommand;
            }
        }

        internal void ExpandAll()
        {
            if (TopLevelNodes != null)
            {
                foreach (CallTreeNode child in TopLevelNodes)
                {
                    child.ExpandAll();
                }
            }
        }

        internal void CollapseAll()
        {
            if (TopLevelNodes != null)
            {
                foreach (CallTreeNode child in TopLevelNodes)
                {
                    child.CollapseAll();
                }
            }
        }

        internal void IntelligentExpand()
        {
            if (TopLevelNodes != null)
            {
                foreach (CallTreeNode child in TopLevelNodes)
                {
                    child.IntelligentExpand();
                }
            }
        }

        internal void SetVerbosity(ThreadFlowLocationImportance importance)
        {
            if (TopLevelNodes != null)
            {
                foreach (CallTreeNode child in TopLevelNodes)
                {
                    child.SetVerbosity(importance);
                }
            }
        }
    }
}