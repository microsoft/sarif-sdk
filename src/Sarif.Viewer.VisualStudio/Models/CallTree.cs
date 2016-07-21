// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Sarif.Viewer.Models
{
    public class CallTree : NotifyPropertyChangedObject
    {
        private CodeLocationObject _selectedItem;

        public CallTree(IList<CallTreeNode> topLevelNodes)
        {
            TopLevelNodes = new ObservableCollection<CallTreeNode>(topLevelNodes);
        }

        public ObservableCollection<CallTreeNode> TopLevelNodes { get; }

        public CodeLocationObject SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                this.NotifyPropertyChanged(nameof(SelectedItem));

                _selectedItem.OnSelectKeyEvent();
            }
        }
    }
}
