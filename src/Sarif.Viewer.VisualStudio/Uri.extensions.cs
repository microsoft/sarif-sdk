// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class UriExtensions
    {
        // The acceptable URI schemes
        static List<String> s_schemes = new List<string>() { Uri.UriSchemeHttp, Uri.UriSchemeHttps };
        // The acceptable URI hosts
        static List<String> s_hosts = new List<string>() { "raw.githubusercontent.com" };

        public static string ToPath(this Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            // For http(s)://raw.githubusercontent.com return the absolute URI.
            // If the file needs to be displayed, the CodeAnalsyisResultManager will download the file before 
            // displaying it in the IDE. 
            // For all other URIs, return the local path.
            if (uri.IsAbsoluteUri
                && s_schemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase)
                && s_hosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
            {
                return uri.ToString();
            }
            else
            {
                if (uri.IsAbsoluteUri)
                {
                    return uri.LocalPath;
                }
                else
                {
                    return uri.OriginalString;
                }
            }
        }
    }
}
