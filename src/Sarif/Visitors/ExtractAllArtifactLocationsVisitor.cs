// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{

    public class ExtractAllArtifactLocationsVisitor : SarifRewritingVisitor
    {
        private Run _currentRun;
        public HashSet<ArtifactLocation> allArtifactLocations { get; private set; }

        public ExtractAllArtifactLocationsVisitor()
        {
            allArtifactLocations = new HashSet<ArtifactLocation>(ArtifactLocation.ValueComparer);
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (node.Uri == null)
            {
                return node;
            }
            allArtifactLocations.Add(node.Resolve(_currentRun));
            return node;
        }

        public override Run VisitRun(Run node)
        {
            _currentRun = base.VisitRun(node);
            return node;
        }
    }
}
