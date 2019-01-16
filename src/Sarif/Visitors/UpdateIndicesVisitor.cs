// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class UpdateIndicesVisitor : SarifRewritingVisitor
    {
        private IDictionary<string, int> _fullyQualifiedLogicalNameToIndexMap;
        private IDictionary<string, int> _fileLocationKeyToIndexMap;

        public UpdateIndicesVisitor(IDictionary<string, int> fullyQualifiedLogicalNameToIndexMap, IDictionary<string, int> fileLocationKeyToIndexMap)
        {
            _fullyQualifiedLogicalNameToIndexMap = fullyQualifiedLogicalNameToIndexMap;
            _fileLocationKeyToIndexMap = fileLocationKeyToIndexMap;
        }

        public override Location VisitLocation(Location node)
        {
            if (_fullyQualifiedLogicalNameToIndexMap != null && !string.IsNullOrEmpty(node.FullyQualifiedLogicalName))
            {
                if (_fullyQualifiedLogicalNameToIndexMap.TryGetValue(node.FullyQualifiedLogicalName, out int index))
                {
                    node.LogicalLocationIndex = index;
                }
            }

            return base.VisitLocation(node);
        }

        public override FileLocation VisitFileLocation(FileLocation node)
        {

            if (_fileLocationKeyToIndexMap != null)
            {
                string key = node.Uri.OriginalString;

                string uriBaseId = node.UriBaseId;
                if (!string.IsNullOrEmpty(uriBaseId))
                {
                    key = "#" + uriBaseId + "#" + key;
                }

                if (_fileLocationKeyToIndexMap.TryGetValue(key, out int index))
                {
                    var fileLocation = FileLocation.CreateFromFilesDictionaryKey(key);
                    node.Uri = new Uri(UriHelper.MakeValidUri(fileLocation.Uri.OriginalString), UriKind.RelativeOrAbsolute);
                    node.UriBaseId = fileLocation.UriBaseId;
                    node.FileIndex = index;
                }
            }

            return base.VisitFileLocation(node);
        }
    }
}