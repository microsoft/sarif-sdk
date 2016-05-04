// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
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

            Assembly[] plugInAssemblies = null;

            if (analyzeOptions.DefaultPlugInFilePaths != null)
            {
                var assemblies = new List<Assembly>();
                foreach (string plugInFilePath in analyzeOptions.DefaultPlugInFilePaths)
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
        public void InvalidCommandLineOption()
        {
            var options = new TestAnalyzeOptions
            {
                RegardOptionsAsInvalid = true
            };

            ExceptionTestHelper(
                ExceptionCondition.ValidatingOptions,
                RuntimeConditions.InvalidCommandLineOption,
                ExitReason.InvalidCommandLineOption,
                options);
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
                analyzeOptions : options);
        }

        [Fact]
        public void NoRulesLoaded()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                DefaultPlugInFilePaths = new string[] { typeof(string).Assembly.Location },
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.NoRulesLoaded,
                ExitReason.NoRulesLoaded,
                analyzeOptions : options
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
        public void FileUri()
        {
            Uri uri = new Uri(this.GetType().Assembly.Location);

            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { uri.ToString() },
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.NoErrors,
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
                analyzeOptions : options);

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
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, Guid.NewGuid().ToString());

            using (var stream = File.Create(path, 1, FileOptions.DeleteOnClose))
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
        }

        [Fact]
        public void MissingConfigurationFile()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, Guid.NewGuid().ToString());

            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                ConfigurationFilePath = path,
                Verbose = true,
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.MissingFile,
                expectedExitReason: ExitReason.InvalidCommandLineOption,
                analyzeOptions: options);
        }

        [Fact]
        public void MissingPlugInFile()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, Guid.NewGuid().ToString());

            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                PlugInFilePaths = new string[] { path },
                Verbose = true,
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.MissingFile,
                expectedExitReason: ExitReason.InvalidCommandLineOption,
                analyzeOptions: options);
        }

        [Fact]
        public void MissingOutputFile()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, Guid.NewGuid().ToString());

            try {
                var options = new TestAnalyzeOptions()
                {
                    TargetFileSpecifiers = new string[] { this.GetType().Assembly.Location },
                    OutputFilePath = path,
                    Verbose = true,
                };

                // A missing output file is a good condition. :)
                ExceptionTestHelper(
                    ExceptionCondition.None,
                    RuntimeConditions.NoErrors,
                    expectedExitReason: ExitReason.None,
                    analyzeOptions: options);
            }
            finally
            {
                if (File.Exists(path)) { File.Delete(path); }
            }
        }

        public Run AnalyzeFile(string fileName)
        {
            string path = Path.GetTempFileName();
            Run run = null;

            try
            {
                var options = new TestAnalyzeOptions
                {
                    TargetFileSpecifiers = new string[] { fileName },
                    Verbose = true,
                    Statistics = true,
                    ComputeTargetsHash = true,
                    ConfigurationFilePath = TestAnalyzeCommand.DEFAULT_POLICY_NAME,
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

                SarifLog log = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(path), settings);
                Assert.NotNull(log);
                Assert.Equal<int>(1, log.Runs.Count);

                run = log.Runs.First();
            }
            finally
            {
                File.Delete(path);
            }

            return run;
        }

        [Fact]
        public void AnalyzeCommand_EndToEndAnalysisWithNoIssues()
        {
            Run run = AnalyzeFile(this.GetType().Assembly.Location);

            int resultCount = 0;
            SarifHelpers.ValidateRun(run, (issue) => { resultCount++; });
            Assert.Equal(1, resultCount);
        }
    }
}