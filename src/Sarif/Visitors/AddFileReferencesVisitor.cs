// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class AddFileReferencesVisitor : SarifRewritingVisitor
    {
        private Run _currentRun;
        private IDictionary<ArtifactLocation, int> _fileToIndexMap;

        public override Run VisitRun(Run run)
        {
            _fileToIndexMap = new Dictionary<ArtifactLocation, int>();

            run.Artifacts = run.Artifacts ?? new List<Artifact>();

            // First, we'll initialize our file object to index map
            // with any files that already exist in the table
            for (int i = 0; i < run.Artifacts.Count; i++)
            {
                Artifact fileData = run.Artifacts[i];

                var fileLocation = new ArtifactLocation
                {
                    Uri = fileData.Location.Uri,
                    UriBaseId = fileData.Location.UriBaseId
                };

                _fileToIndexMap[fileLocation] = i;

                // For good measure, we'll explicitly populate the file index property
                run.Artifacts[i].Location.Index = i;
            }

            _currentRun = run;

            // Next, visit all run file locations. This will add any
            // previously unknown file objects to the files table.
            base.VisitRun(run);
            return _currentRun;
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (_currentRun.OriginalUriBaseIds == null || !_currentRun.OriginalUriBaseIds.Values.Contains(node))
            {
                node.Index = _currentRun.GetFileIndex(node, addToFilesTableIfNotPresent: true);
            }

            return base.VisitArtifactLocation(node);
        }
    }
}
