using Microsoft.Sarif.Viewer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.ViewModels
{
    public class ResultViewModel : NotifyPropertyChangedObject
    {
        private string _ruleId;
        private string _ruleName;
        private string _message;
        private ObservableCollection<AnnotatedCodeLocationCollection> _locations;
        private ObservableCollection<AnnotatedCodeLocationCollection> _relatedLocations;
        private ObservableCollection<AnnotatedCodeLocationCollection> _codeFlows;
        private ObservableCollection<StackCollection> _stacks;

        public ResultViewModel()
        {
            this._locations = new ObservableCollection<AnnotatedCodeLocationCollection>();
            this._relatedLocations = new ObservableCollection<AnnotatedCodeLocationCollection>();
            this._codeFlows = new ObservableCollection<AnnotatedCodeLocationCollection>();
            this._stacks = new ObservableCollection<StackCollection>();
        }

        public string RuleId
        {
            get
            {
                return this._ruleId;
            }
            set
            {
                if (value != this._ruleId)
                {
                    this._ruleId = value;
                    NotifyPropertyChanged("RuleId");
                }
            }
        }

        public string RuleName
        {
            get
            {
                return this._ruleName;
            }
            set
            {
                if (value != this._ruleName)
                {
                    this._ruleName = value;
                    NotifyPropertyChanged("RuleName");
                }

            }
        }

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

        public ObservableCollection<AnnotatedCodeLocationCollection> Locations
        {
            get
            {
                return this._locations;
            }
        }

        public ObservableCollection<AnnotatedCodeLocationCollection> RelatedLocations
        {
            get
            {
                return this._relatedLocations;
            }
        }

        public ObservableCollection<AnnotatedCodeLocationCollection> CodeFlows
        {
            get
            {
                return this._codeFlows;
            }
        }

        public ObservableCollection<StackCollection> Stacks
        {
            get
            {
                return this._stacks;
            }
        }
    }
}
