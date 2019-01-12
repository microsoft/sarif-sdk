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

        public override FileLocation VisitFileLocation(FileLocation node)
        {
            if ( _run.OriginalUriBaseIds!= null &&
                !string.IsNullOrEmpty(node?.UriBaseId) &&
                _run.OriginalUriBaseIds.ContainsKey(node.UriBaseId))
            {
                Uri baseUri = _run.GetExpandedUriBaseIdValue(node.UriBaseId);
                node.Uri = CombineUris(baseUri, node.Uri);
                node.UriBaseId = null;
            }

            return node;
        }

        private Uri CombineUris(Uri baseUri, Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
