// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Test.UnitTests.Sarif.Driver.Net48
{
    [Verb("analyze-test", HelpText = "Test the analysis driver framework.")]
    public class AnalyzeTestOptions : AnalyzeOptionsBase
    {
    }
}
