// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Microsoft.Sarif.Viewer.Models
{
    public class FixModel : NotifyPropertyChangedObject
    {
        private string _description;
        private ObservableCollection<FileChangeModel> _fileChanges;
        private DelegateCommand<FixModel> _previewFixCommand;
        private DelegateCommand<FixModel> _applyFixCommand;
        private static Dictionary<string, SortedList<int, int>> s_fileChangeLedger = new Dictionary<string, SortedList<int, int>>();

        public FixModel(string description)
        {
            this._description = description;
            this._fileChanges = new ObservableCollection<FileChangeModel>();
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (value != this._description)
                {
                    _description = value;

                    NotifyPropertyChanged("Description");
                }
            }
        }

        public ObservableCollection<FileChangeModel> FileChanges
        {
            get
            {
                return _fileChanges;
            }
        }

        public DelegateCommand<FixModel> PreviewFixCommand
        {
            get
            {
                if (_previewFixCommand == null)
                {
                    _previewFixCommand = new DelegateCommand<FixModel>(l => PreviewFix(l));
                }

                return _previewFixCommand;
            }
            set
            {
                _previewFixCommand = value;
            }
        }

        public DelegateCommand<FixModel> ApplyFixCommand
        {
            get
            {
                if (_applyFixCommand == null)
                {
                    _applyFixCommand = new DelegateCommand<FixModel>(l => ApplyFix(l));
                }

                return _applyFixCommand;
            }
            set
            {
                _applyFixCommand = value;
            }
        }

        private void PreviewFix(FixModel selectedFix)
        {
            HashSet<string> files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Gather the list of files for the fix.
            foreach (var fileChanges in selectedFix.FileChanges)
            {
                files.Add(fileChanges.FilePath);
            }

            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    List<ReplacementModel> replacements = new List<ReplacementModel>();

                    var fileChanges = selectedFix.FileChanges.Where(fc => fc.FilePath.Equals(file, StringComparison.OrdinalIgnoreCase));
                    foreach (FileChangeModel fileChange in fileChanges)
                    {
                        replacements.AddRange(fileChange.Replacements);
                    }

                    byte[] fixedFile;
                    if (TryFixFile(file, replacements, out fixedFile))
                    {
                        string extension = Path.GetExtension(file);
                        string baseFileName = Path.GetFileNameWithoutExtension(file);
                        string previewFileName = baseFileName + ".preview" + extension;
                        string previewFilePath = Path.Combine(Path.GetTempPath(), previewFileName);

                        File.WriteAllBytes(previewFilePath, fixedFile.ToArray());

                        SarifViewerPackage.Dte.ExecuteCommand("Tools.DiffFiles", file + " " + previewFilePath);
                    }
                }
            }
        }

        private void ApplyFix(FixModel selectedFix)
        {
            HashSet<string> files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Gather the list of files for the fix.
            foreach (var fileChanges in selectedFix.FileChanges)
            {
                files.Add(fileChanges.FilePath);
            }

            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    List<ReplacementModel> replacements = new List<ReplacementModel>();

                    var fileChanges = selectedFix.FileChanges.Where(fc => fc.FilePath.Equals(file, StringComparison.OrdinalIgnoreCase));
                    foreach (FileChangeModel fileChange in fileChanges)
                    {
                        replacements.AddRange(fileChange.Replacements);
                    }

                    byte[] fixedFile;
                    if (TryFixFile(file, replacements, out fixedFile))
                    {
                        File.WriteAllBytes(file, fixedFile.ToArray());
                    }
                }
            }
        }

        private bool TryFixFile(string filePath, IEnumerable<ReplacementModel> replacements, out byte[] fixedFile)
        {
            fixedFile = null;

            if (File.Exists(filePath))
            {
                // Sort the replacements from top to bottom.
                var sortedReplacements = replacements.OrderBy(r => r.Offset);

                // Delete/Insert the bytes for each replacement.
                List<byte> bytes = File.ReadAllBytes(filePath).ToList();

                SortedList<int, int> list = null;
                string path = filePath.ToLower();
                if (!s_fileChangeLedger.ContainsKey(path))
                {
                    // Create a new dictionary entry for this file
                    list = new SortedList<int, int>();

                    // Account for the BOM if it's present
                    if (!list.ContainsKey(0) &&
                        bytes.Count > 2 &&
                        bytes[0] == 0xEF &&
                        bytes[1] == 0xBB &&
                        bytes[2] == 0xBF)
                    {
                        list.Add(0, 3);
                    }

                    s_fileChangeLedger.Add(path, list);
                }
                else
                {
                    list = s_fileChangeLedger[path];
                }

                foreach (ReplacementModel replacement in sortedReplacements)
                {
                    // and add to this replacement's offset
                    int offset = list.Where(kvp => kvp.Key < replacement.Offset)
                                     .Sum(kvp => kvp.Value) + replacement.Offset;
                    int delta = 0;

                    if (replacement.DeletedLength > 0)
                    {
                        bytes.RemoveRange(offset, replacement.DeletedLength);
                        delta -= replacement.DeletedLength;
                    }

                    if (replacement.InsertedBytes.Length > 0)
                    {
                        bytes.InsertRange(offset, replacement.InsertedBytes);
                        delta += replacement.InsertedBytes.Length;
                    }

                    if (!list.ContainsKey(replacement.Offset))
                    {
                        // First change at this offset
                        list.Add(replacement.Offset, delta);
                    }
                    else
                    {
                        list[replacement.Offset] += delta;
                    }
                }

                fixedFile = bytes.ToArray();
            }

            return fixedFile != null;
        }
    }
}
