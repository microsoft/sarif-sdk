// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebResponse
    {
        [JsonIgnore]
        public string HttpVersion { get; private set; }

        public static WebResponse Parse(string responseString)
        {
            var webResponse = new WebResponse();

            ParseStatusLine(
                responseString,
                out string httpVersion,
                out string protocol,
                out string version,
                out int statusCode,
                out string reasonPhrase,
                out int statusLineLength);

            webResponse.HttpVersion = httpVersion;
            webResponse.Protocol = protocol;
            webResponse.Version = version;
            webResponse.StatusCode = statusCode;
            webResponse.ReasonPhrase = reasonPhrase;

            responseString = responseString.Substring(statusLineLength);
            WebMessageUtilities.ParseHeaderLines(responseString, out Dictionary<string, string> headers, out int totalHeadersLength);
            webResponse.Headers = headers;

            if (responseString.Length > totalHeadersLength)
            {
                webResponse.Body = new ArtifactContent
                {
                    Text = responseString.Substring(totalHeadersLength)
                };
            }

            return webResponse;
        }

        private const string StatusLinePattern =
            @"^
            (?<httpVersion>" + WebMessageUtilities.HttpVersionPattern + @") # The HTTP version, e.g., 'HTTP/1.1',
            \x20                                                            # followed by a single space (which we must write this way
                                                                            # because we're using RegexOptions.IgnorePatternWhitespace),
            (?<statusCode>\d\d\d)                                           # a 3-digit status code,
            \x20                                                            # another space,
            (?<reasonPhrase>.*?)                                            # and the 'reason phrase', which we match non-greedy (.*?)
            \r\n                                                            # so that it doesn't include the trailing CRLF.
            ";

        private static readonly Regex s_statusLineRegex = SarifUtilities.RegexFromPattern(StatusLinePattern);

        internal static void ParseStatusLine(
            string responseString,
            out string httpVersion,
            out string protocol,
            out string version,
            out int statusCode,
            out string reasonPhrase,
            out int length)
        {
            Match match = s_statusLineRegex.Match(responseString);
            if (!match.Success)
            {
                throw new ArgumentException($"Invalid status line: '{WebMessageUtilities.Truncate(responseString)}'", nameof(responseString));
            }

            httpVersion = match.Groups["httpVersion"].Value;
            protocol = match.Groups["protocol"].Value;
            version = match.Groups["version"].Value;
            statusCode = int.Parse(match.Groups["statusCode"].Value);
            reasonPhrase = match.Groups["reasonPhrase"].Value;

            length = match.Length;
        }
    }
}
