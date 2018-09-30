// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class InsertOptionalDataVisitor : SarifRewritingVisitor
    {
        internal IFileSystem s_fileSystem = new FileSystem();

        private Run _run;
        private string _ruleId;
        private FileRegionsCache _fileRegionsCache;
        private readonly OptionallyEmittedData _dataToInsert;
        private readonly IDictionary<string, Uri> _originalUriBaseIds;
        
        public InsertOptionalDataVisitor(OptionallyEmittedData dataToInsert, IDictionary<string, Uri> originalUriBaseIds = null)
        {
            _dataToInsert = dataToInsert;
            _originalUriBaseIds = originalUriBaseIds;
        }

        public override Run VisitRun(Run node)
        {
            _run = node;

            if (_originalUriBaseIds != null)
            {
                _run.OriginalUriBaseIds = _run.OriginalUriBaseIds ?? new Dictionary<string, Uri>();

                foreach (string key in _originalUriBaseIds.Keys)
                {
                    _run.OriginalUriBaseIds[key] = _originalUriBaseIds[key];
                }
            }

            if (node == null) { return null; }

            bool scrapeFileReferences = _dataToInsert.Includes(OptionallyEmittedData.Hashes) ||
                                        _dataToInsert.Includes(OptionallyEmittedData.TextFiles) ||
                                        _dataToInsert.Includes(OptionallyEmittedData.BinaryFiles);

            if (scrapeFileReferences)
            {
                var visitor = new AddFileReferencesVisitor();
                visitor.VisitRun(node);
            }

            Run visited = base.VisitRun(node);

            return visited;
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            if (node.Region == null || node.Region.IsBinaryRegion)
            {
                goto Exit;
            }

            bool insertRegionSnippets = _dataToInsert.Includes(OptionallyEmittedData.RegionSnippets);
            bool overwriteExistingData = _dataToInsert.Includes(OptionallyEmittedData.OverwriteExistingData);
            bool insertContextCodeSnippets = _dataToInsert.Includes(OptionallyEmittedData.ContextRegionSnippets);
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

                if (!resolvedUri.IsAbsoluteUri) goto Exit;

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

        public override FileData VisitFileData(FileData node)
        {
            FileLocation fileLocation = node.FileLocation;
            if (fileLocation != null && _run.OriginalUriBaseIds != null)
            {
                bool workToDo = false;
                bool overwriteExistingData = _dataToInsert.Includes(OptionallyEmittedData.OverwriteExistingData);

                workToDo |= (node.Hashes == null || overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.Hashes);
                workToDo |= (node.Contents?.Text == null || overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.TextFiles);
                workToDo |= (node.Contents?.Binary == null || overwriteExistingData) && _dataToInsert.Includes(OptionallyEmittedData.BinaryFiles);

                if (workToDo)
                {
                    if (fileLocation.TryReconstructAbsoluteUri(_run.OriginalUriBaseIds, out Uri uri))
                    {
                        Encoding encoding = null;

                        string encodingText = node.Encoding ?? _run.DefaultFileEncoding;

                        if (!string.IsNullOrWhiteSpace(encodingText))
                        {
                            try
                            {
                                encoding = Encoding.GetEncoding(encodingText);
                            }
                            catch (ArgumentException) { }
                        }

                        int length = node.Length;
                        node = FileData.Create(uri, _dataToInsert, node.MimeType, encoding: encoding);
                        node.Length = length;
                    }
                }
            }

            return base.VisitFileData(node);
        }

        public override Result VisitResult(Result node)
        {
            _ruleId = node.RuleId;
            node = base.VisitResult(node);
            _ruleId = null;

            return node;
        }

        public override Message VisitMessage(Message node)
        {
            if ((node.Text == null || _dataToInsert.Includes(OptionallyEmittedData.OverwriteExistingData)) &&
                _dataToInsert.Includes(OptionallyEmittedData.FlattenedMessages))
            {
                Rule rule = null;
                string formatString = null;
                if (_ruleId != null &&
                    _run.Resources?.Rules.TryGetValue(_ruleId, out rule) == true)
                {
                    node.Text = node.Arguments?.Count > 0 
                        ? rule.Format(node.MessageId, node.Arguments) 
                        : rule.MessageStrings[node.MessageId];
                }
                else if (_run.Resources?.MessageStrings?.TryGetValue(node.MessageId, out formatString) == true)
                {
                    node.Text = node.Arguments?.Count > 0
                        ? string.Format(CultureInfo.CurrentCulture, formatString, node.Arguments.ToArray())
                        : formatString;
                }
            }
            return base.VisitMessage(node);
        }
    }
}
