// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class ReformattingVisitor : SarifRewritingVisitor
    {
        private OptionallyEmittedData _dataToInsert;
        
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

            return node;
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
