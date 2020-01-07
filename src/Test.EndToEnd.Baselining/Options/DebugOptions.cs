using CommandLine;

namespace Test.EndToEnd.Baselining.Options
{
    [Verb("debug", HelpText = "Debug Baselining for a specific Series, Log, and Result")]
    public class DebugOptions : OptionsBase
    {
        [Value(1, Required = false, HelpText = "Debug: Series Path (everything under 'Input/'")]
        public string DebugSeriesPath { get; set; }

        [Value(2, Required = false, HelpText = "Debug: Log Index")]
        public int DebugLogIndex { get; set; }

        [Value(3, Required = false, HelpText = "Debug: Result Index")]
        public int DebugResultIndex { get; set; }
    }
}
