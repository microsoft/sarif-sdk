﻿// Copyright (c) Microsoft. All rights reserved. 
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

namespace Microsoft.Sarif.Viewer.ErrorList
{
    /// <summary>
    /// This class provides a data snapshot for the current contents of the error list
    /// </summary>
    internal class SarifSnapshot : TableEntriesSnapshotBase, IWpfTableEntriesSnapshot
    {
        private string _projectName;
        private readonly List<SarifErrorListItem> _errors;

        internal SarifSnapshot(string filePath, IEnumerable<SarifErrorListItem> errors)
        {
            FilePath = filePath;
            _errors = new List<SarifErrorListItem>(errors);
            Count = _errors.Count;
        }

        public override int Count { get; }

        public IList<SarifErrorListItem> Errors { get; }

        public string FilePath { get; }

        public override int VersionNumber { get; } = 1;

        public SarifErrorListItem GetItem(int index)
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
                    // The error list assumes the line number provided will be zero based and adds one before displaying the value.
                    // i.e. if we pass 5, the error list will display 6. 
                    // Subtract one from the line number so the error list displays the correct value.
                    int lineNumber = _errors[index].LineNumber - 1;
                    content = lineNumber;
                }
                else if (columnName == StandardTableKeyNames.Column)
                {
                    content = _errors[index].ColumnNumber;
                }
                else if (columnName == StandardTableKeyNames.Text)
                {
                    content = _errors[index].ShortMessage;
                }
                else if (columnName == StandardTableKeyNames.FullText)
                {
                    if (!string.IsNullOrEmpty(_errors[index].Message) && _errors[index].Message.Trim() != _errors[index].ShortMessage.Trim())
                    {
                        content = _errors[index].Message;
                    }
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
                    content = _errors[index].Tool.Name;
                }
                else if (columnName == StandardTableKeyNames.ErrorCode)
                {
                    content = _errors[index].Rule.Id;
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
                    content = error.Rule.Id + ":" + error.Rule.Name;
                }
                //else if (columnName == StandardTableKeyNames.DetailsExpander)
                //{
                //    var error = _errors[index];

                //    if (!string.IsNullOrEmpty(error.Message) && error.Message.Trim() != error.ShortMessage.Trim())
                //    {
                //        content = String.Empty;
                //    }
                //    else
                //    {
                //        content = null;
                //    }
                //}
                else if (columnName == "suppressionstate")
                {
                    var error = _errors[index];
                    content = error.SuppressionStates != SuppressionStates.None ? "Suppressed" : "Active";
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

            return (!string.IsNullOrEmpty(error.Message)) && (error.Message.Trim() != error.ShortMessage.Trim());
        }

        public bool TryCreateDetailsContent(int index, out FrameworkElement expandedContent)
        {
            var error = _errors[index];

            expandedContent = null;

            if (string.IsNullOrWhiteSpace(error.Message))
            {
                return false;
            }

            if (error.Message.Trim() == error.ShortMessage.Trim())
            {
                return false;
            }

            expandedContent = new TextBlock()
            {
                Background = null,
                Padding = new Thickness(10, 6, 10, 8),
                TextWrapping = TextWrapping.Wrap,
                Text = error.Message
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
                toolTip = _errors[index].Message;
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
