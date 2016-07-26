// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Sarif.Viewer.Models
{
    public class CallTree : NotifyPropertyChangedObject
    {
        private CallTreeNode _selectedItem;

        public CallTree(IList<CallTreeNode> topLevelNodes)
        {
            TopLevelNodes = new ObservableCollection<CallTreeNode>(topLevelNodes);
        }

        public ObservableCollection<CallTreeNode> TopLevelNodes { get; }

        public CallTreeNode SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                this.NotifyPropertyChanged(nameof(SelectedItem));

                // Navigate to the source of the selected node and highlight the region.
                if (_selectedItem != null)
                {
                    SarifViewerPackage.SarifToolWindow.ApplySelectionList(_selectedItem);

                    try
                    {
                        if (_selectedItem.LineMarker != null)
                        {
                            _selectedItem.OnSelectKeyEvent();
                        }
                    }
                    catch (System.Exception e)
                    {
                        e.ToString();
                    }
                }
            }
        }
    }
}
