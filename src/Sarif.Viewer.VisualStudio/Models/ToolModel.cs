// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

namespace Microsoft.Sarif.Viewer.Models
{
    public class ToolModel : NotifyPropertyChangedObject
    {
        private string _name;
        private string _fullName;
        private string _version;
        private string _semanticVersion;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged(nameof(Name));
                }
            }
        }

        public string FullName
        {
            get
            {
                return _fullName;
            }
            set
            {
                if (value != _fullName)
                {
                    _fullName = value;
                    NotifyPropertyChanged(nameof(FullName));
                }
            }
        }

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                if (value != _version)
                {
                    _version = value;
                    NotifyPropertyChanged(nameof(Version));
                }
            }
        }

        public string SemanticVersion
        {
            get
            {
                return _semanticVersion;
            }
            set
            {
                if (value != _semanticVersion)
                {
                    _semanticVersion = value;
                    NotifyPropertyChanged(nameof(SemanticVersion));
                }
            }
        }
    }
}
