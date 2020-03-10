// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// A visitor that examines a specified SARIF log to extract all artifact locations for later processing.
    /// This will extract every location present in the log file, which may then need to be filtered by a
    /// downstream consumer.
    /// 
    /// User may optionally set a specific run before invoking, in order to only fetch artifact locations
    /// from a subset on an entire SARIF file.
    /// </summary>
    public class ExtractAllArtifactLocationsVisitor : SarifRewritingVisitor
    {
        private Run _currentRun;
        public HashSet<ArtifactLocation> AllArtifactLocations { get; private set; }

        public ExtractAllArtifactLocationsVisitor()
        {
            AllArtifactLocations = new HashSet<ArtifactLocation>(ArtifactLocation.ValueComparer);
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            AllArtifactLocations.Add(node.Resolve(_currentRun));
            return node;
        }

        public override Run VisitRun(Run node)
        {
            _currentRun = node;
            _currentRun = base.VisitRun(node);
            return node;
        }
    }
}
