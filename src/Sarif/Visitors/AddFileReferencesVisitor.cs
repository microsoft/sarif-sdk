// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class AddFileReferencesVisitor : SarifRewritingVisitor
    {
        IDictionary<string, FileData> _files;

        public override Run VisitRun(Run node)
        {
            _files = node.Files;

            Run result = base.VisitRun(node);
            result.Files = _files;

            return result;
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            _files = _files ?? new Dictionary<string, FileData>();

            string uriText = node.Uri.ToString();

            string mimeType = Writers.MimeType.DetermineFromFileExtension(uriText);

            _files[uriText] = new FileData()
            {
                MimeType = mimeType
            };

            return base.VisitPhysicalLocation(node);
        }
    }
}
