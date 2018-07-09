// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class ReformattingVisitor : SarifRewritingVisitor
    {
        private LoggingOptions _loggingOptions;
        
        public ReformattingVisitor(LoggingOptions loggingOptions)
        {
            _loggingOptions = loggingOptions;
        }

        public override Run VisitRun(Run node)
        {
            if (node != null)
            {
                bool scrapeFileReferences = _loggingOptions.Includes(LoggingOptions.ComputeFileHashes)     ||
                                            _loggingOptions.Includes(LoggingOptions.PersistBinaryContents) ||
                                            _loggingOptions.Includes(LoggingOptions.PersistTextFileContents);

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

            return node;
        }

        internal FileData VisitDictionaryValueNullChecked(string key, FileData node)
        {
            Uri uri;

            if (!Uri.TryCreate(key, UriKind.RelativeOrAbsolute, out uri))
            {
                return node;
            }

            bool workToDo = false;

            workToDo |= node.Hashes == null   && _loggingOptions.Includes(LoggingOptions.ComputeFileHashes);
            workToDo |= node.Contents == null && _loggingOptions.Includes(LoggingOptions.PersistBinaryContents);
            workToDo |= node.Contents == null && _loggingOptions.Includes(LoggingOptions.PersistTextFileContents);

            if (workToDo)
            {
                // TODO: we should convert node.Encoding to a .NET equivalent and pass it here
                // https://github.com/Microsoft/sarif-sdk/issues/934
                node = FileData.Create(uri, _loggingOptions, node.MimeType, encoding: null);
            }

            return base.VisitFileData(node);
        }
    }
}
