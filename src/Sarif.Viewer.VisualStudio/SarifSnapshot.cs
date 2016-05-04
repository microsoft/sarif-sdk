// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using EnvDTE;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// This class provides a data snapshot for the current contents of the error list
    /// </summary>
    internal class SarifSnapshot : TableEntriesSnapshotBase, IWpfTableEntriesSnapshot
    {
        private string _projectName;
        private readonly List<SarifError> _errors;

        internal SarifSnapshot(string filePath, IEnumerable<SarifError> errors)
        {
            FilePath = filePath;
            _errors = new List<SarifError>(errors);
            Count = _errors.Count;
        }

        public override int Count { get; }

        public IList<SarifError> Errors { get; }

        public string FilePath { get; }

        public override int VersionNumber { get; } = 1;

        public SarifError GetItem(int index)
        {
            return _errors[index];
        }

        public override bool TryGetValue(int index, string columnName, out object content)
        {
            content = null;

            if ((index >= 0) && (index < _errors.Count))
            {
                if (columnName == StandardTableKeyNames.DocumentName)
                {
                    content = FilePath;
                }
                else if (columnName == StandardTableKeyNames.ErrorCategory)
                {
                    var error = _errors[index];
                    content = error.Category;
                }
                else if (columnName == StandardTableKeyNames.Line)
                {
                    content = _errors[index].LineNumber;
                }
                else if (columnName == StandardTableKeyNames.Column)
                {
                    content = _errors[index].ColumnNumber;
                }
                else if (columnName == StandardTableKeyNames.Text)
                {
                    content = _errors[index].ShortMessage;
                }
                else if (columnName == StandardTableKeyNames.ErrorSeverity)
                {
                    content = GetSeverity(_errors[index].Level);
                }
                else if (columnName == StandardTableKeyNames.Priority)
                {
                    content = GetSeverity(_errors[index].Level) == __VSERRORCATEGORY.EC_ERROR
                        ? vsTaskPriority.vsTaskPriorityHigh
                        : vsTaskPriority.vsTaskPriorityMedium;
                }
                else if (columnName == StandardTableKeyNames.ErrorSource)
                {
                    content = ErrorSource.Build;
                }
                else if (columnName == StandardTableKeyNames.BuildTool)
                {
                    content = _errors[index].Tool;
                }
                else if (columnName == StandardTableKeyNames.ErrorCode)
                {
                    content = _errors[index].RuleId;
                }
                else if (columnName == StandardTableKeyNames.ProjectName)
                {
                    if (string.IsNullOrEmpty(_projectName))
                    {
                        var _item = SarifViewerPackage.Dte.Solution.FindProjectItem(_errors[index].FileName);

                        if (_item != null && _item.Properties != null && _item.ContainingProject != null)
                            _projectName = _item.ContainingProject.Name;
                    }

                    content = _projectName;
                }
                else if (columnName == StandardTableKeyNames.HelpLink)
                {
                    var error = _errors[index];
                    string url = null;
                    if (!string.IsNullOrEmpty(error.HelpLink))
                    {
                        url = error.HelpLink;
                    }
                    else
                    {
                        //url = string.Format("http://www.bing.com/search?q={0} {1}", _errors[index].Provider.Name, _errors[index].ErrorCode);
                    }

                    if (url != null)
                    {
                        content = Uri.EscapeUriString(url);
                    }
                }
                else if (columnName == StandardTableKeyNames.ErrorCodeToolTip)
                {
                    var error = _errors[index];
                    content = error.RuleId + ":" + error.RuleName;
                }
                else if (columnName == StandardTableKeyNames.DetailsExpander)
                {
                    var error = _errors[index];
                    content = !string.IsNullOrEmpty(error.FullMessage);
                }
            }

            return content != null;
        }

        private __VSERRORCATEGORY GetSeverity(ResultLevel level)
        {
            switch (level)
            {
                case ResultLevel.Error:
                {
                    return __VSERRORCATEGORY.EC_ERROR;
                }
                case ResultLevel.Warning:
                {
                    return __VSERRORCATEGORY.EC_WARNING;
                }
                case ResultLevel.NotApplicable:
                case ResultLevel.Pass:
                case ResultLevel.Note:
                {
                    return __VSERRORCATEGORY.EC_MESSAGE;
                }
            }
            return __VSERRORCATEGORY.EC_WARNING;
        }

        internal IVsWindowFrame NavigateTo(int index, bool isPreview)
        {
            if (isPreview)
            {
                return null;
            }

            SarifError sarifError = _errors[index];

            IVsWindowFrame result;
            CodeAnalysisResultManager.Instance.TryNavigateTo(sarifError, out result);
            return result;
        }

        public bool TryCreateImageContent(int index, string columnName, bool singleColumnView, out object content)
        {
            throw new NotImplementedException();
        }

        public bool TryCreateStringContent(int index, string columnName, bool truncatedText, bool singleColumnView, out string content)
        {
            content = null;
            return false;
        }

        public bool TryCreateColumnContent(int index, string columnName, bool singleColumnView, out FrameworkElement content)
        {
            content = null;
            return false;
        }

        public bool CanCreateDetailsContent(int index)
        {
            var error = _errors[index];

            return (!string.IsNullOrEmpty(error.FullMessage));
        }

        public bool TryCreateDetailsContent(int index, out FrameworkElement expandedContent)
        {
            var error = _errors[index];

            expandedContent = null;

            if (string.IsNullOrWhiteSpace(error.FullMessage))
            {
                return false;
            }

            expandedContent = new TextBlock()
            {
                Background = null,
                Padding = new Thickness(10, 6, 10, 8),
                TextWrapping = TextWrapping.Wrap,
                Text = error.FullMessage
            };

            return true;
        }

        public bool TryCreateDetailsStringContent(int index, out string content)
        {
            content = null;
            return false;
        }

        public bool TryCreateToolTip(int index, string columnName, out object toolTip)
        {            
            toolTip = null;

            if (columnName == StandardTableKeyNames.Text)
            {
                toolTip = _errors[index].FullMessage;
            }
            return toolTip != null;
        }

        public bool TryCreateImageContent(int index, string columnName, bool singleColumnView, out ImageMoniker content)
        {
            content = new ImageMoniker();
            return false;
        }

    }
}
