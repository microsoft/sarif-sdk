// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommandLine;

namespace Test.EndToEnd.Baselining.Options
{
    [Verb("rebuild-debug-logs", HelpText = "Regenerate human-debuggable logs from raw logs in Output and Expected")]
    public class RebuildDebugLogsOptions : OptionsBase
    { }
}
