// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Verb("export-validation-rules-documentation", HelpText = "Export the documentation for the validation rules to a Markdown file.")]
    public class ExportValidationRulesDocumentationOptions : ExportRulesDocumentationOptions
    {
    }
}
