// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.IO;

namespace Microsoft.Sarif.Viewer.Models
{
    public class StackFrameModel : CodeLocationObject
    {
        private string _message;
        private int _line;
        private int _column;
        private int _address;
        private int _offset;
        private string _fullyQualifiedLogicalName;
        private string _module;

        public string Message
        {
            get
            {
                return this._message;
            }
            set
            {
                if (value != this._message)
                {
                    this._message = value;
                    NotifyPropertyChanged("Message");
                }
            }
        }

        public int Line
        {
            get
            {
                return this._line;
            }
            set
            {
                if (value != this._line)
                {
                    this._line = value;
                    NotifyPropertyChanged("Line");
                }
            }
        }

        public int Column
        {
            get
            {
                return this._column;
            }
            set
            {
                if (value != this._column)
                {
                    this._column = value;
                    NotifyPropertyChanged("Column");
                }
            }
        }

        public int Address
        {
            get
            {
                return this._address;
            }
            set
            {
                if (value != this._address)
                {
                    this._address = value;
                    NotifyPropertyChanged("Address");
                }
            }
        }

        public int Offset
        {
            get
            {
                return this._offset;
            }
            set
            {
                if (value != this._offset)
                {
                    this._offset = value;
                    NotifyPropertyChanged("Offset");
                }
            }
        }

        public override string FilePath
        {
            get
            {
                return base.FilePath;
            }
            set
            {
                if (value != this._filePath)
                {
                    base.FilePath = value;
                    NotifyPropertyChanged("FileName");
                }
            }
        }

        public string FileName
        {
            get
            {
                return Path.GetFileName(this.FilePath);
            }
        }

        public string FullyQualifiedLogicalName
        {
            get
            {
                return this._fullyQualifiedLogicalName;
            }
            set
            {
                if (value != this._fullyQualifiedLogicalName)
                {
                    this._fullyQualifiedLogicalName = value;
                    NotifyPropertyChanged("FullyQualifiedLogicalName");
                }
            }
        }

        public string Module
        {
            get
            {
                return this._module;
            }
            set
            {
                if (value != this._module)
                {
                    this._module = value;
                    NotifyPropertyChanged("Module");
                }
            }
        }
    }
}
