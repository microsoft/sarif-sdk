// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Core;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class UpdateIndicesVisitor : SarifRewritingVisitor
    {
        private IDictionary<string, int> _fullyQualifiedLogicalNameToIndexMap;
        private IDictionary<FileLocation, int> _fileLocationToIndexMap;

        public UpdateIndicesVisitor(IDictionary<string, int> fullyQualifiedLogicalNameToIndexMap, IDictionary<FileLocation, int> fileLocationToIndexMap)
        {
            _fullyQualifiedLogicalNameToIndexMap = fullyQualifiedLogicalNameToIndexMap;
            _fileLocationToIndexMap = fileLocationToIndexMap;
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
            if (_fileLocationToIndexMap != null)
            {
                if (_fileLocationToIndexMap.TryGetValue(node, out int index))
                {
                   node.FileIndex = index;
                }
            }

            return base.VisitFileLocation(node);
        }
    }
}
