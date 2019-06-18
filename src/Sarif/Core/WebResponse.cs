// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebResponse
    {
        [JsonIgnore]
        public bool IsInvalid { get; private set; }

        [JsonIgnore]
        public string HttpVersion { get; private set; }

        public static WebResponse Parse(string responseString)
        {
            var webResponse = new WebResponse();

            if (WebMessageUtilities.ParseStatusLine(
                responseString,
                out string httpVersion,
                out string protocol,
                out string version,
                out int statusCode,
                out string reasonPhrase,
                out int statusLineLength))
            {
                webResponse.HttpVersion = httpVersion;
                webResponse.Protocol = protocol;
                webResponse.Version = version;
                webResponse.StatusCode = statusCode;
                webResponse.ReasonPhrase = reasonPhrase;

                responseString = responseString.Substring(statusLineLength);
                if (WebMessageUtilities.ParseHeaderLines(responseString, out Dictionary<string, string> headers, out int totalHeadersLength))
                {
                    webResponse.Headers = headers;
                }
                else
                {
                    webResponse.IsInvalid = true;
                    return webResponse;
                }

                responseString = responseString.Substring(totalHeadersLength);
            }
            else
            {
                webResponse.IsInvalid = true;
                return webResponse;
            }

            return webResponse;
        }
    }
}
