// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("export-validation-config", HelpText = "Export validation rule options to an XML or JSON file that can be edited and used to configure subsequent analysis.")]
    public class ExportValidationConfigurationOptions : ExportConfigurationOptions
    {
    }
}
