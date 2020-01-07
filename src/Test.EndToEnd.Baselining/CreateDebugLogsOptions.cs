using CommandLine;

namespace Test.EndToEnd.Baselining
{
    [Verb("create-debug-logs", HelpText = "Create human-debuggable logs from baseline output and expected logs")]
    public class CreateDebugLogsOptions
    {
        [Value(0, Required = true, HelpText = "Baseline E2E test content root")]
        public string TestRootPath { get; set; }
    }
}
