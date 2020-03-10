// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class MakeUrisAbsoluteVisitor : SarifRewritingVisitor
    {
        private Run _run;

        public override Run VisitRun(Run node)
        {
            _run = node;
            return base.VisitRun(node);
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (_run.OriginalUriBaseIds != null &&
                !string.IsNullOrEmpty(node?.UriBaseId) &&
                _run.OriginalUriBaseIds.ContainsKey(node.UriBaseId) &&
                !_run.OriginalUriBaseIds.Values.Contains(node))
            {
                Uri baseUri = _run.ExpandUrisWithUriBaseId(node.UriBaseId);
                node.Uri = CombineUris(baseUri, node.Uri);
                node.UriBaseId = null;
            }

            return node;
        }

        internal static Uri CombineUris(Uri absoluteBaseUri, Uri relativeUri)
        {
            if (!absoluteBaseUri.IsAbsoluteUri)
            {
                throw new ArgumentException($"{nameof(absoluteBaseUri)} is not an absolute URI", nameof(absoluteBaseUri));
            }

            if (relativeUri.IsAbsoluteUri)
            {
                throw new ArgumentException($"${nameof(relativeUri)} is not a relative URI", nameof(relativeUri));
            }

            return new Uri(absoluteBaseUri, relativeUri);
        }
    }
}
