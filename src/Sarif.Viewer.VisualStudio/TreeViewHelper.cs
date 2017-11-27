// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    ///  The TreeViewHelper class enables binding to the SelectedItem of a TreeView.
    /// </summary>
    public static class TreeViewSelectionHelper
    {
        private static Dictionary<DependencyObject, TreeViewSelectedNodeBehavior> behaviors = new Dictionary<DependencyObject, TreeViewSelectedNodeBehavior>();

        public static object GetSelectedNode(DependencyObject obj)
        {
            return (object)obj.GetValue(SelectedNodeProperty);
        }

        public static void SetSelectedNode(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedNodeProperty, value);
        }

        public static readonly DependencyProperty SelectedNodeProperty =
            DependencyProperty.RegisterAttached("SelectedNode", typeof(object), typeof(TreeViewSelectionHelper), new UIPropertyMetadata(new object(), SelectedItemChanged));

        private static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is TreeView))
                return;

            if (!behaviors.ContainsKey(obj))
                behaviors.Add(obj, new TreeViewSelectedNodeBehavior(obj as TreeView));

            TreeViewSelectedNodeBehavior view = behaviors[obj];
            view.ChangeSelectedNode(e.NewValue);
        }

        private class TreeViewSelectedNodeBehavior
        {
            private TreeView _view;

            public TreeViewSelectedNodeBehavior(TreeView view)
            {
                _view = view;
                view.SelectedItemChanged += (sender, e) => SetSelectedNode(view, e.NewValue);
            }

            internal void ChangeSelectedNode(object newSelectedNode)
            {
                // In order to set the current node, we need to find it in the tree by navigating
                // the hierarchy from the root down

                Stack<CallTreeNode> pathToNode = new Stack<CallTreeNode>();
                CallTreeNode currentNode = newSelectedNode as CallTreeNode;

                // Construct the path to the new node
                while (currentNode != null)
                {
                    pathToNode.Push(currentNode);
                    currentNode = currentNode.Parent;
                }

                int depth = pathToNode.Count;
                TreeViewItem node = null;
                ItemsControl parent = null;

                // Walk the tree from the root to the new node
                while (pathToNode.Count > 0)
                {
                    currentNode = pathToNode.Pop();
                    if (pathToNode.Count == depth - 1)
                    {
                        parent = _view;
                    }
                    else
                    {
                        parent = node;
                    }

                    node = (TreeViewItem)parent.ItemContainerGenerator.ContainerFromItem(currentNode);

                    // Make sure to expand all the nodes in the hierarchy as we walk down
                    if (node != null)
                    {
                        // Do not expand the new selected node
                        if (pathToNode.Count != 0)
                        {
                            if (!node.IsExpanded)
                            {
                                node.ExpandSubtree();
                            }
                        }
                    }
                }

                // If we found the node in the tree, select it
                if (node != null)
                {
                    node.BringIntoView();
                    node.UpdateLayout();

                    if (!node.IsSelected)
                    {
                        node.IsSelected = true;
                    }

                    if (!node.IsFocused)
                    {
                        node.Focus();
                    }
                }
            }
        }
    }
}

