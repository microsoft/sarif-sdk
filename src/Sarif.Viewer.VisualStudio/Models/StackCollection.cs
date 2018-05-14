// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.Sarif.Viewer.Models
{
    public class StackCollection : ObservableCollection<StackFrameModel>
    {
        private string _message;

        public StackCollection(string message)
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
    }
}
