// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// This class provides utilities common to the parsing of <see cref="WebRequest"/>
    /// and <see cref="WebResponse"/> objects from the raw request and response strings.
    /// </summary>
    /// <remarks>
    /// Ideally, this would have been a base class for both WebRequest and WebResponse.
    /// Unfortunately, every class in the SARIF SDK's object model derives from
    /// <see cref="PropertyBagHolder"/>, so the .NET class inheritance point is already
    /// spent.
    /// </remarks>
    internal static class WebMessageUtilities
    {
        private const string CRLF = "\r\n";
        private const string TokenPattern = "[!#$%&'*+._`|~0-9a-zA-Z^-]+";
        private const string HttpVersionPattern = @"(?<protocol>HTTP)/(?<version>[0-9]\.[0-9])";

        private const string RequestLinePattern =
            @"^
            (?<method>" + TokenPattern + @")            # The method, which is a token,
            \x20                                        # followed by a single space (which we must write this way
                                                        # because we're using RegexOptions.IgnorePatternWhitespace),
            (?<target>[^\s]+)                           # the target URI, which we don't validate further,
            \x20                                        # another space,
            (?<httpVersion>" + HttpVersionPattern + @") # and the HTTP version, e.g., 'HTTP/1.1'.
            \r\n";

        private static readonly Regex s_requestLineRegex = SarifUtilities.RegexFromPattern(RequestLinePattern);

        internal static bool ParseRequestLine(
            string requestString,
            out string method,
            out string target,
            out string httpVersion,
            out string protocol,
            out string version,
            out int length)
        {
            method = target = httpVersion = protocol = version = null;
            length = -1;

            Match match = s_requestLineRegex.Match(requestString);
            if (match.Success)
            {
                method = match.Groups["method"].Value;
                target = match.Groups["target"].Value;
                httpVersion = match.Groups["httpVersion"].Value;
                protocol = match.Groups["protocol"].Value;
                version = match.Groups["version"].Value;

                length = match.Length;
            }

            return match.Success;
        }

        private const string StatusLinePattern =
            @"^
            (?<httpVersion>" + HttpVersionPattern + @") # The HTTP version, e.g., 'HTTP/1.1',
            \x20                                        # followed by a single space (which we must write this way
                                                        # because we're using RegexOptions.IgnorePatternWhitespace),
            (?<statusCode>\d\d\d)                       # a 3-digit status code,
            \x20                                        # another space,
            (?<reasonPhrase>.*?)                        # and the 'reason phrase', which we match non-greedy (.*?)
            \r\n                                        # so that it doesn't include the trailing CRLF.
            ";

        private static readonly Regex s_statusLineRegex = SarifUtilities.RegexFromPattern(StatusLinePattern);

        internal static bool ParseStatusLine(
            string responseString,
            out string httpVersion,
            out string protocol,
            out string version,
            out int statusCode,
            out string reasonPhrase,
            out int length)
        {
            httpVersion = protocol = version = reasonPhrase = null;
            statusCode = length = -1;

            Match match = s_statusLineRegex.Match(responseString);
            if (match.Success)
            {
                httpVersion = match.Groups["httpVersion"].Value;
                protocol = match.Groups["protocol"].Value;
                version = match.Groups["version"].Value;
                statusCode = int.Parse(match.Groups["statusCode"].Value);
                reasonPhrase = match.Groups["reasonPhrase"].Value;

                length = match.Length;
            }

            return match.Success;
        }

        private const string HeaderPattern =
            @"^
              (?<fieldName>" + TokenPattern + @")       # The field name, which must be a token,
              :                                         # immediately followed by a colon,
              \s*                                       # optional white space,
              (?<fieldValue>.*?)                        # and the field value, which is a non-greedy match (.*?)
              \s*?                                      # so that it doesn't include the optional trailing white space...
              \r\n                                      # ... or the CRLF.
                                                        # The pattern does _not_ include '$' because there might
                                                        # be more headers, or a body, to follow"
              ;

        private static readonly Regex s_headerRegex = SarifUtilities.RegexFromPattern(HeaderPattern);

        internal static bool ParseHeaderLines(string requestString, out Dictionary<string, string> headers, out int totalLength)
        {
            headers = new Dictionary<string, string>();
            totalLength = 0;
             
            do
            {
                if (ParseHeaderLine(requestString, out string fieldName, out string fieldValue, out int length))
                {
                    if (headers.ContainsKey(fieldName))
                    {
                        return false;
                    }

                    headers.Add(fieldName, fieldValue);
                    requestString = requestString.Substring(length);
                    totalLength += length;
                }
                else
                {
                    return false;
                }
            } while (!requestString.StartsWith(CRLF));  // An empty line signals the end of the headers.

            totalLength += CRLF.Length;                 // Skip past the empty line;

            return true;
        }

        internal static bool ParseHeaderLine(string headerLine, out string fieldName, out string fieldValue, out int length)
        {
            fieldName = fieldValue = null;
            length = -1;

            Match match = s_headerRegex.Match(headerLine);
            if (match.Success)
            {
                fieldName = match.Groups["fieldName"].Value;
                fieldValue = match.Groups["fieldValue"].Value;
                length = match.Length;
            }

            return match.Success;
        }
    }
}
