// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// A visitor that, given a URI base id (e.g., "%SRCROOT%") and its value (e.g., "C:\src\root\"),
    /// rebases the URIs in a SARIF log to make the log independent of absolute paths (i.e., machine independent).
    /// </summary>
    public class RebaseUriVisitor : SarifRewritingVisitor
    {
        private Run _currentRun;
        private readonly Uri _baseUri;
        private readonly string _uriBaseId;
        private readonly bool _rebaseRelativeUris;

        /// <summary>
        /// Create a new instance of the RebaseUriVisitor class with the specified URI base id and its value.
        /// </summary>
        public RebaseUriVisitor(string uriBaseId, Uri baseUri, bool rebaseRelativeUris = false)
        {
            if (!baseUri.IsAbsoluteUri)
            {
                throw new ArgumentException($"{nameof(baseUri)} must be an absolute URI.", nameof(baseUri));
            }

            _baseUri = baseUri;
            _uriBaseId = uriBaseId;
            _rebaseRelativeUris = rebaseRelativeUris;
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (node.Uri == null)
            {
                return node;
            }

            if (_currentRun?.OriginalUriBaseIds?.Values.Contains(node) != true)
            {
                if (node.Uri.IsAbsoluteUri && _baseUri.IsBaseOf(node.Uri))
                {
                    node.UriBaseId = _uriBaseId;
                    node.Uri = _baseUri.MakeRelativeUri(node.Uri);
                }
                else if (_rebaseRelativeUris && !node.Uri.IsAbsoluteUri)
                {
                    node.UriBaseId = _uriBaseId;
                }
            }

            return node;
        }

        public override Run VisitRun(Run node)
        {
            _currentRun = node;

            Run newRun = base.VisitRun(node);

            newRun.OriginalUriBaseIds = newRun.OriginalUriBaseIds ?? new Dictionary<string, ArtifactLocation>();

            // Add dictionary entry if it doesn't exist, or replace it if it does.
            newRun.OriginalUriBaseIds[_uriBaseId] = new ArtifactLocation { Uri = _baseUri };

            return newRun;
        }
    }
}
