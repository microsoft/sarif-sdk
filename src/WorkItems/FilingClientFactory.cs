// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.WorkItems
{
    public class FilingClientFactory
    {
        public static FilingClient Create(Uri filingHostUri)
        {
            filingHostUri = filingHostUri ?? throw new ArgumentNullException(nameof(filingHostUri));

            string filingUriString = filingHostUri.OriginalString;

            FilingClient filingClient = null;

            foreach (Tuple<string, Regex> regexTuple in WorkItemFilingUtilities.WorkItemHostRegexTuples)
            {
                bool isGitHub = regexTuple.Item1.Equals("github");
                Regex regex = regexTuple.Item2;

                Match match = regex.Match(filingUriString);
                if (match.Success)
                {
                    filingClient = isGitHub ? (FilingClient)new GitHubFilingClient() : new AzureDevOpsFilingClient();
                    filingClient.Provider = isGitHub ? FilingClient.SourceControlProvider.Github : FilingClient.SourceControlProvider.AzureDevOps;
                    filingClient.ProjectOrRepository = match.Groups[WorkItemFilingUtilities.PROJECT].Value;
                    filingClient.AccountOrOrganization = match.Groups[WorkItemFilingUtilities.ACCOUNT].Value;
                    break;
                }
            }

            if (filingClient == null)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "'{0}' is not a recognized target URI for work item filing. Work items can be filed to GitHub or AzureDevOps " +
                        "(with URIs such as https://github.com/microsoft/sarif-sdk or https://dev.azure.com/contoso/contoso-project).",
                        filingUriString));
            }

            return filingClient;
        }
    }
}
