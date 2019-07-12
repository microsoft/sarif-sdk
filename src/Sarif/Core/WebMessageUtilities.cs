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
        // When including the contents of a web request or response in an exception message, truncate
        // it to this length:
        private const int MaxExceptionMessageStringLength = 200;

        // These patterns are taken from the grammar defined in RFC 7230, "Hypertext Transfer Protocol (HTTP/1.1):
        // Message Syntax and Routing". Yes, tokens (such as message header field names) really can have all those
        // weird characters.
        //
        // We are a little looser than the RFC in a couple of places. In the request, we don't verify that the
        // "target" field on the request line is a valid URI. Nor do we verify that the request body or response
        // body contains only "VCHAR"s (visible characters as opposed to control characters), as required by
        // the RFC.
        internal const string CRLF = "\r\n";
        internal const string TokenPattern = "[!#$%&'*+._`|~0-9a-zA-Z^-]+";
        internal const string HttpVersionPattern = @"(?<protocol>HTTP)/(?<version>[0-9]\.[0-9])";

        private const string HeaderPattern =
            @"^
              (?<fieldName>" + TokenPattern + @")       # The field name, which must be a token,
              :                                         # immediately followed by a colon,
              \s*                                       # optional white space,
              (?<fieldValue>.*?)                        # and the field value, which is a non-greedy match (.*?)
              \s*?                                      # so that it doesn't include the optional trailing white space...
              \r\n                                      # ... or the CRLF.
                                                        # The pattern does _not_ include '$' because there might
                                                        # be more headers, or a body, to follow."
              ;

        private static readonly Regex s_headerRegex = SarifUtilities.RegexFromPattern(HeaderPattern);

        internal static IDictionary<string, string> ParseHeaderLines(string requestString, out int totalLength)
        {
            var headers = new Dictionary<string, string>();
            totalLength = 0;

            while (!requestString.StartsWith(CRLF))     // An empty line signals the end of the headers.
            {
                ParseHeaderLine(requestString, out string fieldName, out string fieldValue, out int length);
                if (headers.ContainsKey(fieldName))
                {
                    throw new ArgumentException($"Duplicate header field name: {fieldName}", nameof(requestString));
                }

                headers.Add(fieldName, fieldValue);
                requestString = requestString.Substring(length);
                totalLength += length;
            }

            totalLength += CRLF.Length;                 // Skip past the empty line;

            return headers;
        }

        internal static void ParseHeaderLine(string headerLine, out string fieldName, out string fieldValue, out int length)
        {
            Match match = s_headerRegex.Match(headerLine);
            if (!match.Success)
            {
                throw new ArgumentException($"Invalid header line: '{Truncate(headerLine)}'.", nameof(headerLine));
            }

            fieldName = match.Groups["fieldName"].Value;
            fieldValue = match.Groups["fieldValue"].Value;
            length = match.Length;
        }

        internal static string Truncate(string s)
        {
            return s.Length <= MaxExceptionMessageStringLength
                ? s
                : s.Substring(0, MaxExceptionMessageStringLength) + "...";
        }
    }
}
