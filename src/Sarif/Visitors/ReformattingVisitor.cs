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

        private Run _run;
        private bool _overwriteExistingData;
        private FileRegionsCache _fileRegionsCache;
        private OptionallyEmittedData _dataToInsert;
        private Dictionary<Uri, string> _uriToFileTextMap;
        
        public ReformattingVisitor(OptionallyEmittedData dataToInsert, bool overwriteExistingData = true)
        {
            _dataToInsert = dataToInsert;
            _overwriteExistingData = dataToInsert.Includes(OptionallyEmittedData.OverwriteExistingData);
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
            _run = node;
            return node;
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            if (node.Region == null || node.Region.IsBinaryRegion)
            {
                goto Exit;
            }

            bool insertRegionSnippets = _dataToInsert.Includes(OptionallyEmittedData.RegionSnippets);
            bool insertContextCodeSnippets = _dataToInsert.Includes(OptionallyEmittedData.ContextCodeSnippets);
            bool populateRegionProperties = _dataToInsert.Includes(OptionallyEmittedData.ComprehensiveRegionProperties);

            if (insertRegionSnippets || insertRegionSnippets)
            {
                Region result;

                _fileRegionsCache = _fileRegionsCache ?? new FileRegionsCache(_run);
                result = _fileRegionsCache.PopulateTextRegionProperties(node, populateSnippet: insertContextCodeSnippets);

                FileContent originalSnippet = node.Region.Snippet;

                if (populateRegionProperties)
                {
                    node.Region = result;
                }

                if (originalSnippet == null || _overwriteExistingData)
                {
                    node.Region.Snippet = result.Snippet;
                }
                else
                {
                    node.Region.Snippet = originalSnippet;
                }
            }

            if (insertContextCodeSnippets && (node.ContextRegion == null || _overwriteExistingData))
            {
                node.ContextRegion = ConstructContextSnippet(node);               
            }

            Exit:
            return base.VisitPhysicalLocation(node);
        }

        internal Region ConstructContextSnippet(PhysicalLocation physicalLocation)
        {
            return null;
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

            workToDo |= (node.Hashes == null || _overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.Hashes);
            workToDo |= (node.Contents?.Binary == null || _overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.TextFiles);
            workToDo |= (node.Contents?.Binary == null || _overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.BinaryFiles);

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
