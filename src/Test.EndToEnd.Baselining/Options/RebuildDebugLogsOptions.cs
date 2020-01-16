// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CommandLine;

namespace Test.EndToEnd.Baselining.Options
{
    [Verb("rebuild-debug-logs", HelpText = "Regenerate human-debuggable logs from raw logs in Output and Expected")]
    public class RebuildDebugLogsOptions : OptionsBase
    { }
}
