using System;
using System.Collections.Generic;
using System.Linq;

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
                Uri value = new Uri(tokens[1], UriKind.Absolute);
                uriBaseIdsDictionary[key] = new FileLocation { Uri = value };
            }

            return uriBaseIdsDictionary;
        }
    }
}
