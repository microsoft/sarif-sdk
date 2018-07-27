// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class FileLocation
    {
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
