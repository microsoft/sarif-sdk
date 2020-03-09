// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.WorkItems
{
    public static class WorkItemFilingUtilities
    {
        internal const string PROJECT = "project";
        internal const string ACCOUNT = "account";

        private static readonly string GitHubUriPattern = $"^https://github\\.com/(?<{ACCOUNT}>[^/]+)/(?<{PROJECT}>[^/]+)$";
        private static readonly string AzureDevOpsUriPattern = $"^https://dev\\.azure\\.com/(?<{ACCOUNT}>[^/]+)/(?<{PROJECT}>[^/]+)$";
        private static readonly string LegacyAzureDevOpsUriPattern = $"^https://(?<{ACCOUNT}>[^\\.]+)\\.visualstudio\\.com/(?<{PROJECT}>[^/]+)$";

        internal static readonly Tuple<string, Regex>[] WorkItemHostRegexTuples = new[]
        {
            new Tuple<string, Regex>("github", WorkItemFilingUtilities.RegexFromPattern(GitHubUriPattern)),
            new Tuple<string, Regex>("ado", WorkItemFilingUtilities.RegexFromPattern(AzureDevOpsUriPattern)),
            new Tuple<string, Regex>("ado", WorkItemFilingUtilities.RegexFromPattern(LegacyAzureDevOpsUriPattern))
        };

        public static Regex RegexFromPattern(string pattern) =>
            new Regex(
                pattern,
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        internal static bool UnitTesting = false;
    }
}
