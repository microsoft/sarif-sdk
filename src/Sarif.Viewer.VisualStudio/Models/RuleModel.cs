// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Models
{
    public class RuleModel : NotifyPropertyChangedObject
    {
        private string _id;
        private string _name;
        private string _version;
        private string _category;
        private string _severity;
        private string _description;
        private string _ownerName;
        private string _ownerUri;
        private string _feedbackUri;
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

        public string OwnerName
        {
            get
            {
                return this._ownerName;
            }
            set
            {
                if (value != this._ownerName)
                {
                    this._ownerName = value;
                    NotifyPropertyChanged("OwnerName");
                }
            }
        }

        public string OwnerUri
        {
            get
            {
                return this._ownerUri;
            }
            set
            {
                if (value != this._ownerUri)
                {
                    this._ownerUri = value;
                    NotifyPropertyChanged("OwnerUri");
                }
            }
        }

        public string FeedbackUri
        {
            get
            {
                return this._feedbackUri;
            }
            set
            {
                if (value != this._feedbackUri)
                {
                    this._feedbackUri = value;
                    NotifyPropertyChanged("FeedbackUri");
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

        public string Severity
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

        public string Version
        {
            get
            {
                return this._version;
            }
            set
            {
                if (value != this._version)
                {
                    this._version = value;
                    NotifyPropertyChanged("Version");
                }
            }
        }

        public bool HasValues
        {
            get
            {
                if (string.IsNullOrEmpty(OwnerName) && string.IsNullOrEmpty(OwnerUri) && 
                    string.IsNullOrEmpty(FeedbackUri) && string.IsNullOrEmpty(Description) && 
                    string.IsNullOrEmpty(Category) && string.IsNullOrEmpty(Version) && 
                    (string.IsNullOrEmpty(Severity) || Severity == "Unknown"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
