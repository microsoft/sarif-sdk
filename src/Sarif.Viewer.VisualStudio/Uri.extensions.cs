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
        static List<string> s_httpSchemes = new List<string>() { Uri.UriSchemeHttp, Uri.UriSchemeHttps };

        public static string ToPath(this Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            if (uri.IsAbsoluteUri)
            {
                if (IsHttpScheme(uri))
                {
                    return uri.ToString();
                }
                else
                {
                    return uri.LocalPath + uri.Fragment;
                }
            }
            else
            {
                return uri.OriginalString;
            }
        }

        public static bool IsHttpScheme(this Uri uri)
        {
            return s_httpSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase);
        }
    }
}
