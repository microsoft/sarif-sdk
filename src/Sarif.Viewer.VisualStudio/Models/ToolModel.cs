using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Models
{
    public class ToolModel : NotifyPropertyChangedObject
    {
        private string _name;
        private string _version;
        private string _description;
        private string _ownerName;
        private string _ownerUri;
        private string _feedbackUri;
        private string _helpUri;

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
    }
}
