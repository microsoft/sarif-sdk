// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class ArtifactLocation
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
        public bool TryReconstructAbsoluteUri(IDictionary<string, ArtifactLocation> originalUriBaseIds, out Uri resolvedUri)
        {
            resolvedUri = null;

            // If this artifactLocation represents a top level originalUriBaseId, then the value of the URI may be absent.
            if (this.Uri == null) { return false; }

            if (this.Uri.IsAbsoluteUri)
            {
                resolvedUri = this.Uri;
                return true;
            }

            // The URI is a relative reference. Do we have the information we need to resolve it?
            if (originalUriBaseIds == null) { return false; }

            // Walk up the UriBaseId chain until we find an ArtifactLocation object:
            //    - whose Uri is absolute (success), or
            //      which lacks a UriBaseId property (failure), or
            //    - whose UriBaseId is not defined in originalUriBaseIds (failure), or
            //    - which lacks a Uri property (failure).
            ArtifactLocation artifactLocation = this;
            Uri stemUri = this.Uri;
            while (!stemUri.IsAbsoluteUri)
            {
                if (string.IsNullOrEmpty(artifactLocation.UriBaseId) ||
                    !originalUriBaseIds.TryGetValue(artifactLocation.UriBaseId, out artifactLocation) ||
                    artifactLocation.Uri == null)
                {
                    return false;
                }

                // I'd like to use the ctor new Uri(baseUri, relativeUri) here, but it fails with
                // ArgumentOutOfRangeException, perhaps because it doesn't like the baseUri argument
                // to be relative. So...
                string artifactLocationOriginalUriString = artifactLocation.Uri.OriginalString;
                if (!artifactLocationOriginalUriString.EndsWith("/")) { artifactLocationOriginalUriString += "/"; }
                stemUri = new Uri(artifactLocationOriginalUriString + stemUri.OriginalString, UriKind.RelativeOrAbsolute);
            }

            // If we got here, we found an absolute URI.
            resolvedUri = stemUri;
            return true;
        }

        public static ArtifactLocation CreateFromFilesDictionaryKey(string key, string parentKey = null)
        {
            string uriBaseId = null;
            string originalKey = key;

            // A parent key indicates we're looking at an item that's nested within a container
            if (!string.IsNullOrEmpty(parentKey))
            {
                key = originalKey.Substring(parentKey.Length).Trim(new[] { '#' });
            }
            else if (key.StartsWith("#"))
            {
                string[] tokens = key.Split(new[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                uriBaseId = tokens[0];

                // +2 to skip past leading and trailing octothorpes
                key = key.Substring(uriBaseId.Length + 2);
            }

            // At this point, if the key still contains an octothorpe, we are dealing with a 
            // reference to a nested item (which we didn't identify because the caller to this
            // utility did not have the parent key in hand).
            if (key.Contains("#"))
            {
                key = key.Substring(key.IndexOf('#')).Trim(new[] { '#' });

                // A uriBaseId is only valid for a location that refers to a root container
                uriBaseId = null;
            }

            return new ArtifactLocation()
            {
                Uri = new Uri(UriHelper.MakeValidUri(key), UriKind.RelativeOrAbsolute),
                UriBaseId = uriBaseId
            };
        }

        public ArtifactLocation Resolve(Run run)
        {
            return Index >= 0 && Index < run?.Artifacts?.Count
                ? run.Artifacts[Index].Location
                : this;
        }
    }
}
