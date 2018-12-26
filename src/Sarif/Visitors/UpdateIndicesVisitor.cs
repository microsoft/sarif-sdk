// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class UpdateIndicesVisitor : SarifRewritingVisitor
    {
        private IDictionary<string, int> _fullyQualifiedNameToIndexMap;

        public UpdateIndicesVisitor(IDictionary<string, int> fullyQualifiedNameToIndexMap)
        {
            _fullyQualifiedNameToIndexMap = fullyQualifiedNameToIndexMap;
        }

        public override Location VisitLocation(Location node)
        {
            if (_fullyQualifiedNameToIndexMap != null && !string.IsNullOrEmpty(node.FullyQualifiedLogicalName))
            {
                if (_fullyQualifiedNameToIndexMap.TryGetValue(node.FullyQualifiedLogicalName, out int index))
                {
                    node.LogicalLocationIndex = index;
                }
            }

            return node;
        }
    }
}
