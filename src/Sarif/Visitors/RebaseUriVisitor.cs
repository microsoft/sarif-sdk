// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class RebaseUriVisitor : SarifRewritingVisitor
    {
        private string _baseName;
        private Uri _baseUri;

        public RebaseUriVisitor(string baseName, Uri baseUri)
        {
            _baseName = baseName;
            _baseUri = baseUri;
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            PhysicalLocation newNode = base.VisitPhysicalLocation(node);
            // TODO...  this does not work yet.
            newNode.UriBaseId = _baseName;
            newNode.Uri = node.Uri.MakeRelativeUri(_baseUri);

            return newNode;
        }

        public override Run VisitRun(Run node)
        {
            Run newRun = base.VisitRun(node);
            if(!node.Properties.ContainsKey("BLAH"))
            {
                throw new NotImplementedException();
            }

            newRun.SetProperty(_baseName, _baseUri);
            return newRun;
        }
    }
}
