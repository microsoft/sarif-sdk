using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Models
{
    public class AnnotatedCodeLocation : NotifyPropertyChangedObject
    {
        private string _message;
        private string _filePath;
        private string _module;
        private Region _region;

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

        public string FilePath
        {
            get
            {
                return this._filePath;
            }
            set
            {
                if (value != this._filePath)
                {
                    this._filePath = value;
                    NotifyPropertyChanged("FilePath");
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

        public Region Region
        {
            get
            {
                return this._region;
            }
            set
            {
                if (value != this._region)
                {
                    this._region = value;
                    NotifyPropertyChanged("Region");
                    NotifyPropertyChanged("Location");
                }
            }
        }

        public string Location { get { return Region.FormatForVisualStudio(); } }
    }
}
