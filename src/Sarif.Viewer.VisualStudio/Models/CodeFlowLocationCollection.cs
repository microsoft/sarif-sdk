// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.Sarif.Viewer.Models
{
    public class CodeFlowLocationCollection : ObservableCollection<CodeFlowLocationModel>
    {
        private string _message;
        private CodeFlowLocationModel _selectedItem;
        private DelegateCommand<CodeFlowLocationModel> _selectedCommand;

        public CodeFlowLocationCollection(string message)
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

        public CodeFlowLocationModel SelectedItem
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

        public DelegateCommand<CodeFlowLocationModel> SelectedCommand
        {
            get
            {
                if (_selectedCommand == null)
                {
                    _selectedCommand = new DelegateCommand<CodeFlowLocationModel>(l => SelectionChanged(l));
                }

                return _selectedCommand;
            }
            set
            {
                _selectedCommand = value;
            }
        }

        private void SelectionChanged(CodeFlowLocationModel selectedItem)
        {
            selectedItem.NavigateTo();
            selectedItem.ApplySelectionSourceFileHighlighting();
        }
    }
}
