// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("export-validation-docs", HelpText = "Export the documentation for the validation rules to a Markdown file.")]
    public class ExportValidationDocumentationOptions : ExportRulesDocumentationOptions
    {
    }
}
