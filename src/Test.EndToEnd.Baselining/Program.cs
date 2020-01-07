using System;
using System.Diagnostics;
using System.IO;
using CommandLine;

namespace Test.EndToEnd.Baselining
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Program program = new Program();

                return Parser.Default.ParseArguments<TestBaseliningOptions, CreateDebugLogsOptions>(args).MapResult(
                    (TestBaseliningOptions options) => Program.TestBaselining(options),
                    (CreateDebugLogsOptions options) => Program.CreateDebugLogs(options),
                    errs => 1);
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.WriteLine(ex.ToString());
                return 2;
            }
        }

        static int TestBaselining(TestBaseliningOptions options)
        {
            // Debugging:
            //     1. Change Common.props to reference the Sarif SDK as a project (for the desired SDK version)
            //     2. In the output logs, find a Result which matched unexpectedly
            //     3. Copy the log path (top line) and new Result ID (### ### on the '+' or '=' line) as arguments
            //     4. Re-run. The harness will compare the desired series only and stop before comparing the specified Log/Result.

            BaseliningTester tester = new BaseliningTester();

            if (!string.IsNullOrEmpty(options.DebugSeriesPath))
            {
                // Debug Mode: Debug the desired log in the desired series
                tester.RunSeries(Path.Combine(options.TestRootPath, options.DebugSeriesPath), options.DebugLogIndex, options.DebugResultIndex);
            }
            else
            {
                // Run Mode: Run baselining for everything in the input folder
                BaseliningSummary overallSummary = tester.RunAll(options.TestRootPath);
                Console.WriteLine($"{overallSummary}");
            }

            return 0;
        }

        static int CreateDebugLogs(CreateDebugLogsOptions options)
        {
            BaseliningTester tester = new BaseliningTester();
            tester.EnrichUnder(options.TestRootPath);
            return 0;
        }
    }
}