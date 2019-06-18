// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
        private const string HttpVersionPattern = @"(?<protocol>[^/]+)/(?<version>.+)";
        private static readonly Regex s_httpVersionRegex = SarifUtilities.RegexFromPattern(HttpVersionPattern);

        internal static string MakeProtocolVersion(string protocol, string version)
            => (protocol ?? string.Empty) + "/" + (version ?? string.Empty);

        internal static bool ParseProtocolAndVersion(string httpVersion, out string protocol, out string version)
        {
            protocol = version = null;

            Match match = s_httpVersionRegex.Match(httpVersion);
            if (match.Success)
            {
                protocol = match.Groups["protocol"].Value;
                version = match.Groups["version"].Value;
            }

            return match.Success;
        }

        private const string TokenPattern = "[!#$%&'*+._`|~0-9a-zA-Z^-]+";

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

        internal static bool ParseRequestLine(string requestString, out string method, out string target, out string httpVersion, out string protocol, out string version, out int length)
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

        private const string HeaderPattern =
            @"^
              (?<fieldName>" + TokenPattern + @")       # The field name, which must be a token,
              :                                         # immediately followed by a colon,
              \s*                                       # optional white space,
              (?<fieldValue>.*?)                        # and the field value, which is a non-greedy match (.*?)
              \s*                                       # so that it doesn't include the optional trailing white space.
              $";

        private static readonly Regex s_headerRegex = SarifUtilities.RegexFromPattern(HeaderPattern);

        internal static bool ParseHeaderLine(string headerLine, out string fieldName, out string fieldValue)
        {
            fieldName = fieldValue = null;

            Match match = s_headerRegex.Match(headerLine);
            if (match.Success)
            {
                fieldName = match.Groups["fieldName"].Value;
                fieldValue = match.Groups["fieldValue"].Value;
            }

            return match.Success;
        }

        internal static bool ValidateMethod(string method)
        {
            return ValidateToken(method);
        }

        private const string TokenOnlyPattern = "^" + TokenPattern + "$";
        private static readonly Regex s_tokenOnlyRegex = SarifUtilities.RegexFromPattern(TokenOnlyPattern);

        private static bool ValidateToken(string token)
        {
            return s_tokenOnlyRegex.IsMatch(token);
        }
    }
}
