// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebRequest
    {
        [JsonIgnore]
        public bool IsInvalid { get; private set; }

        public static WebRequest Parse(string requestString)
        {
            var webRequest = new WebRequest();

            if (!string.IsNullOrEmpty(requestString))
            {
                string[] lines = requestString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                int lineIndex = 0;
                string requestLine = lines[0];
                string[] fields = requestLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length > 0)
                {
                    webRequest.Method = fields[0];
                }

                if (fields.Length > 1)
                {
                    webRequest.Target = fields[1];
                }

                if (fields.Length > 2)
                {
                    bool success = WebMessageUtilities.ParseProtocolAndVersion(fields[0], out string protocol, out string version);
                    if (success)
                    {
                        webRequest.Protocol = protocol;
                        webRequest.Version = version;
                    }
                    else
                    {
                        webRequest.IsInvalid = true;
                    }
                }

                if (fields.Length < 3)
                {
                    webRequest.IsInvalid = true;
                }

                ++lineIndex;
            }
            else
            {
                webRequest.IsInvalid = true;
            }

            return webRequest;
        }
    }
}
