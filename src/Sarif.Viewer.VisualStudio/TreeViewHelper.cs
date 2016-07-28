// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.Sarif.Viewer.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
                Stack<CallTreeNode> pathToItem = new Stack<CallTreeNode>();
                CallTreeNode currentNode = newSelectedItem as CallTreeNode;

                while (currentNode != null)
                {
                    pathToItem.Push(currentNode);
                    currentNode = currentNode.Parent;
                }

                int depth = pathToItem.Count;
                TreeViewItem item = null;

                while (pathToItem.Count > 0)
                {
                    currentNode = pathToItem.Pop();
                    if (pathToItem.Count == depth - 1)
                    {
                        item = (TreeViewItem)_view.ItemContainerGenerator.ContainerFromItem(currentNode);
                    }
                    else
                    {
                        item = (TreeViewItem)item.ItemContainerGenerator.ContainerFromItem(currentNode);
                    }

                    // Make sure to expand all the nodes in the hierarchy.
                    if (item != null)
                    {
                        if (!item.IsExpanded)
                        {
                            item.ExpandSubtree();
                        }
                    }
                }

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

