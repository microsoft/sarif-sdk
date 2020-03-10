// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class AnalyzeCommandBaseTests
    {
        private const int FAILURE = AnalyzeCommandBase<TestAnalysisContext, AnalyzeOptionsBase>.FAILURE;
        private const int SUCCESS = AnalyzeCommandBase<TestAnalysisContext, AnalyzeOptionsBase>.SUCCESS;

        private void ExceptionTestHelper(
            TestRuleBehaviors testRuleBehaviors,
            RuntimeConditions runtimeConditions,
            ExitReason expectedExitReason = ExitReason.None,
            TestAnalyzeOptions analyzeOptions = null)
        {
            TestRule.s_testRuleBehaviors = testRuleBehaviors;
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
                plugInAssemblies = new Assembly[] { typeof(TestRule).Assembly };
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
            TestRule.s_testRuleBehaviors = TestRuleBehaviors.None;
        }


        [Fact]
        public void InvalidCommandLineOption()
        {
            var options = new TestAnalyzeOptions
            {
                RegardOptionsAsInvalid = true
            };

            ExceptionTestHelper(
                TestRuleBehaviors.RaiseExceptionValidatingOptions,
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
                TestRuleBehaviors.None,
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
                TestRuleBehaviors.None,
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
                TestRuleBehaviors.None,
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
                TestRuleBehaviors.RaiseExceptionInvokingConstructor,
                RuntimeConditions.ExceptionInstantiatingSkimmers,
                ExitReason.UnhandledExceptionInstantiatingSkimmers,
                analyzeOptions: options);
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
                TestRuleBehaviors.None,
                RuntimeConditions.NoRulesLoaded,
                ExitReason.NoRulesLoaded,
                analyzeOptions: options
            );
        }

        [Fact]
        public void NoValidAnalysisTargets()
        {
            ExceptionTestHelper(
                TestRuleBehaviors.None,
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
                TestRuleBehaviors.RaiseExceptionInvokingInitialize,
                RuntimeConditions.ExceptionInSkimmerInitialize,
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
                TestRuleBehaviors.None,
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
                TestRuleBehaviors.RaiseTargetParseError,
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
                TestRuleBehaviors.RaiseExceptionInvokingCanAnalyze,
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
                TestRuleBehaviors.RaiseExceptionInvokingAnalyze,
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
                TestRuleBehaviors.None,
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
                using (FileStream stream = File.OpenWrite(path))
                {
                    // Our log file is locked for write
                    // causing exceptions at analysis time.

                    var options = new TestAnalyzeOptions()
                    {
                        TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                        OutputFilePath = path,
                        Verbose = true,
                        Force = true
                    };

                    ExceptionTestHelper(
                        TestRuleBehaviors.None,
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

            using (FileStream stream = File.Create(path, 1, FileOptions.DeleteOnClose))
            {
                // Attempt to persist to unauthorized location will raise exception.
                var options = new TestAnalyzeOptions()
                {
                    TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                    OutputFilePath = path,
                    Verbose = true,
                    Force = true
                };

                ExceptionTestHelper(
                    TestRuleBehaviors.None,
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
                TestRuleBehaviors.None,
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
                TestRuleBehaviors.None,
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
                    TestRuleBehaviors.None,
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
        public void AnalyzeCommandBase_ReportsErrorOnInvalidInvocationPropertyName()
        {
            var options = new TestAnalyzeOptions()
            {
                InvocationPropertiesToLog = new string[] { "CommandLine", "NoSuchProperty" }
            };

            ExceptionTestHelper(
                TestRuleBehaviors.None,
                RuntimeConditions.InvalidCommandLineOption,
                expectedExitReason: ExitReason.InvalidCommandLineOption,
                analyzeOptions: options);
        }

        [Fact]
        public void AnalyzeCommandBase_ReportsWarningOnUnsupportedPlatformForRule()
        {
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            // There are two default rules, so when this check is not on a supported platform, 
            // a single rule will still be loaded.
            ExceptionTestHelper(
                TestRuleBehaviors.TreatPlatformAsInvalid,
                RuntimeConditions.RuleCannotRunOnPlatform,
                expectedExitReason: ExitReason.None,
                analyzeOptions: options);
        }


        [Fact]
        public void AnalyzeCommandBase_ReportsWarningOnUnsupportedPlatformForRuleAndNoRulesLoaded()
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

                // There are two default rules. One of which is disabled by configuration,
                // the other is disabled as unsupported on current platform.
                ExceptionTestHelper(
                    TestRuleBehaviors.TreatPlatformAsInvalid,
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
                    SarifOutputVersion = SarifVersion.Current,
                    Force = true
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
        public void AnalyzeCommandBase_DefaultEndToEndAnalysis()
        {
            string location = GetThisTestAssemblyFilePath();

            Run run = null;
            try
            {
                TestRule.s_testRuleBehaviors = TestRuleBehaviors.LogError;
                run = AnalyzeFile(location);
            }
            finally
            {
                TestRule.s_testRuleBehaviors = TestRuleBehaviors.None;
            }

            int resultCount = 0;
            int toolNotificationCount = 0;
            int configurationNotificationCount = 0;

            SarifHelpers.ValidateRun(
                run,
                (issue) => { resultCount++; },
                (toolNotification) => { toolNotificationCount++; },
                (configurationNotification) => { configurationNotificationCount++; });

            // As configured by injected TestRuleBehaviors, we should
            // see an error per scan target (one file in this case).
            resultCount.Should().Be(1);
            run.Results[0].Kind.Should().Be(ResultKind.Fail);

            toolNotificationCount.Should().Be(0);
            configurationNotificationCount.Should().Be(0);
        }

        [Fact]
        public void AnalyzeCommandBase_PersistsSarifOneZeroZero()
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
                    Force = true,
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

                SarifLogVersionOne log = JsonConvert.DeserializeObject<SarifLogVersionOne>(File.ReadAllText(path), settings);
                log.Should().NotBeNull();
                log.Runs.Count.Should().Be(1);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void AnalyzeCommandBase_RunDefaultRules()
        {
            string location = GetThisTestAssemblyFilePath();

            Run run = null;
            try
            {
                TestRule.s_testRuleBehaviors = TestRuleBehaviors.LogError;
                run = AnalyzeFile(location);
            }
            finally
            {
                TestRule.s_testRuleBehaviors = TestRuleBehaviors.None;
            }

            int resultCount = 0;
            int toolNotificationCount = 0;
            int configurationNotificationCount = 0;

            SarifHelpers.ValidateRun(
                run,
                (issue) => { resultCount++; },
                (toolNotification) => { toolNotificationCount++; },
                (configurationNotification) => { configurationNotificationCount++; });

            // As configured by the inject TestRuleBehaviors value, we should see 
            // an error for every scan target (of which there is one file in this test).
            resultCount.Should().Be(1);
            run.Results[0].Level.Should().Be(FailureLevel.Error);

            toolNotificationCount.Should().Be(0);
            configurationNotificationCount.Should().Be(0);
        }

        [Fact]
        public void AnalyzeCommandBase_FireAllRules()
        {
            PropertiesDictionary configuration = ExportConfigurationCommandBaseTests.s_defaultConfiguration;

            string path = Path.GetTempFileName() + ".xml";

            configuration.SetProperty(TestRule.Behaviors, TestRuleBehaviors.LogError);

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

                // As configured by context, we should see a single error raised. 
                resultCount.Should().Be(1);
                run.Results.Where((result) => result.Level == FailureLevel.Error).Count().Should().Be(1);

                toolNotificationCount.Should().Be(0);
                configurationNotificationCount.Should().Be(0);
            }
            finally
            {
                if (File.Exists(path)) { File.Delete(path); }
            }
        }

        [Fact]
        public void AnalyzeCommandBase_EndToEndAnalysisWithExplicitlyDisabledRules()
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

                // Three notifications. One for each disabled rule, i.e. SimpleTestRule
                // and SimpleTestRule + an error notification that all rules have been disabled
                configurationNotificationCount.Should().Be(3);

                run.Invocations.Should().NotBeNull();
                run.Invocations.Count.Should().Be(1);

                // Error: all rules were disabled
                run.Invocations[0].ToolConfigurationNotifications.Where((notification) => notification.Level == FailureLevel.Error).Count().Should().Be(1);
                run.Invocations[0].ToolConfigurationNotifications.Where((notification) => notification.Descriptor.Id == Errors.ERR997_AllRulesExplicitlyDisabled).Count().Should().Be(1);

                // Warnings: one per disabled rule.
                run.Invocations[0].ToolConfigurationNotifications.Where((notification) => notification.Level == FailureLevel.Warning).Count().Should().Be(2);
                run.Invocations[0].ToolConfigurationNotifications.Where((notification) => notification.Descriptor.Id == Warnings.Wrn999_RuleExplicitlyDisabled).Count().Should().Be(2);

                // We raised a notification error, which means the invocation failed.
                run.Invocations[0].ExecutionSuccessful.Should().Be(false);

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
        public void AnalyzeCommandBase_LoadConfigurationFile(string configValue, bool defaultFileExists, string expectedFileName)
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
            if (string.IsNullOrEmpty(expectedFileName))
            {
                fileName.Should().BeNull();
            }
            else
            {
                fileName.Should().EndWith(expectedFileName);
            }
        }

        private static string GetThisTestAssemblyFilePath()
        {
            string filePath = typeof(AnalyzeCommandBaseTests).Assembly.Location;
            return filePath;
        }

        [Fact]
        public void AnalyzeCommandBase_UpdateLocationsAndMessageWithCurrentUri()
        {
            Uri uri = new Uri(@"c:\directory\test.txt", UriKind.RelativeOrAbsolute);
            Notification actualNotification = BuildTestNotification(uri);

            Uri updatedUri = new Uri(@"c:\updated\directory\newFileName.txt", UriKind.RelativeOrAbsolute);
            Notification expectedNotification = BuildTestNotification(updatedUri);

            AnalyzeCommandBase<TestAnalysisContext, AnalyzeOptionsBase>
                .UpdateLocationsAndMessageWithCurrentUri(actualNotification.Locations, actualNotification.Message, updatedUri);

            actualNotification.Should().BeEquivalentTo(expectedNotification);
        }

        private static Notification BuildTestNotification(Uri uri)
        {
            string filePath = uri.OriginalString;
            string fileName = Path.GetFileName(uri.OriginalString);
            return new Notification
            {
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = uri
                            }
                        }
                    }
                },
                Message = new Message
                {
                    Arguments = new List<string> { filePath, fileName },
                    Text = string.Format(@"Found an issue in {0} (full path is {1}", filePath, fileName)
                }
            };
        }

        [Fact]
        public void AnalyzeCommandBase_GetFileNameFromUriWorks()
        {
            var sb = new StringBuilder();

            var testCases = new Tuple<string, string>[]
            {
                new Tuple<string, string>(null, null),
                new Tuple<string, string>(@"file.txt", "file.txt"),
                new Tuple<string, string>(@".\file.txt", "file.txt"),
                new Tuple<string, string>(@"c:\directory\file.txt", "file.txt"),
                new Tuple<string, string>(@"\\computer\computer\file.txt", "file.txt"),
                new Tuple<string, string>(@"file://directory/file.txt", "file.txt"),
                new Tuple<string, string>(@"/file.txt", "file.txt"),
                new Tuple<string, string>(@"directory/file.txt", "file.txt"),
            };

            foreach (Tuple<string, string> testCase in testCases)
            {
                Uri uri = testCase.Item1 != null ? new Uri(testCase.Item1, UriKind.RelativeOrAbsolute) : null;
                string expectedFileName = testCase.Item2;

                string actualFileName = AnalyzeCommandBase<TestAnalysisContext, AnalyzeOptionsBase>.GetFileNameFromUri(uri);

                if (!object.Equals(actualFileName, expectedFileName))
                {
                    sb.AppendLine(string.Format("Incorrect file name returned for uri '{0}'. Expected '{1}' but saw '{2}'.", uri, expectedFileName, actualFileName));
                }
            }
            sb.Length.Should().Be(0, because: "all URI to file name conversions should succeed but the following cases failed." + Environment.NewLine + sb.ToString());
        }

        #region ResultsCachingTestsAndHelpers

        [Fact]
        public void AnalyzeCommandBase_CachesErrors()
        {
            // Produce two errors results
            var testCase = new ResultsCachingTestCase()
            {
                Files = new List<string> { "Error.dll", "Error.exe" }
            };

            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase);
        }


        [Fact]
        public void AnalyzeCommandBase_CachesNotes()
        {
            // Produce three results in verbose runs only
            var testCase = new ResultsCachingTestCase()
            {
                Files = new List<string> { "Note.dll", "Note.exe", "Note.sys" }
            };

            // Notes are verbose only results
            testCase.ExpectedResultsCount.Should().Be(0);
            testCase.ExpectedNoteCount.Should().Be(0);

            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;

            testCase.ExpectedResultsCount.Should().Be(testCase.Files.Count);
            testCase.ExpectedNoteCount.Should().Be(testCase.Files.Count);

            RunResultsCachingTestCase(testCase);
        }

        [Fact]
        public void AnalyzeCommandBase_CachesNotificationsWithoutPersistingToLogFile()
        {
            var testCase = new ResultsCachingTestCase
            {
                Files = ComprehensiveKindAndLevelsByFileName,
                TestRuleBehaviors = TestRuleBehaviors.RaiseTargetParseError,
                ExpectedReturnCode = FAILURE,
            };

            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase);
        }

        [Fact]
        public void AnalyzeCommandBase_CachesNotificationsWhenPersistingToLogFile()
        {
            var testCase = new ResultsCachingTestCase
            {
                Files = ComprehensiveKindAndLevelsByFileName,
                PersistLogFileToDisk = true,
                TestRuleBehaviors = TestRuleBehaviors.RaiseTargetParseError,
                ExpectedReturnCode = FAILURE,
            };

            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase);
        }

        [Fact]
        public void AnalyzeCommandBase_CachesResultsWithoutPersistingToLogFile()
        {
            var testCase = new ResultsCachingTestCase
            {
                Files = ComprehensiveKindAndLevelsByFileName,
                PersistLogFileToDisk = false,
            };

            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase);
        }

        [Fact]
        public void AnalyzeCommandBase_CachesResultsWhenPersistingToLogFile()
        {
            var testCase = new ResultsCachingTestCase
            {
                Files = ComprehensiveKindAndLevelsByFileName,
                PersistLogFileToDisk = true,
            };

            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase);
        }

        private static readonly IList<string> ComprehensiveKindAndLevelsByFileName = new List<string>(new string[20]
            {
                // Every one of these files will be regarded as identical in content by level/kind. So every file
                // with 'Error' as a prefix should produce an error result, whether using results caching or not.
                // We distinguish file names as this is required in the actual scenario, i.e., when 'replaying'
                // cached results we must retain the unique fully qualified directory + file name for each copy.
                // In actual production systems, this differentiation mostly occurs by directory name (i.e., copies
                // of files tend to have the same name but appear in different directories). For a source code scanner, 
                // however, two files in the same directory may hash the same (an empty file that produces no scan
                // results is an obvious case).
                "Error.1.of.5.cpp",
                "Error.2.of.5.cs",
                "Error.3.of.5.exe",
                "Error.4.of.5.h",
                "Error.5.of.5.sys",
                "Warning.1.of.2.java",
                "Warning.2.of.2.cs",
                "Note.1.of.3.dll",
                "Note.2.of.3.exe",
                "Note.3.of.3jar",
                "Pass.1.of.4.cs",
                "Pass.2.of.4.cpp",
                "Pass.3.of.4.exe",
                "Pass.4.of.4.dll",
                "NotApplicable.1.of.2.js",
                "NotApplicable.2.of.2.exe",
                "Informational.1.of.1.sys",
                "Open.1.of.1.cab",
                "Review.1.of.2.txt",
                "Review.2.of.2.dll"
            });

        private static void RunResultsCachingTestCase(ResultsCachingTestCase testCase)
        {
            // This makes sure that we will reinitialize the mock file system. This
            // allows callers to reuse test case instances, by adjusting specific 
            // property values. The mock file system cannot be reused in this way, once
            // a test executes, it must be reset.
            testCase.FileSystem = null;

            var options = new TestAnalyzeOptions
            {
                OutputFilePath = testCase.PersistLogFileToDisk ? Guid.NewGuid().ToString() : null,
                TargetFileSpecifiers = new string[] { Guid.NewGuid().ToString() },
                Verbose = testCase.Verbose,
                ComputeFileHashes = false,
            };

            int expectedResultsCount = testCase.ExpectedWarningCount + testCase.ExpectedErrorCount;
            Run runWithoutCaching = RunAnalyzeCommand(options, testCase);

            options.ComputeFileHashes = true;
            Run runWithCaching = RunAnalyzeCommand(options, testCase);

            // Core static analysis results
            runWithCaching.Results.Should().BeEquivalentTo(runWithoutCaching.Results);

            // Tool configuration errors, such as 'Could not locate scan target PDB.'
            runWithoutCaching.Invocations?[0].ToolConfigurationNotifications?.Should()
                .BeEquivalentTo(runWithCaching.Invocations?[0].ToolConfigurationNotifications);

            // Not yet explicitly tested
            runWithoutCaching.Invocations?[0].ToolExecutionNotifications?.Should()
                .BeEquivalentTo(runWithCaching.Invocations?[0].ToolExecutionNotifications);
        }

        private static IFileSystem CreateDefaultFileSystemForResultsCaching(IList<string> files)
        {
            // This helper creates a file system that returns the same file contents for
            // every file passed in the 'files' argument.

            string logFileContents = Guid.NewGuid().ToString();

            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(x => x.GetDirectoriesInDirectory(It.IsAny<string>())).Returns(new string[0]);
            mockFileSystem.Setup(x => x.GetFilesInDirectory(It.IsAny<string>(), It.IsAny<string>())).Returns(files);

            for (int i = 0; i < files.Count; i++)
            {
                string fullyQualifiedName = Environment.CurrentDirectory + @"\" + files[i];
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullyQualifiedName);
                mockFileSystem.Setup(x => x.ReadAllText(It.Is<string>(f => f == fullyQualifiedName))).Returns(logFileContents);

                mockFileSystem.Setup(x => x.OpenRead(It.Is<string>(f => f == fullyQualifiedName)))
                    .Returns(new MemoryStream(Encoding.UTF8.GetBytes(fileNameWithoutExtension)));
            }
            return mockFileSystem.Object;
        }

        private static Run RunAnalyzeCommand(TestAnalyzeOptions options, ResultsCachingTestCase testCase)
        {
            Run run = null;
            SarifLog sarifLog;
            try
            {
                TestRule.s_testRuleBehaviors = testCase.TestRuleBehaviors;
                sarifLog = RunAnalyzeCommand(options, testCase.FileSystem, testCase.ExpectedReturnCode);
                run = sarifLog.Runs[0];

                run.Results.Count.Should().Be(testCase.ExpectedResultsCount);
            }
            finally
            {
                TestRule.s_testRuleBehaviors = TestRuleBehaviors.None;
            }
            return run;
        }

        private static SarifLog RunAnalyzeCommand(TestAnalyzeOptions options, IFileSystem fileSystem, int expectedReturnCode)
        {
            // If no log file is specified, we will convert the console output into a log file
            bool captureConsoleOutput = string.IsNullOrEmpty(options.OutputFilePath);

            var command = new TestAnalyzeCommand(fileSystem) { _captureConsoleOutput = captureConsoleOutput };
            command.DefaultPlugInAssemblies = new Assembly[] { typeof(AnalyzeCommandBaseTests).Assembly };

            try
            {
                HashUtilities.FileSystem = fileSystem;
                command.Run(options).Should().Be(expectedReturnCode);
            }
            finally
            {
                HashUtilities.FileSystem = null;
            }

            return captureConsoleOutput
                ? ConvertConsoleOutputToSarifLog(command._consoleLogger.CapturedOutput)
                : JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(options.OutputFilePath));
        }

        private static SarifLog ConvertConsoleOutputToSarifLog(string consoleOutput)
        {
            var sb = new StringBuilder();
            var converter = new MSBuildConverter(verbose: true);

            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(consoleOutput)))
            using (var outputTextWriter = new StringWriter(sb))
            using (var outputJson = new JsonTextWriter(outputTextWriter))
            using (var output = new ResultLogJsonWriter(outputJson))
            {
                converter.Convert(input, output, OptionallyEmittedData.None);
            }
            return JsonConvert.DeserializeObject<SarifLog>(sb.ToString());
        }

        private class ResultsCachingTestCase
        {
            private IFileSystem _fileSystem;

            public ResultsCachingTestCase()
            {
                ExpectedReturnCode = SUCCESS;
            }

            public bool Verbose;

            public IList<string> Files;

            public IFileSystem FileSystem
            {
                get
                {
                    if (_fileSystem == null)
                    {
                        _fileSystem = CreateDefaultFileSystemForResultsCaching(Files);
                    }
                    return _fileSystem;
                }

                set => _fileSystem = value;
            }

            public int ExpectedReturnCode;

            public int ExpectedResultsCount =>
                // Non-verbose results
                (ExpectedErrorCount + ExpectedWarningCount) +
                // Verbose results
                (Verbose
                    ? ExpectedNoteCount + ExpectedPassCount + ExpectedInformationalCount +
                      ExpectedOpenCount + ExpectedReviewCount + ExpectedNotApplicableCount
                    : 0);

            public int ExpectedErrorCount =>
                Files.Count((f) => f.Contains("Error")) +
                // For our special case, all files except for those that are marked as 'not applicable' will
                // produce a 'pdb load' notification that will be converted to an error. The not applicable
                // cases will not do this, because the return of 'not applicable' from the CanAnalyze
                // method will result in Analyze not getting called subsequently for those scan targets.
                (NotificationsWillBeConvertedToErrorResults
                    ? Files.Count() - Files.Where((f) => f.Contains("NotApplicable")).Count()
                    : 0);

            public int ExpectedWarningCount =>
                Files.Where((f) => f.Contains("Warning")).Count();

            public int ExpectedNoteCount => Verbose
                ? Files.Where((f) => f.Contains("Note")).Count()
                : 0;

            public int ExpectedPassCount => Verbose
                ? Files.Where((f) => f.Contains("Pass")).Count()
                : 0;

            public int ExpectedInformationalCount => Verbose
                ? Files.Where((f) => f.Contains("Informational")).Count()
                : 0;

            public int ExpectedReviewCount => Verbose
                ? Files.Where((f) => f.Contains("Review")).Count()
                : 0;

            public int ExpectedOpenCount => Verbose
                ? Files.Where((f) => f.Contains("Open")).Count()
                : 0;

            public int ExpectedNotApplicableCount => Verbose
                ? Files.Where((f) => f.Contains("NotApplicable")).Count()
                : 0;

            public bool PersistLogFileToDisk;

            public TestRuleBehaviors TestRuleBehaviors;

            // This is a special knob that that accounts for a current SDK behavior.
            // Specifically, the MSBuildConverter currently transforms all notifications
            // to results. This will require us to recompute expected notification vs.
            // results counts depending on whether we are examining the console output
            // or an actual persisted log file to validate outcomes.
            bool NotificationsWillBeConvertedToErrorResults => TestRuleBehaviors == TestRuleBehaviors.RaiseTargetParseError && !PersistLogFileToDisk;
        }
        #endregion ResultsCachingTestsAndHelpers
    }
}