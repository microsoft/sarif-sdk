// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Models
{
    public class ReplacementModel : NotifyPropertyChangedObject
    {
        private int _offset;
        private int _deletedLength;
        private string _insertedBytes;

        public int Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                if (value != this._offset)
                {
                    _offset = value;

                    NotifyPropertyChanged("Offset");
                }
            }
        }

        public int DeletedLength
        {
            get
            {
                return _deletedLength;
            }
            set
            {
                if (value != this._deletedLength)
                {
                    _deletedLength = value;

                    NotifyPropertyChanged("DeletedLength");
                }
            }
        }

        public string InsertedBytes
        {
            get
            {
                return _insertedBytes;
            }
            set
            {
                if (value != this._insertedBytes)
                {
                    _insertedBytes = value;

                    NotifyPropertyChanged("InsertedBytes");
                }
            }
        }
    }
}
