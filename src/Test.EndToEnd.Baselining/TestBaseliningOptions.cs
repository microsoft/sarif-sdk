using CommandLine;

namespace Test.EndToEnd.Baselining
{
    [Verb("test-baselining", HelpText = "Run Baselining E2E test on specified content")]
    public class TestBaseliningOptions
    {
        [Value(0, Required = true, HelpText = "Baseline E2E test content root")]
        public string TestRootPath { get; set; }

        [Value(1, Required = false, HelpText = "Debug: Series Path")]
        public string DebugSeriesPath { get; set; }

        [Value(2, Required = false, HelpText = "Debug: Log Index")]
        public int DebugLogIndex { get; set; }

        [Value(3, Required = false, HelpText = "Debug: Result Index")]
        public int DebugResultIndex { get; set; }
    }
}
