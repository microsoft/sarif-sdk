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
        private FileRegionsCache _fileRegionsCache;
        private readonly OptionallyEmittedData _dataToInsert;
        
        public InsertOptionalDataVisitor(OptionallyEmittedData dataToInsert)
        {
            _dataToInsert = dataToInsert;
        }

        public override Run VisitRun(Run node)
        {
            _run = node;

            if (node == null) { return null; }

            bool scrapeFileReferences = _dataToInsert.Includes(OptionallyEmittedData.Hashes) ||
                                        _dataToInsert.Includes(OptionallyEmittedData.TextFiles) ||
                                        _dataToInsert.Includes(OptionallyEmittedData.BinaryFiles);

            if (scrapeFileReferences)
            {
                var visitor = new AddFileReferencesVisitor();
                visitor.VisitRun(node);
            }

            if (node.Files != null)
            {
                // Note, we modify this collection as  we enumerate it.
                // Hence the need to convert to an array here. Otherwise,
                // the standard collection enumerator will throw an
                // exception after we touch the collection.
                var keys = node.Files.Keys.ToArray();
                foreach (string key in keys)
                {
                    var value = node.Files[key];
                    if (value != null)
                    {
                        node.Files[key] = VisitDictionaryValueNullChecked(key, value);
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
            bool overwriteExistingData = _dataToInsert.Includes(OptionallyEmittedData.OverwriteExistingData);
            bool insertContextCodeSnippets = _dataToInsert.Includes(OptionallyEmittedData.ContextCodeSnippets);
            bool populateRegionProperties = _dataToInsert.Includes(OptionallyEmittedData.ComprehensiveRegionProperties);

            if (insertRegionSnippets || populateRegionProperties || insertContextCodeSnippets)
            {
                Region expandedRegion;

                _fileRegionsCache = _fileRegionsCache ?? new FileRegionsCache(_run);

                // If we can resolve a file location to a newly constructed
                // absolute URI, we will prefer that
                if (!node.FileLocation.TryReconstructAbsoluteUri(_run.OriginalUriBaseIds, out Uri resolvedUri))
                {
                    resolvedUri = node.FileLocation.Uri;
                }

                expandedRegion = _fileRegionsCache.PopulateTextRegionProperties(node.Region, resolvedUri, populateSnippet: insertRegionSnippets);

                FileContent originalSnippet = node.Region.Snippet;

                if (populateRegionProperties)
                {
                    node.Region = expandedRegion;
                }

                if (originalSnippet == null || overwriteExistingData)
                {
                    node.Region.Snippet = expandedRegion.Snippet;
                }
                else
                {
                    node.Region.Snippet = originalSnippet;
                }

                if (insertContextCodeSnippets && (node.ContextRegion == null || overwriteExistingData))
                {
                    node.ContextRegion = _fileRegionsCache.ConstructMultilineContextSnippet(expandedRegion, resolvedUri);
                }
            }

            Exit:
            return base.VisitPhysicalLocation(node);
        }

        internal FileData VisitDictionaryValueNullChecked(string key, FileData node)
        {
            FileLocation fileLocation = FileLocation.CreateFromFilesDictionaryKey(key);

            bool workToDo = false;
            bool overwriteExistingData = _dataToInsert.Includes(OptionallyEmittedData.OverwriteExistingData);

            workToDo |= (node.Hashes == null || overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.Hashes);
            workToDo |= (node.Contents?.Binary == null || overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.TextFiles);
            workToDo |= (node.Contents?.Binary == null || overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.BinaryFiles);

            if (workToDo)
            {
                fileLocation.TryReconstructAbsoluteUri(_run.OriginalUriBaseIds, out Uri uri);

                // TODO: we should convert node.Encoding to a .NET equivalent and pass it here
                // https://github.com/Microsoft/sarif-sdk/issues/934
                node = FileData.Create(uri, _dataToInsert, node.MimeType, encoding: null);
            }

            return base.VisitFileData(node);
        }
    }
}
