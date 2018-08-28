﻿// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class FileLocation
    {
        /// <summary>
        /// Attempt to reconstruct a URI, if appropriate, using Run instance 
        /// originalUriBaseId and uriBaseId properties. If this method cannot
        /// successfully reconstitute an absolute URI, it will return false
        /// and populate 'resolvedUri' with null.
        /// </summary>
        /// <param name="fileLocation">The fileLocatio instance from which an absolute URI should be reconstructed, if possible.</param>
        /// <param name="originalUriBaseIds">The original uri base id values associated with the tool run.</param>
        /// <param name="resolvedUri">The reconstructed absolute URI or null (if an absolute URI cannot be reconstructed).</param>
        /// <returns></returns>
        public bool TryReconstructAbsoluteUri(IDictionary<string, Uri> originalUriBaseIds, out Uri resolvedUri)
        {
            resolvedUri = this.Uri.IsAbsoluteUri ? this.Uri : null;

            // We can't restore any absolute URIs unless someone has
            // deconstructed them using uriBaseId + originalUriBaseIds
            if (originalUriBaseIds == null) { return this.Uri.IsAbsoluteUri; }

            resolvedUri = this.Uri;

            if (!string.IsNullOrEmpty(this.UriBaseId) &&
                !this.Uri.IsAbsoluteUri)
            {
                if (originalUriBaseIds.TryGetValue(this.UriBaseId, out Uri originalUriBaseId))
                {
                    resolvedUri = new Uri(originalUriBaseId, resolvedUri.ToString());
                }
            }

            // If we weren't able to reconstruct a URI (or resolvedUri wasn't already 
            // an absolute URI on input), initialize out parameter to null;
            if (!resolvedUri.IsAbsoluteUri)
            {
                resolvedUri = null;
            }

            return resolvedUri != null;
        }
        public static FileLocation CreateFromFilesDictionaryKey(string key)
        {
            string uriBaseId = null;

            if (key.StartsWith("#"))
            {
                string[] tokens = key.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                uriBaseId = tokens[0];

                // +2 to skip past leading and trailing octothorpes
                key = key.Substring(uriBaseId.Length + 2);
            }

            return new FileLocation()
            {
                 Uri = new Uri(key, UriKind.RelativeOrAbsolute),
                 UriBaseId = uriBaseId
            };
        }
    }
}
