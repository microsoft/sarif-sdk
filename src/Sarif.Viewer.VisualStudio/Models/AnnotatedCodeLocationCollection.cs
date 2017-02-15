// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Microsoft.Sarif.Viewer.Models
{
    public class AnnotatedCodeLocationCollection : ObservableCollection<AnnotatedCodeLocationModel>
    {
        private string _message;
        private AnnotatedCodeLocationModel _selectedItem;
        private DelegateCommand<AnnotatedCodeLocationModel> _selectedCommand;

        public AnnotatedCodeLocationCollection(string message)
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

        public AnnotatedCodeLocationModel SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;

                // Check if the dispatcher is processing requests.
                var dispatcherType = typeof(Dispatcher);
                var countField = dispatcherType.GetField("_disableProcessingCount", BindingFlags.Instance | BindingFlags.NonPublic);
                var count = (int)countField.GetValue(Dispatcher.CurrentDispatcher);
                var suspended = count > 0;

                if (!suspended)
                {
                    this.SelectionChanged(value);
                }
            }
        }

        public DelegateCommand<AnnotatedCodeLocationModel> SelectedCommand
        {
            get
            {
                if (_selectedCommand == null)
                {
                    _selectedCommand = new DelegateCommand<AnnotatedCodeLocationModel>(l => SelectionChanged(l));
                }

                return _selectedCommand;
            }
            set
            {
                _selectedCommand = value;
            }
        }

        private void SelectionChanged(AnnotatedCodeLocationModel selectedItem)
        {
            selectedItem.NavigateTo();
            selectedItem.ApplySelectionSourceFileHighlighting();
        }
    }
}
