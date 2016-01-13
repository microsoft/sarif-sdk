// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using EnvDTE;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.CodeAnalysis.Sarif.Sdk;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SarifViewer
{
    /// <summary>
    /// This class provides a data snapshot for the current contents of the error list
    /// </summary>
    internal class SarifSnapshot : TableEntriesSnapshotBase
    {
        private string _projectName;
        private readonly List<SarifError> _errors;
        private Dictionary<string, string> _remappedFilePaths;
        private List<Tuple<string, string>> _remappedPathPrefixes;
        private Dictionary<string, NewLineIndex> _fileToNewLineIndexMap;

        internal SarifSnapshot(string filePath, IEnumerable<SarifError> errors)
        {
            FilePath = filePath;
            _errors = new List<SarifError>(errors);

            _remappedFilePaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _remappedPathPrefixes = new List<Tuple<string, string>>();

            _fileToNewLineIndexMap = new Dictionary<string, NewLineIndex>(StringComparer.OrdinalIgnoreCase);

            Count = _errors.Count;
        }

        public override int Count { get; }

        public IList<SarifError> Errors { get; }

        public string FilePath { get; }

        public override int VersionNumber { get; } = 1;

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
                    content = Constants.VSIX_NAME;
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
                    content = _errors[index].Message;
                }
                else if (columnName == StandardTableKeyNames.ErrorSeverity)
                {
                    content = _errors[index].IsError ? __VSERRORCATEGORY.EC_ERROR : __VSERRORCATEGORY.EC_WARNING;
                }
                else if (columnName == StandardTableKeyNames.Priority)
                {
                    content = _errors[index].IsError ? vsTaskPriority.vsTaskPriorityHigh : vsTaskPriority.vsTaskPriorityMedium;
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
                    content = _errors[index].ErrorCode;
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
                else if ((columnName == StandardTableKeyNames.ErrorCodeToolTip) || (columnName == StandardTableKeyNames.HelpLink))
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

                    content = Uri.EscapeUriString(url);
                }
            }

            return content != null;
        }

        internal void TryNavigateTo(int index, bool isPreview)
        {
            if (isPreview)
            {
                return;
            }

            SarifError sarifError = _errors[index];
            string fileName = sarifError.FileName;

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            if (!File.Exists(fileName))
            {
                fileName = RebaselineFileName(fileName);

                if (!File.Exists(fileName))
                {
                    return;
                }
            }

            Guid logicalView = VSConstants.LOGVIEWID_Code;
            string vsViewKindCode = "{7651A701-06E5-11D1-8EBD-00A0C90F26EA}";
            var dte = SarifViewerPackage.Dte;
            var window = dte.ItemOperations.OpenFile(fileName, vsViewKindCode);

            if (window == null)
            {
                // No good association. This occurs when attempting to 
                // open a binary file directly, for example.
                return;
            }

            // Get the VsTextBuffer   
            VsTextBuffer buffer = window.DocumentData as VsTextBuffer;
            if (buffer == null)
            {
                IVsTextBufferProvider bufferProvider = window.DocumentData as IVsTextBufferProvider;
                if (bufferProvider != null)
                {
                    IVsTextLines lines;
                    ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(out lines));
                    buffer = lines as VsTextBuffer;
                    Debug.Assert(buffer != null, "IVsTextLines does not implement IVsTextBuffer");

                    if (buffer == null)
                    {
                        return;
                    }
                }
            }

            IVsTextManager mgr = SarifViewerPackage.GetGlobalService(typeof(VsTextManagerClass)) as IVsTextManager;
            if (mgr == null)
            {
                return;
            }

            Region region = sarifError.Region;

            NewLineIndex newLineIndex = null;
            if (!sarifError.RegionPopulated && sarifError.MimeType != MimeType.Binary)
            {
                if (!_fileToNewLineIndexMap.TryGetValue(fileName, out newLineIndex))
                {
                    _fileToNewLineIndexMap[fileName] = newLineIndex = new NewLineIndex(File.ReadAllText(fileName));
                }
                region.Populate(newLineIndex);
                sarifError.RegionPopulated = true;
            }

            // Data is 1-indexed but VS navigation api is 0-indexed
            mgr.NavigateToLineAndColumn(
                buffer, 
                ref logicalView, 
                region.StartLine - 1,
                region.StartColumn - 1,
                region.EndLine - 1,
                region.EndColumn - 1);
        }

        private string RebaselineFileName(string fileName)
        {
            // First, we'll traverse our remappings and see if we can
            // make rebaseline from existing data
            foreach (Tuple<string, string> remapping in _remappedPathPrefixes)
            {
                string remapped = fileName.Replace(remapping.Item1, remapping.Item2);
                if (File.Exists(remapped))
                {
                    return remapped;
                }
            }        

            OpenFileDialog openFileDialog = new OpenFileDialog();

            string fullPath = Path.GetFullPath(fileName);
            string shortName = Path.GetFileName(fullPath);

            openFileDialog.Title = "Locate missing file: " + fullPath;
            openFileDialog.Filter = shortName + "|" + shortName;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return fileName;
            }

            string resolvedPath = openFileDialog.FileName;
            string resolvedFileName = Path.GetFileName(resolvedPath);

            // If remapping has somehow altered the file name itself,
            // we will bail on attempting to do any remapping
            if (Path.GetFileName(fullPath) != resolvedFileName)
            {
                return fileName;
            }

            int offset = resolvedFileName.Length;
            while ((resolvedPath.Length - offset) >= 0 &&
                   (fullPath.Length - offset) >= 0) 
            {
                if (!resolvedPath[resolvedPath.Length - offset].ToString().Equals(fullPath[fullPath.Length - offset].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                offset++;
            }

            offset--;

            // At this point, we've got our hands on the common suffix for both the 
            // original file path and the resolved location. we trim this off both
            // values and then add a remapping that converts one to the other
            string originalPrefix = fullPath.Substring(0, fullPath.Length - offset);
            string resolvedPrefix = resolvedPath.Substring(0, resolvedPath.Length - offset);

            _remappedPathPrefixes.Add(new Tuple<string, string>(originalPrefix, resolvedPrefix));

            return resolvedPath;
        }
    }
}
