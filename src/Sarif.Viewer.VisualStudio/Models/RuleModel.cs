// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

namespace Microsoft.Sarif.Viewer.Models
{
    public class RuleModel : NotifyPropertyChangedObject
    {
        private string _id;
        private string _name;
        private string _category;
        private string _defaultLevel;
        private string _description;
        private string _helpUri;

        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged(nameof(Id));
                }
            }
        }

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

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (value != _description)
                {
                    _description = value;
                    NotifyPropertyChanged(nameof(Description));
                }
            }
        }

        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                if (value != _category)
                {
                    _category = value;
                    NotifyPropertyChanged(nameof(Category));
                }
            }
        }

        public string DefaultLevel
        {
            get
            {
                return _defaultLevel;
            }
            set
            {
                if (value != _defaultLevel)
                {
                    _defaultLevel = value;
                    NotifyPropertyChanged(nameof(DefaultLevel));
                }
            }
        }

        public string HelpUri
        {
            get
            {
                return _helpUri;
            }
            set
            {
                if (value != _helpUri)
                {
                    _helpUri = value;
                    NotifyPropertyChanged(nameof(HelpUri));
                }
            }
        }
    }
}
