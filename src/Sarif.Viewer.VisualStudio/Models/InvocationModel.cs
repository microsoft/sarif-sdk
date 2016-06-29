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
    public class InvocationModel : NotifyPropertyChangedObject
    {
        private string _commandLine;
        private DateTime _startTime;
        private DateTime _endTime;
        private string _machine;
        private string _account;
        private int _processId;
        private string _fileName;
        private string _workingDirectory;
        private object _environmentVariables;

        public string CommandLine
        {
            get
            {
                return this._commandLine;
            }
            set
            {
                if (value != this._commandLine)
                {
                    this._commandLine = value;
                    NotifyPropertyChanged("CommandLine");
                }
            }
        }

        public DateTime StartTime
        {
            get
            {
                return this._startTime;
            }
            set
            {
                if (value != this._startTime)
                {
                    this._startTime = value;
                    NotifyPropertyChanged("StartTime");
                }
            }
        }

        public DateTime EndTime
        {
            get
            {
                return this._endTime;
            }
            set
            {
                if (value != this._endTime)
                {
                    this._endTime = value;
                    NotifyPropertyChanged("EndTime");
                }
            }
        }

        public string Machine
        {
            get
            {
                return this._machine;
            }
            set
            {
                if (value != this._machine)
                {
                    this._machine = value;
                    NotifyPropertyChanged("Machine");
                }
            }
        }

        public string Account
        {
            get
            {
                return this._account;
            }
            set
            {
                if (value != this._account)
                {
                    this._account = value;
                    NotifyPropertyChanged("Account");
                }
            }
        }

        public int ProcessId
        {
            get
            {
                return this._processId;
            }
            set
            {
                if (value != this._processId)
                {
                    this._processId = value;
                    NotifyPropertyChanged("ProcessId");
                }
            }
        }

        public string FileName
        {
            get
            {
                return this._fileName;
            }
            set
            {
                if (value != this._fileName)
                {
                    this._fileName = value;
                    NotifyPropertyChanged("FileName");
                }
            }
        }

        public string WorkingDirectory
        {
            get
            {
                return this._workingDirectory;
            }
            set
            {
                if (value != this._workingDirectory)
                {
                    this._workingDirectory = value;
                    NotifyPropertyChanged("WorkingDirectory");
                }
            }
        }

        public object EnvironmentVariables
        {
            get
            {
                return this._environmentVariables;
            }
            set
            {
                if (value != this._environmentVariables)
                {
                    this._environmentVariables = value;
                    NotifyPropertyChanged("EnvironmentVariables");
                }
            }
        }
    }
}