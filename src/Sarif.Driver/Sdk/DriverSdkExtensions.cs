using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class DriverSdkExtensions
    {
        public static IDictionary<string, FileLocation> ConstructUriBaseIdsDictionary(this CommonOptionsBase options)
        {
            if (options.UriBaseIds == null || options.UriBaseIds.Count() == 0) { return null; }

            var uriBaseIdsDictionary = new Dictionary<string, FileLocation>();

            foreach (string uriBaseId in options.UriBaseIds)
            {
                string[] tokens = uriBaseId.Split('=');
                string key = tokens[0];

                string uriToken = tokens[1];

                Uri value = new Uri(uriToken, UriKind.RelativeOrAbsolute);

                if (!value.IsAbsoluteUri)
                {
                    // Command-line tools may be required to provide relative paths for 
                    // various operations. We will help resolve these to absolute paths
                    // where we can.

                    try
                    {
                        uriToken = Path.GetFullPath(uriToken);
                        if (!Uri.TryCreate(uriToken, UriKind.Absolute, out value))
                        {
                            throw new InvalidOperationException("Could not construct absolute URI from specified value: " + tokens[1]);
                        }
                    }
                    catch (IOException) { }
                    catch (SecurityException) { }
                }

                uriBaseIdsDictionary[key] = new FileLocation { Uri = value };
            }

            return uriBaseIdsDictionary;
        }
    }
}
