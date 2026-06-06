// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>publish-to-ghazdo</c>, which uploads a finalized SARIF file to GitHub Advanced
    /// Security for Azure DevOps. The Azure DevOps target is derived from the run's version-control
    /// provenance, and the bearer secret is read from an environment variable named by
    /// <c>--token-env-var</c>, never from the command line.
    /// </summary>
    [Verb("publish-to-ghazdo", HelpText = "Upload a SARIF file to GitHub Advanced Security for Azure DevOps. The target is derived from the run's versionControlProvenance; the secret is read from the environment variable named by --token-env-var.")]
    public class PublishToGhazdoOptions
    {
        [Value(
            0,
            MetaName = "<sarif-file>",
            HelpText = "The finalized SARIF file to upload. Its first run must carry an Azure DevOps repositoryUri under versionControlProvenance.",
            Required = true)]
        public string SarifPath { get; set; }

        [Option(
            "token-env-var",
            HelpText = "Name of the environment variable holding the bearer secret (an Azure DevOps PAT or an Entra access token). The secret is never accepted on the command line.",
            Default = "GHAZDO_TOKEN")]
        public string TokenEnvironmentVariable { get; set; }

        [Option(
            "api-version",
            HelpText = "The Advanced Security SARIF ingestion API version.",
            Default = "7.2-preview.1")]
        public string ApiVersion { get; set; }

        [Option(
            "dry-run",
            HelpText = "Resolve the target, scheme, and request shape and print them without contacting the server. The secret value is never printed.",
            Default = false)]
        public bool DryRun { get; set; }
    }
}
