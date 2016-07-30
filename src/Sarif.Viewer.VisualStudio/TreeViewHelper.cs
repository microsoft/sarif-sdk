using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer
{
    public class TreeViewHelper
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

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
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
            System.Windows.Controls.TreeView view;
            public TreeViewSelectedItemBehavior(System.Windows.Controls.TreeView view)
            {
                this.view = view;
                view.SelectedItemChanged += (sender, e) => SetSelectedItem(view, e.NewValue);
            }

            internal void ChangeSelectedItem(object p)
            {
                Stack<CallTreeNode> pathToItem = new Stack<CallTreeNode>();
                CallTreeNode current = p as CallTreeNode;
                while (current != null)
                {
                    pathToItem.Push(current);
                    current = current.Parent;
                }

                int depth = pathToItem.Count;
                TreeViewItem item = null;

                // gets the path
                while (pathToItem.Count > 0)
                {
                    current = pathToItem.Pop();
                    if (pathToItem.Count == depth - 1)
                    {
                        item = (TreeViewItem)view.ItemContainerGenerator.ContainerFromItem(current);
                    }
                    else
                    {
                        item = (TreeViewItem)item.ItemContainerGenerator.ContainerFromItem(current);
                    }
                }

                if (item != null)
                {
                    item.IsSelected = true;
                }
            }
        }
    }
}
