// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class InsertOptionalDataVisitor : SarifRewritingVisitor
    {
        internal IFileSystem s_fileSystem = new FileSystem();

        private Run _run;
        private bool _overwriteExistingData;
        private FileRegionsCache _fileRegionsCache;
        private OptionallyEmittedData _dataToInsert;
        
        public InsertOptionalDataVisitor(OptionallyEmittedData dataToInsert, bool overwriteExistingData = true)
        {
            _dataToInsert = dataToInsert;
            _overwriteExistingData = dataToInsert.Includes(OptionallyEmittedData.OverwriteExistingData);
        }

        public override Run VisitRun(Run node)
        {
            _run = node;

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
            if (node.Region == null || node.Region.IsBinaryRegion)
            {
                goto Exit;
            }

            bool insertRegionSnippets = _dataToInsert.Includes(OptionallyEmittedData.RegionSnippets);
            bool insertContextCodeSnippets = _dataToInsert.Includes(OptionallyEmittedData.ContextCodeSnippets);
            bool populateRegionProperties = _dataToInsert.Includes(OptionallyEmittedData.ComprehensiveRegionProperties);

            if (insertRegionSnippets || populateRegionProperties)
            {
                Region result;

                _fileRegionsCache = _fileRegionsCache ?? new FileRegionsCache(_run);
                
                // If we can resolve a file location to a newly constructed
                // absolute URI, we will prefer that
                if (!_run.TryResolveUri(node.FileLocation, out Uri resolvedUri))
                {
                    resolvedUri = node.FileLocation.Uri;
                }

                result = _fileRegionsCache.PopulateTextRegionProperties(node.Region, resolvedUri, populateSnippet: insertRegionSnippets);

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

        internal FileData VisitDictionaryValueNullChecked(string key, FileData node)
        {
            FileLocation fileLocation = FileLocation.CreateFromFilesDictionaryKey(key);

            bool workToDo = false;

            workToDo |= (node.Hashes == null || _overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.Hashes);
            workToDo |= (node.Contents?.Binary == null || _overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.TextFiles);
            workToDo |= (node.Contents?.Binary == null || _overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.BinaryFiles);

            if (workToDo)
            {
                _run.TryResolveUri(fileLocation, out Uri uri);

                // TODO: we should convert node.Encoding to a .NET equivalent and pass it here
                // https://github.com/Microsoft/sarif-sdk/issues/934
                node = FileData.Create(uri, _dataToInsert, node.MimeType, encoding: null);
            }

            return base.VisitFileData(node);
        }
    }
}
