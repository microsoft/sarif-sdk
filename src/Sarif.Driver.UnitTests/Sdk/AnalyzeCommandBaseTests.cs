// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
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

            analyzeOptions.Quiet = true;

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
                (runtimeConditions & ~RuntimeConditions.Nonfatal) == RuntimeConditions.None ?
                    TestAnalyzeCommand.SUCCESS : TestAnalyzeCommand.FAILURE;

            command.RuntimeErrors.Should().Be(runtimeConditions);
            result.Should().Be(expectedResult);

            if (expectedExitReason != ExitReason.None)
            {
                command.ExecutionException.Should().NotBeNull();

                if (expectedExitReason != ExitReason.UnhandledExceptionInEngine)
                {
                    var eax = command.ExecutionException as ExitApplicationException<ExitReason>;
                    eax.Should().NotBeNull();
                    eax.ExitReason.Should().Be(expectedExitReason);
                }
            }
            else
            {
                command.ExecutionException.Should().BeNull();
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
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
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
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                RegardAnalysisTargetAsValid = false
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.TargetNotValidToAnalyze,
                analyzeOptions: options);
        }

        [Fact]
        public void ExceptionLoadingTarget()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
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
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
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
            string assemblyWithNoPlugIns = typeof(TestAnalysisContext).Assembly.Location;

            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                DefaultPlugInFilePaths = new string[] { assemblyWithNoPlugIns },
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
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
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
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
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
                RuntimeConditions.None,
                analyzeOptions: options
            );
        }

        [Fact]
        public void ParseTargetException()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
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
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
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
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
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
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
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
                        TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                        OutputFilePath = path,
                        Verbose = true,
                    };

                    ExceptionTestHelper(
                        ExceptionCondition.None,
                        RuntimeConditions.ExceptionCreatingLogFile,
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
                    TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                    OutputFilePath = path,
                    Verbose = true,
                };

                ExceptionTestHelper(
                    ExceptionCondition.None,
                    RuntimeConditions.ExceptionCreatingLogFile,
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
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
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
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                PluginFilePaths = new string[] { path },
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

            try
            {
                var options = new TestAnalyzeOptions()
                {
                    TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                    OutputFilePath = path,
                    Verbose = true,
                };

                // A missing output file is a good condition. :)
                ExceptionTestHelper(
                    ExceptionCondition.None,
                    RuntimeConditions.None,
                    expectedExitReason: ExitReason.None,
                    analyzeOptions: options);

                if (File.Exists(path)) { File.Delete(path); }
            }
            finally
            {
            }
        }

        [Fact]
        public void AnalyzeCommand_ReportsErrorOnInvalidInvocationPropertyName()
        {
            var options = new TestAnalyzeOptions()
            {
                InvocationPropertiesToLog = new string[] { "CommandLine", "NoSuchProperty" }
            };

            ExceptionTestHelper(
                ExceptionCondition.None,
                RuntimeConditions.InvalidCommandLineOption,
                expectedExitReason: ExitReason.InvalidCommandLineOption,
                analyzeOptions: options);
        }

        [Fact]
        public void AnalyzeCommand_ReportsWarningOnUnsupportedPlatformForRule()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            // There are two default rules, so when this check is not on a supported platform, 
            // a single rule will still be loaded.
            ExceptionTestHelper(
                ExceptionCondition.InvalidPlatform,
                RuntimeConditions.RuleCannotRunOnPlatform,
                expectedExitReason: ExitReason.None,
                analyzeOptions: options);
        }


        [Fact]
        public void AnalyzeCommand_ReportsWarningOnUnsupportedPlatformForRuleAndNoRulesLoaded()
        {
            PropertiesDictionary allRulesDisabledConfiguration = ExportConfigurationCommandBaseTests.s_allRulesDisabledConfiguration;
            string path = Path.GetTempFileName() + ".xml";

            try
            {
                allRulesDisabledConfiguration.SaveToXml(path);
                var options = new TestAnalyzeOptions()
                {
                    TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                    ConfigurationFilePath = path
                };

                // There are two default rules.One of which is disabled by configuration,
                // the other is disabled as unsupported on current platform.
                ExceptionTestHelper(
                    ExceptionCondition.InvalidPlatform,
                    RuntimeConditions.NoRulesLoaded | RuntimeConditions.RuleWasExplicitlyDisabled | RuntimeConditions.RuleCannotRunOnPlatform,
                    expectedExitReason: ExitReason.NoRulesLoaded,
                    analyzeOptions: options);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        public Run AnalyzeFile(
            string fileName, 
            string configFileName = null,
            RuntimeConditions runtimeConditions = RuntimeConditions.None,
            int expectedReturnCode = TestAnalyzeCommand.SUCCESS)
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
                    Quiet = true,
                    ComputeFileHashes = true,
                    ConfigurationFilePath = configFileName ?? TestAnalyzeCommand.DefaultPolicyName,
                    Recurse = true,
                    OutputFilePath = path,
                    SarifOutputVersion = SarifVersion.Current
                };

                var command = new TestAnalyzeCommand();
                command.DefaultPlugInAssemblies = new Assembly[] { this.GetType().Assembly };
                int result = command.Run(options);

                result.Should().Be(expectedReturnCode);

                command.RuntimeErrors.Should().Be(runtimeConditions);

                SarifLog log = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(path));
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
        public void AnalyzeCommand_DefaultEndToEndAnalysis()
        {
            string location = GetThisTestAssemblyFilePath();
            Run run = AnalyzeFile(location);

            int resultCount = 0;
            int toolNotificationCount = 0;
            int configurationNotificationCount = 0;

            SarifHelpers.ValidateRun(
                run,
                (issue) => { resultCount++; },
                (toolNotification) => { toolNotificationCount++; },
                (configurationNotification) => { configurationNotificationCount++; });

            // By default, the exception raising rule produces a single error.
            // The simple test rule doesn't raise anything without add'l configuration
            resultCount.Should().Be(1);
            run.Results[0].Kind.Should().Equals(ResultKind.NotApplicable);

            toolNotificationCount.Should().Be(1);
            configurationNotificationCount.Should().Be(0);
        }


        [Fact]
        public void AnalyzeCommand_PersistsSarifOneZeroZero()
        {
            string fileName = GetThisTestAssemblyFilePath();
            string path = Path.GetTempFileName();

            try
            {
                var options = new TestAnalyzeOptions
                {
                    TargetFileSpecifiers = new string[] { fileName },
                    Verbose = true,
                    Statistics = true,
                    Quiet = true,
                    ComputeFileHashes = true,
                    ConfigurationFilePath = TestAnalyzeCommand.DefaultPolicyName,
                    Recurse = true,
                    OutputFilePath = path,
                    PrettyPrint = true,
                    SarifOutputVersion = SarifVersion.OneZeroZero
                };

                var command = new TestAnalyzeCommand();
                command.DefaultPlugInAssemblies = new Assembly[] { this.GetType().Assembly };
                int returnValue = command.Run(options);

                returnValue.Should().Be(0);

                command.RuntimeErrors.Should().Be(RuntimeConditions.None);

                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    ContractResolver = SarifContractResolverVersionOne.Instance
                };

                //TODO_FILES_ARRAY
                //SarifLogVersionOne log = JsonConvert.DeserializeObject<SarifLogVersionOne>(File.ReadAllText(path), settings);
                //log.Should().NotBeNull();
                //log.Runs.Count.Should().Be(1);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void AnalyzeCommand_FireDefaultRule()
        {
            string location = GetThisTestAssemblyFilePath();
            Run run = AnalyzeFile(location);

            int resultCount = 0;
            int toolNotificationCount = 0;
            int configurationNotificationCount = 0;

            SarifHelpers.ValidateRun(
                run,
                (issue) => { resultCount++; },
                (toolNotification) => { toolNotificationCount++; },
                (configurationNotification) => { configurationNotificationCount++; });

            // By default, the exception raising rule produces a single error.
            // The simple test rule doesn't raise anything without add'l configuration
            resultCount.Should().Be(1);
            run.Results[0].Level.Should().Be(FailureLevel.Warning);

            toolNotificationCount.Should().Be(1);
            configurationNotificationCount.Should().Be(0);
        }

        [Fact]
        public void AnalyzeCommand_FireAllRules()
        {
            PropertiesDictionary configuration = ExportConfigurationCommandBaseTests.s_defaultConfiguration;

            string path = Path.GetTempFileName() + ".xml";

            configuration.SetProperty(SimpleTestRule.Behaviors, TestRuleBehaviors.LogError);

            try
            {
                configuration.SaveToXml(path);

                string location = GetThisTestAssemblyFilePath();

                Run run = AnalyzeFile(location, configFileName: path);

                int resultCount = 0;
                int toolNotificationCount = 0;
                int configurationNotificationCount = 0;

                SarifHelpers.ValidateRun(
                    run,
                    (issue) => { resultCount++; },
                    (toolNotification) => { toolNotificationCount++; },
                    (configurationNotification) => { configurationNotificationCount++; });

                // By default, the exception raising rule produces a single error.
                // The simple test rule doesn't raise anything without add'l configuration
                resultCount.Should().Be(2);
                run.Results.Where((result) => result.Level == FailureLevel.Error).Count().Should().Be(1);
                run.Results.Where((result) => result.Level == FailureLevel.Warning).Count().Should().Be(1);
                run.Results.Where((result) => result.Kind == ResultKind.NotApplicable).Count().Should().Be(0);

                toolNotificationCount.Should().Be(1);
                configurationNotificationCount.Should().Be(0);
            }
            finally
            {
                if (File.Exists(path)) { File.Delete(path); }
            }
        }

        [Fact]
        public void AnalyzeCommand_EndToEndAnalysisWithExplicitlyDisabledRules()
        {
            PropertiesDictionary allRulesDisabledConfiguration = ExportConfigurationCommandBaseTests.s_allRulesDisabledConfiguration;
            string path = Path.GetTempFileName() + ".xml";

            try
            {
                allRulesDisabledConfiguration.SaveToXml(path);

                string location = GetThisTestAssemblyFilePath();

                Run run = AnalyzeFile(
                    location,
                    configFileName: path,
                    runtimeConditions: RuntimeConditions.RuleWasExplicitlyDisabled | RuntimeConditions.NoRulesLoaded,
                    expectedReturnCode: TestAnalyzeCommand.FAILURE);

                int resultCount = 0;
                int toolNotificationCount = 0;
                int configurationNotificationCount = 0;

                SarifHelpers.ValidateRun(
                    run,
                    (issue) => { resultCount++; },
                    (toolNotification) => { toolNotificationCount++; },
                    (configurationNotification) => { configurationNotificationCount++; });

                // When rules are disabled, we expect a configuration warning for each 
                // disabled check that documents it was turned off for the analysis.
                resultCount.Should().Be(0);

                // Three notifications. One for each disabled rule. And an error
                // notification that all rules have been disabled
                configurationNotificationCount.Should().Be(4);

                run.Invocations.Should().NotBeNull();
                run.Invocations.Count.Should().Be(1);

                run.Invocations[0].ToolConfigurationNotifications.Where((notification) => notification.Level == FailureLevel.Error).Count().Should().Be(1);
                run.Invocations[0].ToolConfigurationNotifications.Where((notification) => notification.Level == FailureLevel.Warning).Count().Should().Be(3);

                run.Invocations[0].ToolConfigurationNotifications.Where((notification) => notification.Id == Warnings.Wrn999_RuleExplicitlyDisabled).Count().Should().Be(3);

                toolNotificationCount.Should().Be(0);
            }
            finally
            {
                if (File.Exists(path)) { File.Delete(path); }
            }
        }

        [Theory]
        [InlineData(null, false, null)]
        [InlineData("", false, null)]
        [InlineData(null, true, "default.configuration.xml")]
        [InlineData("", true, "default.configuration.xml")]
        [InlineData("default", false, null)]
        [InlineData("default", true, null)]
        [InlineData("test-newconfig.xml", false, "test-newconfig.xml")]
        [InlineData("test-newconfig.xml", true, "test-newconfig.xml")]
        public void AnalyzeCommand_LoadConfigurationFile(string configValue, bool defaultFileExists, string expectedFileName)
        {
            var options = new TestAnalyzeOptions
            {
                TargetFileSpecifiers = new string[] { "" },
                Verbose = true,
                Statistics = true,
                Quiet = true,
                ComputeFileHashes = true,
                ConfigurationFilePath = configValue,
                Recurse = true,
                OutputFilePath = "",
            };

            var command = new TestAnalyzeCommand();
            
            string fileName = command.GetConfigurationFileName(options, defaultFileExists);
            if(string.IsNullOrEmpty(expectedFileName))
            {
                fileName.Should().BeNull();
            } else
            {
                fileName.Should().EndWith(expectedFileName);
            }
        }


        private static string GetThisTestAssemblyFilePath()
        {
            string filePath = typeof(AnalyzeCommandBaseTests).Assembly.Location;
            return filePath;
        }
    }
}