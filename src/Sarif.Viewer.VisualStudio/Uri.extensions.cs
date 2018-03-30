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
        static List<string> s_schemes = new List<string>() { Uri.UriSchemeHttp, Uri.UriSchemeHttps };

        public static string ToPath(this Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            if (uri.IsAbsoluteUri)
            {
                if (s_schemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase))
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
    }
}
