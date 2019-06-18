// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebResponse
    {
        [JsonIgnore]
        public bool IsInvalid { get; private set; }

        [JsonIgnore]
        public string ProtocolVersion => (Protocol ?? string.Empty) + "/" + (Version ?? string.Empty);

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
                    webResponse.ParseProtocolAndVersion(fields[0]);
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

        private const string HttpVersionPattern = @"(?<protocol>[^/]+)/(?<version>.+)";
        private static readonly Regex s_httpVersionRegex = SarifUtilities.RegexFromPattern(HttpVersionPattern);

        private void ParseProtocolAndVersion(string httpVersion)
        {
            Match match = s_httpVersionRegex.Match(httpVersion);
            if (match.Success)
            {
                Protocol = match.Groups["protocol"].Value;
                Version = match.Groups["version"].Value;
            }
            else
            {
                Protocol = httpVersion;
                IsInvalid = true;
            }
        }
    }
}
