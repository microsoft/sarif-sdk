// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebRequest
    {
        [JsonIgnore]
        public string HttpVersion { get; private set; }

        public static WebRequest Parse(string requestString)
        {
            var webRequest = new WebRequest();

            ParseRequestLine(
                requestString,
                out string method,
                out string target,
                out string httpVersion,
                out string protocol,
                out string version,
                out int requestLineLength);

            webRequest.Method = method;
            webRequest.Target = target;
            webRequest.HttpVersion = httpVersion;
            webRequest.Protocol = protocol;
            webRequest.Version = version;

            requestString = requestString.Substring(requestLineLength);
            WebMessageUtilities.ParseHeaderLines(requestString, out Dictionary<string, string> headers, out int totalHeadersLength);
            webRequest.Headers = headers;

            if (requestString.Length > totalHeadersLength)
            {
                webRequest.Body = new ArtifactContent
                {
                    Text = requestString.Substring(totalHeadersLength)
                };
            }

            if (Uri.TryCreate(webRequest.Target, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                webRequest.Parameters = ParseQueryParametersFromUri(uri);
            }

            return webRequest;
        }

        private const string RequestLinePattern =
            @"^
            (?<method>" + WebMessageUtilities.TokenPattern + @")            # The method, which is a token,
            \x20                                                            # followed by a single space (which we must write this way
                                                                            # because we're using RegexOptions.IgnorePatternWhitespace),
            (?<target>[^\s]+)                                               # the target URI, which we don't validate further,
            \x20                                                            # another space,
            (?<httpVersion>" + WebMessageUtilities.HttpVersionPattern + @") # and the HTTP version, e.g., 'HTTP/1.1'.
            \r\n";

        private static readonly Regex s_requestLineRegex = SarifUtilities.RegexFromPattern(RequestLinePattern);

        internal static void ParseRequestLine(
            string requestString,
            out string method,
            out string target,
            out string httpVersion,
            out string protocol,
            out string version,
            out int length)
        {
            Match match = s_requestLineRegex.Match(requestString);
            if (!match.Success)
            {
                throw new ArgumentException($"Invalid request line: '{WebMessageUtilities.Truncate(requestString)}'", nameof(requestString));
            }

            method = match.Groups["method"].Value;
            target = match.Groups["target"].Value;
            httpVersion = match.Groups["httpVersion"].Value;
            protocol = match.Groups["protocol"].Value;
            version = match.Groups["version"].Value;

            length = match.Length;
        }

        private static IDictionary<string, string> ParseQueryParametersFromUri(Uri uri)
        {
            // uri.Query throws an exception if uri is not absolute, so first make it absolute.
            uri = SynthesizeAbsoluteUriFrom(uri);

            return ParseParametersFromQueryString(uri.Query);
        }

        private static Uri SynthesizeAbsoluteUriFrom(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                var sb = new StringBuilder("http://www.example.com");
                if (!uri.OriginalString.StartsWith("/"))
                {
                    sb.Append("/");
                }

                sb.Append(uri.OriginalString);
                uri = new Uri(sb.ToString(), UriKind.Absolute);
            }

            return uri;
        }

        const string QueryPattern =
            @"^
              \?                              # A literal '?', followed by zero or more
              (
                (?<name>[^=])                 # parameter name (everything that's not an = sign),
                =                             # an equal sign, and
                (
                  (?<value>[^&]*)             # the value (everything that's not an '&')...
                  &?                          # and an '&' (except after the last one).
                )
              )*";

        private static readonly Regex s_queryRegex = SarifUtilities.RegexFromPattern(QueryPattern);

        private static IDictionary<string, string> ParseParametersFromQueryString(string query)
        {
            Dictionary<string, string> parameterDictionary = null;
            if (query != null)
            {
                Match match = s_queryRegex.Match(query);
                if (match.Success)
                {
                    List<string> names = match.Groups["name"].Captures.Cast<Capture>().Select(c => c.Value).ToList();
                    List<string> values = match.Groups["value"].Captures.Cast<Capture>().Select(c => c.Value).ToList();

                    if (names.Count == values.Count)
                    {
                        parameterDictionary = new Dictionary<string, string>();
                        for (int i = 0; i < names.Count; ++i)
                        {
                            parameterDictionary.Add(names[i], values[i]);
                        }
                    }
                }
            }

            return parameterDictionary;
        }
    }
}
