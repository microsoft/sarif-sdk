// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class ReformattingVisitor : SarifRewritingVisitor
    {
        internal IFileSystem s_fileSystem = new FileSystem();

        private OptionallyEmittedData _dataToInsert;
        private Dictionary<Uri, string> _uriToFileTextMap;
        
        public ReformattingVisitor(OptionallyEmittedData dataToInsert)
        {
            _dataToInsert = dataToInsert;
        }

        public override Run VisitRun(Run node)
        {
            if (node != null)
            {
                bool scrapeFileReferences = _dataToInsert.Includes(OptionallyEmittedData.Hashes)      ||
                                            _dataToInsert.Includes(OptionallyEmittedData.TextFiles)   ||
                                            _dataToInsert.Includes(OptionallyEmittedData.BinaryFiles);

                if (scrapeFileReferences)
                {
                    var visitor = new AddFileReferencesVisitor();
                    visitor.VisitRun(node);
                }

                if (node.Files != null)
                {
                    var keys = node.Files.Keys.ToArray();
                    foreach (var key in keys)
                    {
                        var value = node.Files[key];
                        if (value != null)
                        {
                            node.Files[key] = VisitDictionaryValueNullChecked(key, value);
                        }
                    }
                }
            }

            if (node.Results != null)
            {
                var results = new List<Result>();

                foreach (Result result in node.Results)
                {
                    results.Add(VisitResult(result));
                }

                node.Results = results;
            }

            return node;
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            // If we have a binary region, there is no work to do
            if (node.Region == null ||
                (node.Region.ByteOffset > 0 || node.Region.ByteLength > 0))
            {
                goto Exit;
            }

            // If we already have a snippet, let's assume it is correct
            if (node.Region.Snippet != null)
            {
                goto Exit;
            }

            // If the user hasn't specified code snippet insertion, there is no work to do
            bool workToDo = false;
            workToDo |= node.Region != null && _dataToInsert.Includes(OptionallyEmittedData.CodeSnippets);

            if (!workToDo)
            {
                goto Exit;
            }

            // If we have an absolute URL specified as a file that 
            // exist on disk, there is work to do
            if (node.FileLocation.Uri.IsAbsoluteUri &&
                node.FileLocation.Uri.LocalPath != null &&
                s_fileSystem.FileExists(node.FileLocation.Uri.LocalPath))
            {
                Region region = node.Region;
                string fileText = GetFileText(node.FileLocation.Uri);
                GetCompleteFirstLineAssociatedWithRegion(region, fileText);
            }

            Exit:
            return base.VisitPhysicalLocation(node);
        }

        internal static void GetCompleteFirstLineAssociatedWithRegion(Region region, string fileText)
        {
            if (region.IsBinaryRegion)
            {
                return;
            }

            var lineIndex = new NewLineIndex(fileText);

            region.Snippet = new FileContent();

            int startLine;
            LineInfo lineInfo;

            if (region.StartLine == 0)
            {
                lineInfo = lineIndex.GetLineInfoForOffset(region.CharOffset);
                startLine = lineInfo.LineNumber;
            }
            else
            {
                lineInfo = lineIndex.GetLineInfoForLine(region.StartLine);
                startLine = region.StartLine;
            }

            int lineOffset = lineInfo.StartOffset;
            int lineLength;

            if (startLine == lineIndex.MaximumLineNumber)
            {
                // End of file. Compute length using file length
                lineLength = fileText.Length - lineOffset;
            }
            else
            {
                // Not at end of file. Get the offset from the
                // line after this one, then compute length from there.
                // Line length here will include any line break characters
                // at the end of this file.
                lineInfo = lineIndex.GetLineInfoForLine(startLine + 1);
                lineLength = lineInfo.StartOffset - lineOffset;
            }

            // Grab the line text, except for any trailing line break characters
            region.Snippet.Text = fileText.Substring(lineOffset, lineLength).TrimEnd(NewLineIndex.s_newLineChars);
        }        

        private string GetFileText(Uri uri)
        {
            _uriToFileTextMap = _uriToFileTextMap ?? new Dictionary<Uri, string>();
            
            if (!_uriToFileTextMap.TryGetValue(uri, out string fileText))
            {
                fileText = _uriToFileTextMap[uri] = s_fileSystem.ReadAllText(uri.LocalPath);
            }
            return fileText;
        }

        internal FileData VisitDictionaryValueNullChecked(string key, FileData node)
        {
            if (!Uri.TryCreate(key, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                return node;
            }

            bool workToDo = false;

            workToDo |= node.Hashes == null   && _dataToInsert.Includes(OptionallyEmittedData.Hashes);
            workToDo |= node.Contents?.Binary == null && _dataToInsert.Includes(OptionallyEmittedData.TextFiles);
            workToDo |= node.Contents?.Binary == null && _dataToInsert.Includes(OptionallyEmittedData.BinaryFiles);

            if (workToDo)
            {
                // TODO: we should convert node.Encoding to a .NET equivalent and pass it here
                // https://github.com/Microsoft/sarif-sdk/issues/934
                node = FileData.Create(uri, _dataToInsert, node.MimeType, encoding: null);
            }

            return base.VisitFileData(node);
        }
    }
}
