// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public class FilingClientFactory
    {
        private const string GitHubUriPattern = @"^https://github\.com/([^/]+)/([^/]+)$";
        private const string AzureDevOpsUriPattern = @"^(https://dev\.azure\.com/[^/]+)/([^/]+)$";
        private const string LegacyAzureDevOpsUriPattern = @"^(https://[^\.]+\.visualstudio\.com)/([^/]+)$";


        private static readonly Tuple<string, Regex>[] s_regexTuples = new[]
        {
            new Tuple<string, Regex>("github", SarifUtilities.RegexFromPattern(GitHubUriPattern)),
            new Tuple<string, Regex>("ado", SarifUtilities.RegexFromPattern(AzureDevOpsUriPattern)),
            new Tuple<string, Regex>("ado", SarifUtilities.RegexFromPattern(LegacyAzureDevOpsUriPattern))
        };


        public static FilingClient CreateFilingTarget(string filingHostUriString)
        {
            if (filingHostUriString == null) { throw new ArgumentNullException(nameof(filingHostUriString)); }

            if (!Uri.TryCreate(filingHostUriString, UriKind.Absolute, out Uri filingHostUri))
            {
                throw new ArgumentException(nameof(filingHostUriString));
            }

            FilingClient filingClient = null;

            foreach (Tuple<string, Regex> regexTuple in s_regexTuples)
            {
                bool isGitHub = regexTuple.Item1.Equals("github");
                Regex regex = regexTuple.Item2;

                Match match = regex.Match(filingHostUriString);
                if (match.Success)
                {
                    filingClient = isGitHub ? (FilingClient)new GitHubClientWrapper() : new AzureDevOpsClientWrapper();
                    filingClient.AccountOrOrganization = match.Groups[1].Value;
                    filingClient.ProjectOrRepository = match.Groups[2].Value;
                    break;
                }
            }

            if (filingClient == null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "'{0}' is not a recognized target URI for work item filing. Work items can be filed to GitHub or AzureDevOps "+
                        "(with URIs such as https://github.com/microsoft/sarif-sdk or https://dev.azure.com/contoso/contoso-project).",
                        filingHostUriString));
            }

            filingClient.HostUri = filingHostUri;

            return filingClient;
        }
    }
}
