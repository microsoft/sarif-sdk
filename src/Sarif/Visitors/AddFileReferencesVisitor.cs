// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

            for (int i = 0; i < run.Artifacts.Count; i++)
            {
                Artifact fileData = run.Artifacts[i];

                var fileLocation = new ArtifactLocation
                {
                    Uri = fileData.Location.Uri,
                    UriBaseId = fileData.Location.UriBaseId
                };

                _fileToIndexMap[fileLocation] = i;

                run.Artifacts[i].Location.Index = i;
            }

            _currentRun = run;

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

        public override Invocation VisitInvocation(Invocation node)
        {
            // workingDirectory carries only a bare location — no hashes, contents, or length —
            // so an entry in run.artifacts would add nothing; keep it out of the table and out
            // of SARIF2004's location-only artifact check.
            ArtifactLocation workingDirectory = node.WorkingDirectory;
            node.WorkingDirectory = null;

            Invocation result = base.VisitInvocation(node);

            result.WorkingDirectory = workingDirectory;
            return result;
        }
    }
}
