// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public class FilingClientFactory
    {
        private const string LegacyAzureDevOpsUriPattern = @"^https://[^\.]+\.visualstudio\.com/[^/]+$";
        private static readonly Regex s_legacyAzureDevOpsUriRegex = SarifUtilities.RegexFromPattern(LegacyAzureDevOpsUriPattern);

        private const string AzureDevOpsUriPattern = @"^https://dev\.azure\.com/[^/]+/[^/]+$";
        private static readonly Regex s_azureDevOpsUriRegex = SarifUtilities.RegexFromPattern(AzureDevOpsUriPattern);

        private const string GitHubUriPattern = @"^https://github\.com/[^/]+/[^/]+$";
        private static readonly Regex s_gitHubUriRegex = SarifUtilities.RegexFromPattern(GitHubUriPattern);


        // https://secretscantest.visualstudio.com/RTAKET-DELM

        public static FilingClient CreateFilingTarget(string projectUriString)
        {
            if (projectUriString == null) { throw new ArgumentNullException(nameof(projectUriString)); }

            // TODO: make this code populate project/account/org details 
            if (s_azureDevOpsUriRegex.IsMatch(projectUriString))
            {
                return new AzureDevOpsClient();
            }
            else if (s_legacyAzureDevOpsUriRegex.IsMatch(projectUriString))
            {
                return new AzureDevOpsClient();
            }
            else if (s_gitHubUriRegex.IsMatch(projectUriString))
            {
                return new GitHubClient();
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "'{0}' is not a recognized target URI for work item filing. Work items can be filed to GitHub or AzureDevOps.",
                        projectUriString));
            }
        }
    }
}
