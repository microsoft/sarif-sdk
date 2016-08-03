// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class UriExtensions
    {
        static List<String> s_schemes = new List<string>() { Uri.UriSchemeHttp, Uri.UriSchemeHttps };
        static List<String> s_hosts = new List<string>() { "raw.githubusercontent.com" };

        public static string ToPath(this Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            if (s_schemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase)
                && s_hosts.Contains(uri.Host))
            {
                return uri.ToString();
            }
            else
            {
                return uri.LocalPath;
            }
        }
    }
}
