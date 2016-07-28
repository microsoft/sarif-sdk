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
    /// <remarks>
    /// Original source code taken from http://stackoverflow.com/questions/7153813/wpf-mvvm-treeview-selecteditem.
    /// </remarks>
    public static class TreeViewHelper
    {
        private static Dictionary<DependencyObject, TreeViewSelectedItemBehavior> behaviors = new Dictionary<DependencyObject, TreeViewSelectedItemBehavior>();

        public static object GetSelectedItem(DependencyObject obj)
        {
            return (object)obj.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(TreeViewHelper), new UIPropertyMetadata(new object(), SelectedItemChanged));

        private static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is System.Windows.Controls.TreeView))
                return;

            if (!behaviors.ContainsKey(obj))
                behaviors.Add(obj, new TreeViewSelectedItemBehavior(obj as System.Windows.Controls.TreeView));

            TreeViewSelectedItemBehavior view = behaviors[obj];
            view.ChangeSelectedItem(e.NewValue);
        }

        private class TreeViewSelectedItemBehavior
        {
            System.Windows.Controls.TreeView _view;
            public TreeViewSelectedItemBehavior(System.Windows.Controls.TreeView view)
            {
                _view = view;
                view.SelectedItemChanged += (sender, e) => SetSelectedItem(view, e.NewValue);
            }

            internal void ChangeSelectedItem(object newSelectedItem)
            {
                // In order to set the current item, we need to find it in the tree by navigating
                // the hierarchy from the root down.

                Stack<CallTreeNode> pathToItem = new Stack<CallTreeNode>();
                CallTreeNode currentNode = newSelectedItem as CallTreeNode;

                // Collect the path to the new item.
                while (currentNode != null)
                {
                    pathToItem.Push(currentNode);
                    currentNode = currentNode.Parent;
                }

                int depth = pathToItem.Count;
                TreeViewItem item = null;
                ItemsControl parent = null;

                // Walk the tree from the root to the new item.
                while (pathToItem.Count > 0)
                {
                    currentNode = pathToItem.Pop();
                    if (pathToItem.Count == depth - 1)
                    {
                        parent = _view;
                    }
                    else
                    {
                        parent = item;
                    }

                    item = (TreeViewItem)parent.ItemContainerGenerator.ContainerFromItem(currentNode);

                    // Make sure to expand all the nodes in the hierarchy as we walk down.
                    if (item != null)
                    {
                        if (!item.IsExpanded)
                        {
                            item.ExpandSubtree();
                        }
                    }
                    else
                    {
                        item.ToString();
                    }
                }

                // If we found the item in the tree, select it.
                if (item != null)
                {
                    item.BringIntoView();
                    item.UpdateLayout();

                    if (!item.IsSelected)
                    {
                        item.IsSelected = true;
                    }

                    if (!item.IsFocused)
                    {
                        item.Focus();
                    }
                }
            }
        }
    }
}

