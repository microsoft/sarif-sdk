// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.WorkItemFiling
{
    public static class WorkItemFilingUtilities
    {
        internal const string PROJECT = "account";
        internal const string ACCOUNT = "project";

        private static readonly string GitHubUriPattern = $"^https://github\\.com/(?<{ACCOUNT}>[^/]+)/(?<{PROJECT}>[^/]+)$";
        private static readonly string AzureDevOpsUriPattern = $"^(?<{ACCOUNT}>https://dev\\.azure\\.com/[^/]+)/(?<{PROJECT}>[^/]+)$";
        private static readonly string LegacyAzureDevOpsUriPattern = $"^(?<{ACCOUNT}>https://[^\\.]+\\.visualstudio\\.com)/(?<{PROJECT}>[^/]+)$";

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
