// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;

using CommandLine;

using Test.EndToEnd.Baselining.Options;

namespace Test.EndToEnd.Baselining
{
    /// <summary>
    ///  Test.EndToEnd.Baselining is a console app for conducting end-to-end testing of the baselining algorithm.
    ///  It uses sample SARIF logs, some of which contain sensitive data, so the test content is not in this repository.
    ///  The tests are too slow to run as unit tests and are only applicable when changing the Baselining algorithm.
    ///  
    ///  Process
    ///  =======
    ///  - When the baselining algorithm is changed, run these tests to check for differences across a wide set of data.
    ///  - Determine whether the new algorithm appears 'better' overall and investigate specific Results which don't match as they should.
    ///  - Save the Output logs as the new baseline once you've decided to accept them.
    /// 
    ///  
    ///  Content Folder Structure
    ///  ========================
    ///  BaselineE2E\                      [Test Root Path]
    ///    Input\                          [SARIF log series to test]
    ///      CloudMine-Spam\               [Series can be organized in any folder structure; using Tool\Version\ContentType\Series here]
    ///        v1.2.12\              
    ///          WorkItem\
    ///            mseng\                  [A folder with files in it is an individual series]
    ///              Baseline.sarif        [A starting Baseline before any logs are loaded]
    ///              20191203_1530.sarif   [Each other SARIF log will be baselined in order by name; use DateTime prefixes to order them]
    ///              20191204_1135.sarif   [...and use the file name suffix to associate them with their source]
    ///              
    ///    Output\                         [Each test run will create an Output folder with the results]
    ///      Summary.log                   [There is a summary log with one line per series showing overall stats]
    ///      CloudMine-Spam\...            [Output mirrors the Input folder structure]
    ///        mseng.log                   [Each series has a detail log which shows what happened to each Result in each Log]
    ///        
    ///   Expected\                        [If there is an 'Expected' folder, detail logs will be compared between Output and Expected]
    ///     CloudMine-Spam\...
    ///        mseng.log                   [Rename 'Output' to 'Expected' to capture the current state as the new baseline]
    ///        
    ///   Output_Debug\                    [These are a copy of the 'Output' logs with more than just the Result GUIDs included]
    ///     CloudMine-Spam\...            
    ///       mseng.log                    [Change the code in BaseliningDetailEnricher to add the Result properties you need to investigate differences quickly]
    ///   
    ///   Expected_Debug\                  [Each run will create an 'enriched' version of the Expected logs also]
    ///     
    /// 
    ///  Execution Steps
    ///  ===============
    ///  Running tests will find each series folder in Input and:
    ///    - Load the initial baseline
    ///    - Load each log in order
    ///    - Replace the Result.GUID with a 'RID' (LogIndex + ' ' + ResultIndex)
    ///    - Baseline the current log to the Baseline-so-far
    ///    - Write detail report lines about how each Result was baselined
    ///    - Save the output as the Baseline-so-far
    ///    - Compare the output to the same file in 'Expected\', if present
    ///    - Write 'enriched' logs to Output_Debug\ and Expected_Debug\ for human investigation.
    ///  
    ///    
    ///  Usage
    ///  =====
    ///   Get Test Content:
    ///   git clone https://github.com/microsoft/sarif-sdk-test-content
    ///   
    ///   Run a Full Pass:
    ///   "Test.EndToEnd.Baselining run C:\Code\sarif-sdk-test-content\BaselineE2E"
    ///   
    ///   Debug matching for Result 004 in log 001 in series "Spam\v1\WorkItem\mseng":
    ///   To quickly debug a Result, copy and paste the first line from the Series log for the Series path and the RID (ex: 001 004) from the Result to debug.
    ///   "Test.EndToEnd.Baselining debug C:\Code\sarif-sdk-test-content\BaselineE2E Spam\v1\WorkItem\mseng 001 004"
    ///   
    ///   Re-generate debug logs with the details as coded in BaseliningDetailEnricher:
    ///   "Test.EndToEnd.Baselining rebuild-debug-logs C:\Code\sarif-sdk-test-content\BaselineE2E"
    /// </summary>
    internal class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                Program program = new Program();

                return Parser.Default.ParseArguments<RunOptions, DebugOptions, RebuildDebugLogsOptions>(args).MapResult(
                    (RunOptions options) => Program.Run(options),
                    (DebugOptions options) => Program.Debug(options),
                    (RebuildDebugLogsOptions options) => Program.CreateDebugLogs(options),
                    errs => 1);
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.WriteLine(ex.ToString());
                return 2;
            }
        }

        private static int Run(RunOptions options)
        {
            BaseliningTester tester = new BaseliningTester();
            BaseliningSummary overallSummary = tester.RunAll(options.TestRootPath);
            
            return 0;
        }

        private static int Debug(DebugOptions options)
        {
            BaseliningTester tester = new BaseliningTester();
            tester.RunSeries(Path.Combine(options.TestRootPath, BaseliningTester.InputFolderName, options.DebugSeriesPath), options.DebugSeriesPath, options.DebugLogIndex, options.DebugResultIndex);

            return 0;
        }

        private static int CreateDebugLogs(RebuildDebugLogsOptions options)
        {
            BaseliningTester tester = new BaseliningTester();
            tester.EnrichUnder(Path.Combine(options.TestRootPath, BaseliningTester.InputFolderName));

            return 0;
        }
    }
}