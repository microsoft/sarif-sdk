// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.SarifValidator
{
    internal class Options
    {
        [Option(
            's',
            "schema-file-path",
            HelpText = "Path of the SARIF JSON schema file",
            Default = "Sarif.schema.json")]
        public string SchemaFilePath { get; set; }

        [Option(
            'i',
            "instance-file-path",
            HelpText = "Path of the SARIF file to validate",
            Required = true)]
        public string InstanceFilePath { get; set; }

        [Option(
            'l',
            "log-file-path",
            HelpText = "Path to SARIF log file containing the results of the validation",
            Default = "sarif.log")]
        public string LogFilePath { get; set; }
    }
}
