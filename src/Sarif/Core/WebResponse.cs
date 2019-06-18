// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebResponse
    {
        [JsonIgnore]
        public bool IsInvalid { get; private set; }

        [JsonIgnore]
        public string ProtocolVersion => WebMessageUtilities.MakeProtocolVersion(Protocol, Version);

        public static WebResponse Parse(string responseString)
        {
            var webResponse = new WebResponse();

            string[] lines = responseString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            int lineIndex = 0;
            if (lines.Length > 0)
            {
                string requestLine = lines[0];
                string[] fields = requestLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length > 0)
                {
                    bool success = WebMessageUtilities.ParseProtocolAndVersion(fields[0], out string protocol, out string version);
                    if (success)
                    {
                        webResponse.Protocol = protocol;
                        webResponse.Version = version;
                    }
                    else
                    {
                        webResponse.IsInvalid = true;
                    }
                }

                if (fields.Length > 1 && int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out int statusCode))
                {
                    webResponse.StatusCode = statusCode;
                }

                if (fields.Length > 2)
                {
                    webResponse.ReasonPhrase = fields[2];
                }

                if (fields.Length < 3)
                {
                    webResponse.IsInvalid = true;
                }

                ++lineIndex;
            }
            else
            {
                webResponse.IsInvalid = true;
            }

            return webResponse;
        }
    }
}
