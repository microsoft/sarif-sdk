// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class UriHelper
    {
        /// <summary>
        /// Create a syntactically valid URI from a path that might be
        /// absolute or relative, and that might require percent-encoding.
        /// </summary>
        /// <param name="path">
        /// The path to be transformed into a syntactically valid URI.
        /// </param>
        /// <returns>
        /// A syntactically valid URI representing <paramref name="path"/>.
        /// </returns>
        /// <remarks>
        /// In general, <paramref name="path"/> might be:
        /// 
        /// 1. Possible to interpret as an absolute path / absolute URI
        /// 2. Possible to interpret as a relative path / relative URI
        /// 3. Neither
        ///
        /// We must create a valid URI to persist in the SARIF log. We proceed as follows:
        ///
        /// 1. Try to create an absolute System.Uri. If that succeeds, take its
        /// AbsoluteUri, which (unlike Uri.ToString()) will be properly percent-encoded.
        ///
        /// 2. Try to create a relative System.Uri. If that succeeds, we want to write it out,
        /// but since this is a relative URI, we can't access its AbsoluteUri or AbsolutePath
        /// property -- and again, Uri.ToString() does not perform percent encoding.
        /// 
        /// We use this workaround:
        /// 
        ///     a. Combine the relative path with an arbitrary scheme and host to form
        ///        an absolute URI.
        ///     b. Extract the AbsolutePath property, which will be percent encoded.
        ///     
        ///
        /// 3. If all else fails, we have a string that we can't convert to a System.Uri,
        /// so just percent encode the whole thing. This should be extremely rare in practice.
        ///
        /// Thanks and a tip o' the hat to @nguerrera for this code (and for the comment).
        /// </remarks>
        public static string MakeValidUri(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            Uri uri;
            string validUri;
            if (Uri.TryCreate(path, UriKind.Absolute, out uri))
            {
                validUri = uri.AbsoluteUri;
            }
            else if (Uri.TryCreate(path, UriKind.Relative, out uri))
            {
                UriBuilder builder = new UriBuilder("http", "www.example.com", 80, path);
                validUri = builder.Uri.AbsolutePath;

                // Since what we actually want is a relative path, strip the leading "/"
                // from the AbsolutePath -- unless the input string started with "/".
                if (!path.StartsWith("/", StringComparison.Ordinal) &&
                    !path.StartsWith(@"\", StringComparison.Ordinal))
                {
                    validUri = validUri.Substring(1);

                    // When the UriBuilder constructs an absolute URI, it strips any
                    // leading "." and ".." segments ("dot-segments", as RFC 3986 calls
                    // them). Glue them back on so we don't lose the relative path
                    // information.
                    string leadingDotSegments = GetLeadingDotSegments(path);
                    if (!string.IsNullOrEmpty(leadingDotSegments))
                    {
                        validUri = leadingDotSegments + validUri;
                    }
                }
            }
            else
            {
                validUri = System.Net.WebUtility.UrlEncode(path);
            }

            return validUri;
        }

        private static readonly Regex s_oneDotPattern =
            new Regex(@"^\.[\\/]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex s_twoDotPattern =
            new Regex(@"^\.\.[\\/]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static string GetLeadingDotSegments(string path)
        {
            var sb = new StringBuilder();

            bool moreDotSegments = true;
            while (moreDotSegments)
            {
                if (s_oneDotPattern.IsMatch(path))
                {
                    path = path.Substring(2);
                }
                else if (s_twoDotPattern.IsMatch(path))
                {
                    path = path.Substring(3);
                    sb.Append("../");
                }
                else
                {
                    moreDotSegments = false;
                }
            }

            // Corner case: the path is entirely composed of a single two-dot segment,
            // or ends with a two-dot segment.
            if (path.Equals("..", StringComparison.Ordinal))
            {
                sb.Append("..");
            }

            return sb.ToString();
        }
    }
}
