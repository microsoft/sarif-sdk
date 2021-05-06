// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Test.EndToEnd.Baselining.Options
{
    public class OptionsBase
    {
        [Value(0, Required = true, HelpText = "Baseline E2E test content root")]
        public string TestRootPath { get; set; }
    }
}
