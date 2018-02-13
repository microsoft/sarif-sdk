// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
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
        private static Dictionary<string, FixOffsetList> s_sourceFileFixLedger = null;
        private static string s_sourceFileFixLedgerFileName = "SourceFileChangeLedger.json";

        public FixModel(string description)
        {
            this._description = description;
            this._fileChanges = new ObservableCollection<FileChangeModel>();

            LoadFixLedger();
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

        private void SaveFixLedger()
        {
            if (s_sourceFileFixLedger.Count > 0)
            {
                SdkUiUtilities.StoreObject<Dictionary<string, FixOffsetList>>(s_sourceFileFixLedger, s_sourceFileFixLedgerFileName);
            }
        }

        private void LoadFixLedger()
        {
            if (s_sourceFileFixLedger == null)
            {
                s_sourceFileFixLedger = SdkUiUtilities.GetStoredObject<Dictionary<string, FixOffsetList>>(s_sourceFileFixLedgerFileName) ?? new Dictionary<string, FixOffsetList>();

                // Remove entries where the last modified timestamps don't match
                // This could indicate that the file was modified or overwritten externally
                List<string> remove = s_sourceFileFixLedger
                                          .Where(e => !File.Exists(e.Key) || File.GetLastWriteTime(e.Key) != e.Value.LastModified)
                                          .Select(e => e.Key)
                                          .ToList();

                if (remove.Count > 0)
                {
                    remove.ForEach(p => s_sourceFileFixLedger.Remove(p));
                    SaveFixLedger();
                }
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
                files.Add(fileChanges.FilePath.ToLower());
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
                        s_sourceFileFixLedger[file].LastModified = File.GetLastWriteTime(file);

                        // Save after every fix because we don't have sufficient events to
                        // guarantee data integrity
                        SaveFixLedger();
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

                FixOffsetList list = null;
                string path = filePath.ToLower();
                if (!s_sourceFileFixLedger.ContainsKey(path))
                {
                    // Create a new dictionary entry for this file
                    list = new FixOffsetList();

                    // Account for the BOM if it's present
                    if (bytes.Count > 2 &&
                        bytes[0] == 0xEF &&
                        bytes[1] == 0xBB &&
                        bytes[2] == 0xBF)
                    {
                        list.Offsets.Add(0, 3);
                    }

                    s_sourceFileFixLedger.Add(path, list);
                }
                else
                {
                    list = s_sourceFileFixLedger[path];
                }

                // Sum all of the changes that have been made up to the first replacement location
                ReplacementModel rm = sortedReplacements.First();
                int delta = list.Offsets.Where(kvp => kvp.Key < rm.Offset)
                                        .Sum(kvp => kvp.Value);

                foreach (ReplacementModel replacement in sortedReplacements)
                {
                    int offset = replacement.Offset + delta;
                    int thisDelta = 0;

                    if (replacement.DeletedLength > 0)
                    {
                        bytes.RemoveRange(offset, replacement.DeletedLength);
                        thisDelta -= replacement.DeletedLength;
                    }

                    if (replacement.InsertedBytes.Length > 0)
                    {
                        bytes.InsertRange(offset, replacement.InsertedBytes);
                        thisDelta += replacement.InsertedBytes.Length;
                    }

                    if (!list.Offsets.ContainsKey(replacement.Offset))
                    {
                        // First change at this offset
                        list.Offsets.Add(replacement.Offset, thisDelta);
                    }
                    else
                    {
                        // Add to previous change(s) at this location
                        list.Offsets[replacement.Offset] += thisDelta;
                    }

                    delta += thisDelta;
                }

                fixedFile = bytes.ToArray();
            }

            return fixedFile != null;
        }
    }
}
