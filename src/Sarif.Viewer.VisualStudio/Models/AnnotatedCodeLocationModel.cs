// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.IO;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Models
{
    public class AnnotatedCodeLocationModel : CodeLocationObject
    {
        private string _message;
        private string _logicalLocation;
        private string _module;
        private bool _isEssential;

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

        public string LogicalLocation
        {
            get
            {
                return this._logicalLocation;
            }
            set
            {
                if (value != this._logicalLocation)
                {
                    this._logicalLocation = value;
                    NotifyPropertyChanged("LogicalLocation");
                }
            }
        }

        public bool IsEssential
        {
            get
            {
                return this._isEssential;
            }
            set
            {
                if (value != this._isEssential)
                {
                    this._isEssential = value;
                    NotifyPropertyChanged("IsEssential");
                }
            }
        }

        public int Index { get; set; }
        public string Kind { get; set; }
        public bool IsSelected { get; set; }
        public string Location { get { return Region.FormatForVisualStudio(); } }
    }
}
