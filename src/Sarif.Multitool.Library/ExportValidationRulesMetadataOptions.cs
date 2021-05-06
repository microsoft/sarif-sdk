// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("export-validation-rules", HelpText = "Export validation rules metadata to a SARIF or SonarQube XML file.")]
    public class ExportValidationRulesMetadataOptions : ExportRulesMetadataOptions
    {
    }
}
