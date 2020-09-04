// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class GitHubDspIngestionVisitor : SarifRewritingVisitor
    {
        // DSP requires that every related location have a message. It's not clear why this
        // requirement exists, as this data is mostly used to build embedded links from
        // results (where the link anchor text actually resides).
        private const string PlaceholderRelatedLocationMessage = "[No message provided.]";

        // GitHub DSP reportedly has an ingestion limit of 500 issues.
        // Internal static rather than private const to allow a unit test with a practical limit.
        internal static int s_MaxResults = 500;

        private IList<Artifact> artifacts;
        private IList<ThreadFlowLocation> threadFlowLocations;

        public override Run VisitRun(Run node)
        {
            this.artifacts = node.Artifacts;
            this.threadFlowLocations = node.ThreadFlowLocations;

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

                        if (errors.Count == s_MaxResults) { break; }
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

            // DSP requires threadFlowLocations to be inlined in the result,
            // not referenced from run.threadFlowLocations.
            node.ThreadFlowLocations = null;

            return node;
        }

        public override ThreadFlowLocation VisitThreadFlowLocation(ThreadFlowLocation node)
        {
            if (this.threadFlowLocations != null && node.Index > -1)
            {
                ThreadFlowLocation sharedLocation = this.threadFlowLocations[node.Index];

                // Location.Message might vary per usage. For example, on one usage,
                // we might be tracking an uninitialized variable, and the location
                // might have the message "Uninitialized variable 'ptr' passed to 'f'".
                // Another usage of the same location might have a different message,
                // or none at all. So even though we are getting the location from
                // the shared object, don't take its message unless the current object
                // does not have one.
                Message message = node.Location?.Message;

                node.Location = sharedLocation.Location;

                if (message != null)
                {
                    // Make sure there's a place to put the message. threadFlowLocation.Location
                    // is not required, so the shared object might not have had one.
                    if (node.Location == null)
                    {
                        node.Location = new Location();
                    }

                    node.Location.Message = message;
                }

                // Copy other properties that should be the same for each usage of the
                // shared location.
                node.Kinds = sharedLocation.Kinds;
                node.Module = sharedLocation.Module;

                // Merge properties from the shared location, preferring the properties from
                // this location if there are any duplicates.
                MergeProperties(node, sharedLocation);

                // Do NOT copy properties that can be different for each use of the shared
                // threadFlowLocation:
                //     - ExecutionOrder
                //     - ExecutionTimeUtc
                //     - Importance
                //     - NestingLevel
                //     - Stack
                //     - State
                //     - Taxa
                //     - WebRequest
                //     - WebResponse

                node.Index = -1;
            }

            return base.VisitThreadFlowLocation(node);
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (this.artifacts != null && node.Index > -1)
            {
                ArtifactLocation sharedLocation = this.artifacts[node.Index].Location;
                node.Uri = sharedLocation.Uri;
                node.UriBaseId = sharedLocation.UriBaseId;

                // Merge properties from the shared location, preferring the properties from
                // this location if there are any duplicates.
                MergeProperties(node, sharedLocation);

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
                    if (string.IsNullOrEmpty(relatedLocation.Message?.Text))
                    {
                        relatedLocation.Message = new Message
                        {
                            Text = PlaceholderRelatedLocationMessage
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
                foreach (string fingerprintKey in node.Fingerprints.Keys)
                {
                    node.PartialFingerprints ??= new Dictionary<string, string>();
                    node.PartialFingerprints[fingerprintKey] = node.Fingerprints[fingerprintKey];
                }
                node.Fingerprints = null;
            }

            return base.VisitResult(node);
        }

        // Merge properties from a source to a target, preferring the existing properties
        // on the target if there are any duplicates.
        private static void MergeProperties(PropertyBagHolder target, PropertyBagHolder source)
        {
            // Are there any properties to merge?
            if (source.Properties != null)
            {
                // If so, make sure there's someplace to put them.
                if (target.Properties == null)
                {
                    target.Properties = new Dictionary<string, SerializedPropertyInfo>();
                }

                target.Properties = target.Properties.MergePreferFirst(source.Properties);
            }
        }
    }
}
