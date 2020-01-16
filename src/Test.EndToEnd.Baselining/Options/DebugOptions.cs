// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;

namespace Test.EndToEnd.Baselining.Options
{
    [Verb("debug", HelpText = "Debug Baselining for a specific Series, Log, and Result")]
    public class DebugOptions : OptionsBase
    {
        [Value(1, Required = true, HelpText = "Debug: Series Path (path string after 'Input/')")]
        public string DebugSeriesPath { get; set; }

        [Value(2, Required = false, Default = -1, HelpText = "Debug: Log Index")]
        public int DebugLogIndex { get; set; }

        [Value(3, Required = false, Default = -1, HelpText = "Debug: Result Index")]
        public int DebugResultIndex { get; set; }
    }
}
