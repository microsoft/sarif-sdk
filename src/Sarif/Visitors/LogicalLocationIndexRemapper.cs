// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class LogicalLocationIndexRemapper : SarifRewritingVisitor
    {
        private IList<LogicalLocation> _previousLogicalLocations;

        public LogicalLocationIndexRemapper(IList<LogicalLocation> previousLogicalLocations, IDictionary<LogicalLocation, int> remappedLogicalLocations)
        {
            _previousLogicalLocations = previousLogicalLocations;
            RemappedLogicalLocations = remappedLogicalLocations;
        }

        public IDictionary<LogicalLocation, int> RemappedLogicalLocations { get; private set; }

        public override Result VisitResult(Result node)
        {
            if (_previousLogicalLocations == null) { return node; }

            return base.VisitResult(node);
        }

        public override Location VisitLocation(Location node)
        {
            if (node.LogicalLocationIndex != -1)
            {
                LogicalLocation logicalLocation = _previousLogicalLocations[node.LogicalLocationIndex];

                if (!RemappedLogicalLocations.TryGetValue(logicalLocation, out int remappedIndex))
                {
                    remappedIndex = RemappedLogicalLocations.Count;
                    RemappedLogicalLocations[logicalLocation] = remappedIndex;
                }

                node.LogicalLocationIndex = remappedIndex;
            }

            return node;
        }
    }
}
