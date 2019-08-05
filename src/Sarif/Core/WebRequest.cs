// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebRequest
    {
        public static bool TryParse(string requestString, out WebRequest webRequest)
        {
            bool succeeded = false;
            webRequest = null;

            try
            {
                webRequest = Parse(requestString);
                succeeded = true;
            }
            catch (Exception)
            {
            }

            return succeeded;
        }

        public static WebRequest Parse(string requestString)
        {
            var webRequest = new WebRequest();

            webRequest.ParseRequestLine(requestString, out int requestLineLength);

            requestString = requestString.Substring(requestLineLength);
            webRequest.Headers = WebMessageUtilities.ParseHeaderLines(requestString, out int totalHeadersLength);

            if (requestString.Length > totalHeadersLength)
            {
                webRequest.Body = new ArtifactContent
                {
                    Text = requestString.Substring(totalHeadersLength)
                };
            }

            if (Uri.TryCreate(webRequest.Target, UriKind.RelativeOrAbsolute, out Uri uri))
            {
                string query = GetQueryFromUri(uri);
                if (!string.IsNullOrEmpty(query))
                {
                    webRequest.Parameters = ParseParametersFromQueryString(query);
                }
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

        internal void ParseRequestLine(
            string requestString,
            out int length)
        {
            Match match = s_requestLineRegex.Match(requestString);
            if (!match.Success)
            {
                throw new ArgumentException($"Invalid request line: '{WebMessageUtilities.Truncate(requestString)}'", nameof(requestString));
            }

            this.Method = match.Groups["method"].Value;
            this.Target = match.Groups["target"].Value;
            this.Protocol = match.Groups["protocol"].Value;
            this.Version = match.Groups["version"].Value;

            length = match.Length;
        }

        private static string GetQueryFromUri(Uri uri)
        {
            // uri.Query throws an exception if uri is not absolute, so first make it absolute.
            uri = SynthesizeAbsoluteUriFrom(uri);

            return uri.Query;
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

        // In this query pattern, note how backtracking is disabled with (?>...) while parsing the
        // parameter names and values. A parameter name can't contain a '=', and a parameter value
        // can't contain a '&', so it's never necessary to backtrack over an individual parameter
        // name or value.
        const string QueryPattern =
            @"^
              \?                        # A literal '?', followed by zero or more of...
              (
                (?>
                  (?<name>[^=]+)        # a parameter name (everything that's not an = sign),
                )
                =                       # an equal sign, and
                (
                  (?>
                    (?<value>[^&]*)     # the value (everything that's not an '&')...
                  )
                  &?                    # and an '&' (except after the last one).
                )
              )*
              $";

        private static readonly Regex s_queryRegex = SarifUtilities.RegexFromPattern(QueryPattern);

        private static IDictionary<string, string> ParseParametersFromQueryString(string query)
        {
            IDictionary<string, string> parameterDictionary = new Dictionary<string, string>();

            Match match = s_queryRegex.Match(query);

            // RFC 3986 does _not_ require a URI's query to consist of key-value pairs. If it
            // doesn't, don't fail; just return an empty parameter dictionary.
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

            return parameterDictionary;
        }
    }
}
