// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebResponse
    {
        public static bool TryParse(string responseString, out WebResponse webResponse)
        {
            bool succeeded = false;
            webResponse = null;

            try
            {
                webResponse = Parse(responseString);
                succeeded = true;
            }
            catch (Exception)
            {
            }

            return succeeded;
        }

        public static WebResponse Parse(string responseString)
        {
            var webResponse = new WebResponse();

            webResponse.ParseStatusLine(responseString, out int statusLineLength);

            responseString = responseString.Substring(statusLineLength);
            webResponse.Headers = WebMessageUtilities.ParseHeaderLines(responseString, out int totalHeadersLength);

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

        internal void ParseStatusLine(
            string responseString,
            out int length)
        {
            Match match = s_statusLineRegex.Match(responseString);
            if (!match.Success)
            {
                throw new ArgumentException($"Invalid status line: '{WebMessageUtilities.Truncate(responseString)}'", nameof(responseString));
            }

            this.Protocol = match.Groups["protocol"].Value;
            this.Version = match.Groups["version"].Value;
            this.StatusCode = int.Parse(match.Groups["statusCode"].Value);
            this.ReasonPhrase = match.Groups["reasonPhrase"].Value;

            length = match.Length;
        }
    }
}
