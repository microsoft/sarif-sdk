using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Models
{
    public class AnnotatedCodeLocationModel : CodeLocationObject
    {
        private string _message;
        private string _logicalLocation;
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

        public override string FilePath
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

        public int Index { get; set; }
        public string Kind { get; set; }
        public bool IsSelected { get; set; }
        public string Location { get { return Region.FormatForVisualStudio(); } }
    }
}
