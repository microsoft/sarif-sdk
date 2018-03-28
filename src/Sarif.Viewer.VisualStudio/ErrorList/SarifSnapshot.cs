// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using EnvDTE;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using XamlDoc = System.Windows.Documents;

namespace Microsoft.Sarif.Viewer.ErrorList
{
    /// <summary>
    /// This class provides a data snapshot for the current contents of the error list
    /// </summary>
    internal class SarifSnapshot : TableEntriesSnapshotBase, IWpfTableEntriesSnapshot
    {
        private readonly List<SarifErrorListItem> _errors;
        private static string s_embeddedLinkRegexPattern = "\\[(.+?)\\]";

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
                if (columnName == StandardTableKeyNames2.TextInlines)
                {
                    if (_errors[index].Tool.Name.StartsWith("Semmle")) // Semmle logs can have tool name == "Semmle" or "Semmle QL"
                    {
                        // It's a Semmle result, so we will look for [embedded link] substrings
                        string message = _errors[index].Message;
                        var inlines = new List<Inline>();

                        MatchCollection matches = Regex.Matches(message, s_embeddedLinkRegexPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

                        if (matches.Count > 0) // If there are no [embedded links], we do nothing and the FullText case below will provide plain text content
                        {
                            int start = 0;
                            int i = 0;
                            foreach (Match match in matches)
                            {
                                inlines.Add(new XamlDoc.Run(message.Substring(start, match.Index - start)));

                                var link = new Hyperlink();
                                link.Tag = new Tuple<int, int>(index, i);
                                link.Click += ErrorListInlineLink_Click;
                                link.Inlines.Add(new XamlDoc.Run($"{match.Value.Trim('[', ']')}"));

                                inlines.Add(link);

                                start = match.Index + match.Length;
                                if (++i == matches.Count)
                                {
                                    inlines.Add(new XamlDoc.Run(message.Substring(start)));
                                }
                            }

                            content = inlines;
                        }
                    }
                }
                else if (columnName == StandardTableKeyNames.DocumentName)
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
                    SarifErrorListItem error = _errors[index];

                    if (error.Rule != null)
                    {
                        content = _errors[index].Rule.Id;
                    }
                }
                else if (columnName == StandardTableKeyNames.ProjectName)
                {
                    content = _errors[index].ProjectName;
                }
                else if (columnName == StandardTableKeyNames.HelpLink)
                {
                    var error = _errors[index];
                    string url = null;
                    if (!string.IsNullOrEmpty(error.HelpLink))
                    {
                        url = error.HelpLink;
                    }

                    if (url != null)
                    {
                        content = Uri.EscapeUriString(url);
                    }
                }
                else if (columnName == StandardTableKeyNames.ErrorCodeToolTip)
                {
                    var error = _errors[index];
                    if (error.Rule != null)
                    {
                        content = error.Rule.Id + ":" + error.Rule.Name;
                    }
                }
                else if (columnName == "suppressionstate")
                {
                    var error = _errors[index];
                    content = error.SuppressionStates != SuppressionStates.None ? "Suppressed" : "Active";
                }
            }

            return content != null;
        }

        private void ErrorListInlineLink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperLink = sender as Hyperlink;

            if (hyperLink != null)
            {
                Tuple<int, int> indices = hyperLink.Tag as Tuple<int, int>;

                SarifErrorListItem sarifResult = _errors[indices.Item1];

                // Set the current sarif error in the manager so we track code locations.
                CodeAnalysisResultManager.Instance.CurrentSarifError = sarifResult;

                if (sarifResult.HasDetails)
                {
                    // Setting the DataContext to be null first forces the TabControl to select the appropriate tab.
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = null;
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = sarifResult;
                }
                else
                {
                    SarifViewerPackage.SarifToolWindow.Control.DataContext = null;
                }

                sarifResult.RelatedLocations[indices.Item2].NavigateTo(false);
                sarifResult.RelatedLocations[indices.Item2].ApplyDefaultSourceFileHighlighting();
            }
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
