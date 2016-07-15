// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

namespace Microsoft.Sarif.Viewer.Models
{
    public class RuleModel : NotifyPropertyChangedObject
    {
        private string _id;
        private string _name;
        private string _category;
        private string _severity;
        private string _description;
        private string _helpUri;

        public string Id
        {
            get
            {
                return this._id;
            }
            set
            {
                if (value != this._id)
                {
                    this._id = value;
                    NotifyPropertyChanged("Id");
                }
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                if (value != this._description)
                {
                    this._description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        public string Category
        {
            get
            {
                return this._category;
            }
            set
            {
                if (value != this._category)
                {
                    this._category = value;
                    NotifyPropertyChanged("Category");
                }
            }
        }

        public string DefaultLevel
        {
            get
            {
                return this._severity;
            }
            set
            {
                if (value != this._severity)
                {
                    this._severity = value;
                    NotifyPropertyChanged("Severity");
                }
            }
        }

        public string HelpUri
        {
            get
            {
                return this._helpUri;
            }
            set
            {
                if (value != this._helpUri)
                {
                    this._helpUri = value;
                    NotifyPropertyChanged("HelpUri");
                }
            }
        }
    }
}
