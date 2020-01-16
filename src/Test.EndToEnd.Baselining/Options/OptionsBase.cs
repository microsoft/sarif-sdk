// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;

namespace Test.EndToEnd.Baselining.Options
{
    public class OptionsBase
    {
        [Value(0, Required = true, HelpText = "Baseline E2E test content root")]
        public string TestRootPath { get; set; }
    }
}
