// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Options for <c>publish-to-ghas</c>, which uploads a finalized SARIF file to GitHub Advanced
    /// Security code scanning. The GitHub target, commit, and ref are derived from the run's
    /// version-control provenance, and the bearer token is read from an environment variable named by
    /// <c>--token-env-var</c>, never from the command line.
    /// </summary>
    [Verb("publish-to-ghas", HelpText = "Upload a SARIF file to GitHub Advanced Security code scanning. The target, commit, and ref are derived from the run's versionControlProvenance; the token is read from the environment variable named by --token-env-var.")]
    public class PublishToGhasOptions
    {
        [Value(
            0,
            MetaName = "<sarif-file>",
            HelpText = "The finalized SARIF file to upload. Its first run must carry a GitHub repositoryUri, revisionId, and branch under versionControlProvenance.",
            Required = true)]
        public string SarifPath { get; set; }

        [Option(
            "token-env-var",
            HelpText = "Name of the environment variable holding the GitHub bearer token (a classic or fine-grained PAT with security_events write). The token is never accepted on the command line.",
            Default = "GHAS_TOKEN")]
        public string TokenEnvironmentVariable { get; set; }

        [Option(
            "dry-run",
            HelpText = "Resolve the target, ref, commit, and request shape and print them without contacting the server. The token value is never printed.",
            Default = false)]
        public bool DryRun { get; set; }
    }
}
