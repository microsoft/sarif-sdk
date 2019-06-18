// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebRequest
    {
        [JsonIgnore]
        public bool IsInvalid { get; private set; }

        [JsonIgnore]
        public string HttpVersion { get; private set; }

        public static WebRequest Parse(string requestString)
        {
            var webRequest = new WebRequest();

            if (WebMessageUtilities.ParseRequestLine(requestString, out string method, out string target, out string httpVersion, out string protocol, out string version, out int length))
            {
                webRequest.Method = method;
                webRequest.Target = target;
                webRequest.HttpVersion = httpVersion;
                webRequest.Protocol = protocol;
                webRequest.Version = version;
            }
            else
            {
                webRequest.IsInvalid = true;
                return webRequest;
            }

            return webRequest;
        }
    }
}
