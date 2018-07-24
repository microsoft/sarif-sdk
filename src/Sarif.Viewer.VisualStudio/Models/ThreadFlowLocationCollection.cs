// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.Sarif.Viewer.Models
{
    public class ThreadFlowLocationCollection : ObservableCollection<ThreadFlowLocationModel>
    {
        private string _message;
        private ThreadFlowLocationModel _selectedItem;
        private DelegateCommand<ThreadFlowLocationModel> _selectedCommand;

        public ThreadFlowLocationCollection(string message)
        {
            this._message = message;
        }

        public string  Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value != this._message)
                {
                    _message = value;

                    this.OnPropertyChanged(new PropertyChangedEventArgs("Message"));
                }
            }
        }

        public ThreadFlowLocationModel SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;

                this.SelectionChanged(value);
            }
        }

        public DelegateCommand<ThreadFlowLocationModel> SelectedCommand
        {
            get
            {
                if (_selectedCommand == null)
                {
                    _selectedCommand = new DelegateCommand<ThreadFlowLocationModel>(l => SelectionChanged(l));
                }

                return _selectedCommand;
            }
            set
            {
                _selectedCommand = value;
            }
        }

        private void SelectionChanged(ThreadFlowLocationModel selectedItem)
        {
            selectedItem.NavigateTo();
            selectedItem.ApplySelectionSourceFileHighlighting();
        }
    }
}
