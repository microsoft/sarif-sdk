// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;
using System;
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
            // Strictly speaking, some elements that may contribute to a files table 
            // key are case sensitive, e.g., everything but the schema and protocol of a
            // web URI. We don't have a proper comparer implementation that can handle 
            // all cases. For now, we cover the Windows happy path, which assumes that
            // most URIs in log files are file paths (which are case-insensitive)
            //
            // Tracking item for an improved comparer:
            // https://github.com/Microsoft/sarif-sdk/issues/973
            _files = _files ?? new Dictionary<string, FileData>(StringComparer.OrdinalIgnoreCase);

            FileLocation fileLocation = node.FileLocation;

            string uriText = Uri.EscapeUriString(fileLocation.Uri.ToString());

            if (!string.IsNullOrEmpty(fileLocation.UriBaseId))
            {
                // See EXAMPLE 3 of 3.11.13.2 'Property Names' of
                // SARIF v2 'files' property specification 
                uriText = "#" + fileLocation.UriBaseId + "#" + uriText;
            }

            // If the file already exists, we will not insert one as we want to 
            // preserve mime-type, hash details, and other information that 
            // may already be present
            if (!_files.ContainsKey(uriText))
            {
                string mimeType = Writers.MimeType.DetermineFromFileExtension(uriText);

                _files[uriText] = new FileData()
                {
                    MimeType = mimeType,
                    FileLocation = fileLocation
                };
            }

            return base.VisitPhysicalLocation(node);
        }
    }
}
