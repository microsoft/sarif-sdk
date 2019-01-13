// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// A class that, given a variable name (e.g. "%SRCROOT%") and a value (e.g. "C:\src\root\"), rebases the URIs in a SARIF log 
    /// in order to make the log independent of absolute paths (i.e., machine independent).
    /// </summary>
    public class RebaseUriVisitor : SarifRewritingVisitor
    {
        private readonly Uri _baseUri;
        private readonly string _uriBaseId;
        private readonly bool _rebaseRelativeUris;

        /// <summary>
        /// Create a RebaseUriVisitor, with a given name for the Base URI and a value for the base URI.
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

        public override FileLocation VisitFileLocation(FileLocation node)
        {
            FileLocation newNode = base.VisitFileLocation(node);

            if (newNode.Uri.IsAbsoluteUri && _baseUri.IsBaseOf(newNode.Uri))
            {
                newNode.UriBaseId = _uriBaseId;
                newNode.Uri = _baseUri.MakeRelativeUri(node.Uri);
            }
            else if (_rebaseRelativeUris && !newNode.Uri.IsAbsoluteUri)
            {
                newNode.UriBaseId = _uriBaseId;
            }

            return newNode;
        }

        public override Run VisitRun(Run node)
        {
            Run newRun = base.VisitRun(node);

            newRun.OriginalUriBaseIds = newRun.OriginalUriBaseIds ?? new Dictionary<string, FileLocation>();

            // Note--this is an add or update, so if this is run twice with the same base variable, we'll replace the path.
            newRun.OriginalUriBaseIds[_uriBaseId] = new FileLocation { Uri =_baseUri };

            return newRun;
        }
    }
}
