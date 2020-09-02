// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{ 
    public class GitHubDspIngestionVisitor : SarifRewritingVisitor
    {
        // GitHub DSP reportedly has an ingestion limit of 500 issues.
        // Internal static rather than private const to allow a unit test with a practical limit.
        internal static int s_MaxResults = 500;

        private IList<Artifact> artifacts;

        public override Run VisitRun(Run node)
        {
            this.artifacts = node.Artifacts;
            
            // DSP does not support submitting invocation objects. Invocations
            // contains potentially sensitive environment details, such as 
            // account names embedded in paths. Invocations also store 
            // notifications of catastrophic tool failures, however, which 
            // means there is current no mechanism for reporting these to
            // DSP users in context of the security tab.
            node.Invocations = null;

            if (node.Results != null)
            {
                int errorsCount = 0;
                foreach (Result result in node.Results)
                {
                    if (result.Level == FailureLevel.Error)
                    {
                        errorsCount++;
                    }
                }
                
                if (errorsCount != node.Results.Count)
                {
                    var errors = new List<Result>();

                    foreach (Result result in node.Results)
                    {
                        if (result.Level == FailureLevel.Error)
                        {
                            errors.Add(result);
                        }

                        if (errors.Count > s_MaxResults) { break; }
                    }

                    node.Results = errors;
                }

                if (node.Results.Count > s_MaxResults)
                {
                    node.Results = node.Results.Take(s_MaxResults).ToList();
                }
            }
            
            node = base.VisitRun(node);

            // DSP prefers a relative path local to the result. We clear
            // the artifacts table, as all artifact information is now
            // inlined with each result.
             node.Artifacts = null;

            return node;
        }


        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (this.artifacts != null && node.Index > -1)
            {
                node.Uri = this.artifacts[node.Index].Location.Uri;
                node.UriBaseId = this.artifacts[node.Index].Location.UriBaseId;
                node.Index = -1;
            }
            return base.VisitArtifactLocation(node);
        }

        public override Result VisitResult(Result node)
        {
            if (node.RelatedLocations != null)
            {
                foreach (Location relatedLocation in node.RelatedLocations)
                {
                    // DSP requires that every related location have a message. It's
                    // not clear why this requirement exists, as this data is mostly 
                    // used to build embedded links from results (where the link
                    // anchor text actually resides).
                    if (string.IsNullOrEmpty(relatedLocation.Message?.Text))
                    {
                        relatedLocation.Message = new Message
                        {
                            Text = "[No message provided.]" 
                        };
                    }
                }
            }

            if (node.Fingerprints != null)
            {
                // DSP appears to require that fingerprints be emitted to the
                // partial fingerprints property in order to prefer these
                // values for matching (over DSP's built-in SARIF-driven
                // results matching heuristics).
                foreach(string fingerprintKey in node.Fingerprints.Keys)
                {
                    node.PartialFingerprints ??= new Dictionary<string, string>();
                    node.PartialFingerprints[fingerprintKey] = node.Fingerprints[fingerprintKey];
                }
                node.Fingerprints = null;
            }

            node.CodeFlows = null;

            return base.VisitResult(node);
        }
    }
}
