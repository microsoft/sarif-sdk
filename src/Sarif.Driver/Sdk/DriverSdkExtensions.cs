using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class DriverSdkExtensions
    {
        public static IDictionary<string, Uri> ConstructUriBaseIdsDictionary(this CommonOptionsBase options)
        {
            if (options.UriBaseIds == null) { return null; }

            IDictionary<string, Uri> uriBaseIdsDictionary = new Dictionary<string, Uri>();

            foreach (string uriBaseId in options.UriBaseIds)
            {
                string[] tokens = uriBaseId.Split('=');
                string key = tokens[0];
                Uri value = new Uri(tokens[1], UriKind.Absolute);
                uriBaseIdsDictionary[key] = value;
            }

            return uriBaseIdsDictionary;
        }
    }
}
