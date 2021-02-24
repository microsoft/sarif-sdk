// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("kusto", HelpText = "TODO.")]
    public class KustoOptions : CommonOptionsBase
    {
        [Value(
            0,
            HelpText = "Output path for exported SARIF",
            Required = true)]
        public string OutputFilePath { get; set; }

        [Option(
            "host-address",
            HelpText = "TODO",
            Required = true)]
        public string HostAddress { get; set; }

        [Option(
            "database",
            HelpText = "TODO",
            Required = true)]
        public string Database { get; set; }

        [Option(
            "query",
            HelpText = "TODO",
            Required = true)]
        public string Query { get; set; }

        public bool Validate()
        {
            string appClientId = Environment.GetEnvironmentVariable("AppClientId");
            string appSecret = Environment.GetEnvironmentVariable("AppSecret");
            string authorityId = Environment.GetEnvironmentVariable("AuthorityId");

            if (string.IsNullOrEmpty(appClientId) ||
                string.IsNullOrEmpty(appSecret) ||
                string.IsNullOrEmpty(authorityId))
            {
                return false;
            }

            return true;
        }
    }
}
