// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.Sarif.Sdk;
using Microsoft.CodeAnalysis.Sarif.Readers;

using Newtonsoft.Json;

using Xunit;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class AnalyzeCommandBaseTests
    {
        private void ExceptionTestHelper(
            ExceptionCondition exceptionCondition,
            RuntimeConditions runtimeConditions,
            ExitReason expectedExitReason = ExitReason.None,
            TestAnalyzeOptions analyzeOptions = null)
        {
            ExceptionRaisingRule.s_exceptionCondition = exceptionCondition;
            analyzeOptions = analyzeOptions ?? new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[0]
            };

            var command = new TestAnalyzeCommand();

            Assembly[] plugInAssemblies;

            if (analyzeOptions.PlugInFilePaths != null)
            {
                var assemblies = new List<Assembly>();
                foreach (string plugInFilePath in analyzeOptions.PlugInFilePaths)
                {
                    assemblies.Add(Assembly.LoadFrom(plugInFilePath));
                }
                plugInAssemblies = new Assembly[assemblies.Count];
                assemblies.CopyTo(plugInAssemblies, 0);
            }
            else
            {
                plugInAssemblies = new Assembly[] { typeof(ExceptionRaisingRule).Assembly };
            }

            command.DefaultPlugInAssemblies = plugInAssemblies;

            int result = command.Run(analyzeOptions);

            int expectedResult =
                (runtimeConditions & RuntimeConditions.Fatal) == RuntimeConditions.NoErrors ?
                    TestAnalyzeCommand.SUCCESS : TestAnalyzeCommand.FAILURE;

            Assert.Equal(runtimeConditions, command.RuntimeErrors);
            Assert.Equal(expectedResult, result);

            if (expectedExitReason != ExitReason.None)
            {
                Assert.NotNull(command.ExecutionException);

                if (expectedExitReason != ExitReason.UnhandledExceptionInEngine)
                {
                    var eax = command.ExecutionException as ExitApplicationException<ExitReason>;
                    Assert.NotNull(eax);
                }
            }
            else
            {
                Assert.Null(command.ExecutionException);
            }
            ExceptionRaisingRule.s_exceptionCondition = ExceptionCondition.None;
        }

        [Fact]
        public void NotApplicableToTarget()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                RegardAnalysisTargetAsNotApplicable = true
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.RuleNotApplicableToTarget,
                analyzeOptions: options);
        }


        [Fact]
        public void InvalidTarget()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                RegardAnalysisTargetAsValid = false
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.TargetNotValidToAnalyze,
                analyzeOptions: options);
        }

        [Fact]
        public void MissingRequiredConfiguration()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                RegardRequiredConfigurationAsMissing = true
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.RuleMissingRequiredConfiguration,
                analyzeOptions: options);
        }

        [Fact]
        public void ExceptionLoadingTarget()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                RegardAnalysisTargetAsCorrupted = true
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.ExceptionLoadingTargetFile,
                analyzeOptions: options);
        }

        [Fact]
        public void ExceptionRaisedInstantiatingSkimmers()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
            };

            ExceptionTestHelper(
                ExceptionCondition.InvokingConstructor,
                RuntimeConditions.ExceptionInstantiatingSkimmers,
                ExitReason.UnhandledExceptionInstantiatingSkimmers,
                analyzeOptions: options);
        }

        [Fact]
        public void NoRulesLoaded()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                PlugInFilePaths = new string[] { typeof(string).Assembly.Location }
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.NoRulesLoaded,
                ExitReason.NoRulesLoaded,
                analyzeOptions: options
            );
        }

        [Fact]
        public void NoValidAnalysisTargets()
        {
            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.NoValidAnalysisTargets,
                ExitReason.NoValidAnalysisTargets
            );
        }

        [Fact]
        public void ExceptionRaisedInvokingInitialize()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
            };

            ExceptionTestHelper(
                ExceptionCondition.InvokingInitialize,
                RuntimeConditions.ExceptionInSkimmerInitialize,
                analyzeOptions: options
            );
        }


        [Fact]
        public void LoadPdbException()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
            };

            ExceptionTestHelper(
                ExceptionCondition.LoadingPdb,
                RuntimeConditions.ExceptionLoadingPdb,
                analyzeOptions: options
            );
        }

        [Fact]
        public void ParseTargetException()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
            };

            ExceptionTestHelper(
                ExceptionCondition.ParsingTarget,
                RuntimeConditions.TargetParseError,
                analyzeOptions: options
            );
        }

        [Fact]
        public void ExceptionRaisedInvokingCanAnalyze()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
            };

            ExceptionTestHelper(
                ExceptionCondition.InvokingCanAnalyze,
                RuntimeConditions.ExceptionRaisedInSkimmerCanAnalyze,
                analyzeOptions: options
            );
        }

        [Fact]
        public void ExceptionRaisedInvokingAnalyze()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
            };

            ExceptionTestHelper(
                ExceptionCondition.InvokingAnalyze,
                RuntimeConditions.ExceptionInSkimmerAnalyze,
                analyzeOptions: options
            );
        }

        [Fact]
        public void ExceptionRaisedInEngine()
        {
            TestAnalyzeCommand.RaiseUnhandledExceptionInDriverCode = true;

            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.ExceptionInEngine,
                ExitReason.UnhandledExceptionInEngine,
                analyzeOptions: options);

            TestAnalyzeCommand.RaiseUnhandledExceptionInDriverCode = false;
        }

        [Fact]
        public void IOExceptionRaisedCreatingSarifLog()
        {
            string path = Path.GetTempFileName();

            try
            {
                using (var stream = File.OpenWrite(path))
                {
                    // our log file is locked for write
                    // causing exceptions at analysis time

                    var options = new TestAnalyzeOptions()
                    {
                        TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                        OutputFilePath = path,
                        Verbose = true,
                    };

                    ExceptionTestHelper(
                        ExceptionCondition.None,
                        RuntimeConditions.ExceptionCreatingLogfile,
                        expectedExitReason: ExitReason.ExceptionCreatingLogFile,
                        analyzeOptions: options);
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void UnauthorizedAccessExceptionCreatingSarifLog()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            path = Path.Combine(path, Guid.NewGuid().ToString());

            try
            {
                // attempt to persist to unauthorized location will raise exception
                var options = new TestAnalyzeOptions()
                {
                    TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                    OutputFilePath = path,
                    Verbose = true,
                };

                ExceptionTestHelper(
                    ExceptionCondition.None,
                    RuntimeConditions.ExceptionCreatingLogfile,
                    expectedExitReason: ExitReason.ExceptionCreatingLogFile,
                    analyzeOptions: options);
            }
            finally
            {
                File.Delete(path);
            }
        }


        public RunLog AnalyzeFile(string fileName)
        {
            string path = Path.GetTempFileName();
            RunLog runLog = null;

            try
            {
                var options = new TestAnalyzeOptions
                {
                    TargetFileSpecifiers = new string[] { fileName },
                    Verbose = true,
                    Statistics = true,
                    ComputeTargetsHash = true,
                    ConfigurationFilePath = "default",
                    Recurse = true,
                    OutputFilePath = path,
                };

                var command = new TestAnalyzeCommand();
                command.DefaultPlugInAssemblies = new Assembly[] { this.GetType().Assembly };
                int result = command.Run(options);

                Assert.Equal(TestAnalyzeCommand.SUCCESS, result);

                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    ContractResolver = SarifContractResolver.Instance
                };

                ResultLog log = JsonConvert.DeserializeObject<ResultLog>(File.ReadAllText(path), settings);
                Assert.NotNull(log);
                Assert.Equal<int>(1, log.RunLogs.Count);

                runLog = log.RunLogs[0];
            }
            finally
            {
                File.Delete(path);
            }

            return runLog;
        }

        [Fact]
        public void AnalyzeCommand_EndToEndAnalysisWithNoIssues()
        {
            RunLog runLog = AnalyzeFile(this.GetType().Assembly.Location);

            int issueCount = 0;
            SarifHelpers.ValidateRunLog(runLog, (issue) => { issueCount++; });
            Assert.Equal(1, issueCount);
        }
    }
}