// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class DriverSdkExtensions
    {
        public static IDictionary<string, ArtifactLocation> ConstructUriBaseIdsDictionary(this CommonOptionsBase options)
        {
            if (options.UriBaseIds == null || options.UriBaseIds.Count() == 0) { return null; }

            var uriBaseIdsDictionary = new Dictionary<string, ArtifactLocation>();

            foreach (string uriBaseId in options.UriBaseIds)
            {
                string[] tokens = uriBaseId.Split('=');
                string key = tokens[0];

                string uriToken = tokens[1];

                Uri value = null;

                try
                {
                    value = new Uri(uriToken, UriKind.RelativeOrAbsolute);

                    if (!value.IsAbsoluteUri)
                    {
                        value = null;

                        // Command-line tools may be required to provide relative paths for 
                        // various operations. We will help resolve these to absolute paths
                        // where we can.

                        try
                        {
                            uriToken = Path.GetFullPath(uriToken);
                            Uri.TryCreate(uriToken, UriKind.Absolute, out value);
                        }
                        catch (ArgumentException) { } // illegal file path characters throw this
                        catch (PathTooLongException) { }
                    }
                }
                catch (UriFormatException) { }

                if (value == null)
                {
                    throw new InvalidOperationException("Could not construct absolute URI from specified value: " + tokens[1]);
                }

                uriBaseIdsDictionary[key] = new ArtifactLocation { Uri = value };
            }

            return uriBaseIdsDictionary;
        }
    }
}
