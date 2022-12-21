// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

using Microsoft.Coyote;
using Microsoft.Coyote.Specifications;
using Microsoft.Coyote.SystematicTesting;

using Moq;

using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class AnalyzeCommandBaseTests
    {
        private const int FAILURE = AnalyzeCommandBase<TestAnalysisContext, AnalyzeOptionsBase>.FAILURE;
        private const int SUCCESS = AnalyzeCommandBase<TestAnalysisContext, AnalyzeOptionsBase>.SUCCESS;

        private readonly ITestOutputHelper Output;

        public AnalyzeCommandBaseTests(ITestOutputHelper output)
        {
            this.Output = output;
        }

        [Fact]
        public void AnalyzeCommandBase_RootContextIsDisposed()
        {
            var options = new TestAnalyzeOptions();
            var singleThreadedCommand = new TestAnalyzeCommand();
            int result = singleThreadedCommand.Run(options);
            singleThreadedCommand._rootContext.Disposed.Should().BeTrue();

            var multithreadedAnalyzeCommand = new TestMultithreadedAnalyzeCommand();
            result = singleThreadedCommand.Run(options);
            singleThreadedCommand._rootContext.Disposed.Should().BeTrue();
        }

        private void ExceptionTestHelper(
            RuntimeConditions runtimeConditions,
            ExitReason expectedExitReason = ExitReason.None,
            TestAnalyzeOptions analyzeOptions = null)
        {
            analyzeOptions ??= new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = Array.Empty<string>()
            };

            ExceptionTestHelperImplementation(
                runtimeConditions, expectedExitReason,
                analyzeOptions,
                multithreaded: false);

            ExceptionTestHelperImplementation(
                runtimeConditions, expectedExitReason,
                analyzeOptions,
                multithreaded: true);
        }

        private void ExceptionTestHelperImplementation(
             RuntimeConditions runtimeConditions,
             ExitReason expectedExitReason,
             TestAnalyzeOptions analyzeOptions,
             bool multithreaded)
        {
            TestRule.s_testRuleBehaviors = analyzeOptions.TestRuleBehaviors.AccessibleOutsideOfContextOnly();
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

            ITestAnalyzeCommand command = multithreaded
                ? new TestMultithreadedAnalyzeCommand()
                : (ITestAnalyzeCommand)new TestAnalyzeCommand();

            command.DefaultPluginAssemblies = plugInAssemblies;

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
                TestRuleBehaviors =
                    TestRuleBehaviors.RaiseExceptionValidatingOptions |
                    TestRuleBehaviors.RegardOptionsAsInvalid
            };

            ExceptionTestHelper(
                RuntimeConditions.InvalidCommandLineOption,
                ExitReason.InvalidCommandLineOption,
                options);
        }

        [Fact]
        public void NotApplicableToTarget()
        {
            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.RegardAnalysisTargetAsNotApplicable,
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() }
            };

            ExceptionTestHelper(
                RuntimeConditions.RuleNotApplicableToTarget,
                analyzeOptions: options);
        }

        [Fact]
        public void InvalidTarget()
        {
            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.RegardAnalysisTargetAsInvalid,
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            ExceptionTestHelper(
                RuntimeConditions.TargetNotValidToAnalyze,
                analyzeOptions: options);
        }

        [Fact]
        public void InvalidTargetFilePath()
        {
            string validPath = GetThisTestAssemblyFilePath();
            string[] invalidPaths = validPath.Split('.');

            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.RegardAnalysisTargetAsInvalid,
                TargetFileSpecifiers = invalidPaths,
            };

            ExceptionTestHelper(
                RuntimeConditions.NoValidAnalysisTargets,
                analyzeOptions: options,
                expectedExitReason: ExitReason.NoValidAnalysisTargets);
        }

        [Fact]
        public void ExceptionLoadingTarget()
        {
            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.RegardAnalysisTargetAsCorrupted,
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() }
            };

            ExceptionTestHelper(
                RuntimeConditions.ExceptionLoadingTargetFile,
                analyzeOptions: options);
        }

        [Fact]
        public void ExceptionRaisedInstantiatingSkimmers()
        {
            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.RaiseExceptionInvokingConstructor,
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            ExceptionTestHelper(
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
                RuntimeConditions.NoRulesLoaded,
                ExitReason.NoRulesLoaded,
                analyzeOptions: options
            );
        }

        [Fact]
        public void NoValidAnalysisTargets()
        {
            ExceptionTestHelper(
                RuntimeConditions.NoValidAnalysisTargets,
                ExitReason.NoValidAnalysisTargets
            );
        }

        [Fact]
        public void ExceptionRaisedInvokingInitialize()
        {
            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.RaiseExceptionInvokingInitialize,
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            ExceptionTestHelper(
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
                RuntimeConditions.None,
                analyzeOptions: options
            );
        }

        [Fact]
        public void ParseTargetException()
        {
            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.RaiseTargetParseError,
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            ExceptionTestHelper(
                RuntimeConditions.TargetParseError,
                analyzeOptions: options
            );
        }

        [Fact]
        public void ExceptionRaisedInvokingCanAnalyze()
        {
            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.RaiseExceptionInvokingCanAnalyze,
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            ExceptionTestHelper(
                RuntimeConditions.ExceptionRaisedInSkimmerCanAnalyze,
                analyzeOptions: options
            );
        }

        [Fact]
        public void ExceptionRaisedInvokingAnalyze()
        {
            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.RaiseExceptionInvokingAnalyze,
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            ExceptionTestHelper(
                RuntimeConditions.ExceptionInSkimmerAnalyze,
                analyzeOptions: options
            );
        }

        [Fact]
        public void ExceptionRaisedProcessingBaseline()
        {
            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.RaiseExceptionProcessingBaseline,
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            ExceptionTestHelper(
                RuntimeConditions.ExceptionProcessingBaseline,
                expectedExitReason: ExitReason.ExceptionProcessingBaseline,
                analyzeOptions: options
            );
        }

        [Fact]
        public void ExceptionRaisedInvokingAnalyze_PersistInnerException()
        {
            string location = GetThisTestAssemblyFilePath();

            Run run = AnalyzeFile(location,
                                  TestRuleBehaviors.RaiseExceptionInvokingAnalyze,
                                  runtimeConditions: RuntimeConditions.ExceptionInSkimmerAnalyze,
                                  expectedReturnCode: 1);

            run.Invocations[0]?.ToolExecutionNotifications.Count.Should().Be(1);
            Stack stack = run.Invocations[0]?.ToolExecutionNotifications[0].Exception.Stack;
            string fqn = stack.Frames[0].Location.LogicalLocation.FullyQualifiedName;
            fqn.Contains(nameof(TestRule.RaiseExceptionViaReflection)).Should().BeTrue();
        }

        [Fact]
        public void ExceptionRaisedInEngine()
        {
            TestAnalyzeCommand.RaiseUnhandledExceptionInDriverCode = true;
            TestMultithreadedAnalyzeCommand.RaiseUnhandledExceptionInDriverCode = true;

            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            ExceptionTestHelper(
                RuntimeConditions.ExceptionInEngine,
                ExitReason.UnhandledExceptionInEngine,
                analyzeOptions: options);

            TestAnalyzeCommand.RaiseUnhandledExceptionInDriverCode = false;
            TestMultithreadedAnalyzeCommand.RaiseUnhandledExceptionInDriverCode = false;
        }

        [Fact]
        public void IOExceptionRaisedCreatingSarifLog()
        {
            string path = Path.GetTempFileName();

            try
            {
                using (_ = File.OpenWrite(path))
                {
                    // Our log file is locked for write
                    // causing exceptions at analysis time.

                    var options = new TestAnalyzeOptions()
                    {
                        TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                        OutputFilePath = path,
                        Force = true
                    };

                    ExceptionTestHelper(
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

            using (_ = File.Create(path, 1, FileOptions.DeleteOnClose))
            {
                // Attempt to persist to unauthorized location will raise exception.
                var options = new TestAnalyzeOptions()
                {
                    TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                    OutputFilePath = path,
                    Force = true
                };

                ExceptionTestHelper(
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
                ConfigurationFilePath = path
            };

            ExceptionTestHelper(
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
                PluginFilePaths = new string[] { path }
            };

            ExceptionTestHelper(
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
                    OutputFilePath = path
                };

                // A missing output file is a good condition. :)
                ExceptionTestHelperImplementation(
                    RuntimeConditions.None,
                    expectedExitReason: ExitReason.None,
                    analyzeOptions: options,
                    multithreaded: false);

                if (File.Exists(path)) { File.Delete(path); }

                ExceptionTestHelperImplementation(
                    RuntimeConditions.None,
                    expectedExitReason: ExitReason.None,
                    analyzeOptions: options,
                    multithreaded: true);
            }
            finally
            {
                if (File.Exists(path)) { File.Delete(path); }
            }
        }

        [Fact]
        public void MissingBaselineFile()
        {
            string outputFilePath = Path.GetTempFileName() + ".sarif";
            string baselineFilePath = Path.GetTempFileName() + ".sarif";

            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                OutputFilePath = outputFilePath,
                BaselineSarifFile = baselineFilePath
            };

            ExceptionTestHelper(
                RuntimeConditions.MissingFile,
                expectedExitReason: ExitReason.InvalidCommandLineOption,
                analyzeOptions: options);
        }

        [Fact]
        public void BaselineWithoutOutputFile()
        {
            string path = Path.GetTempFileName() + ".sarif";

            using (_ = File.Create(path, 1, FileOptions.DeleteOnClose))
            {
                var options = new TestAnalyzeOptions()
                {
                    TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                    Quiet = true,
                    OutputFilePath = null,
                    BaselineSarifFile = path
                };

                ExceptionTestHelper(
                    RuntimeConditions.InvalidCommandLineOption,
                    expectedExitReason: ExitReason.InvalidCommandLineOption,
                    analyzeOptions: options);
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
                RuntimeConditions.InvalidCommandLineOption,
                expectedExitReason: ExitReason.InvalidCommandLineOption,
                analyzeOptions: options);
        }

        [Fact]
        public void AnalyzeCommandBase_ReportsWarningOnUnsupportedPlatformForRule()
        {
            var options = new TestAnalyzeOptions()
            {
                TestRuleBehaviors = TestRuleBehaviors.TreatPlatformAsInvalid,
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            // There are two default rules, so when this check is not on a supported platform,
            // a single rule will still be loaded.
            ExceptionTestHelper(
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
                    // This option needs to be specified here as the file-based
                    // configuration has not yet been read when skimmers are loaded.
                    // This means we can't use that data to inject a skimmer
                    // behavior to assert that it doesn't work against the current
                    // platform.
                    TestRuleBehaviors = TestRuleBehaviors.TreatPlatformAsInvalid,
                    TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
                    ConfigurationFilePath = path
                };

                // There are two default rules. One of which is disabled by configuration,
                // the other is disabled as unsupported on current platform.
                ExceptionTestHelper(
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
            TestRuleBehaviors behaviors = TestRuleBehaviors.None,
            string configFileName = null,
            RuntimeConditions runtimeConditions = RuntimeConditions.None,
            int expectedReturnCode = TestAnalyzeCommand.SUCCESS,
            string postUri = null)
        {
            string path = Path.GetTempFileName();
            Run run = null;

            try
            {
                var options = new TestAnalyzeOptions
                {
                    TargetFileSpecifiers = new string[] { fileName },
                    Quiet = true,
                    ConfigurationFilePath = configFileName ?? TestAnalyzeCommand.DefaultPolicyName,
                    Recurse = true,
                    OutputFilePath = path,
                    SarifOutputVersion = SarifVersion.Current,
                    Force = true,
                    TestRuleBehaviors = behaviors,
                    PostUri = postUri,
                };

                var command = new TestAnalyzeCommand();
                command.DefaultPluginAssemblies = new Assembly[] { this.GetType().Assembly };
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

            Run run = AnalyzeFile(location, TestRuleBehaviors.LogError);

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
            resultCount.Should().Be((int)TestRule.ErrorsCount.DefaultValue());
            run.Results[0].Kind.Should().Be(ResultKind.Fail);

            toolNotificationCount.Should().Be(0);
            configurationNotificationCount.Should().Be(0);
        }

        [Fact]
        public void AnalyzeCommandBase_EndToEndAnalysisWithPostUri()
        {
            PostUriTestHelper(@"https://httpbin.org/post", TestAnalyzeCommand.SUCCESS, RuntimeConditions.None);
            PostUriTestHelper(@"https://httpbin.org/get", TestAnalyzeCommand.FAILURE, RuntimeConditions.ExceptionPostingLogFile);
            PostUriTestHelper(@"https://host.does.not.exist", TestAnalyzeCommand.FAILURE, RuntimeConditions.ExceptionPostingLogFile);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_EndToEndMultithreadedAnalysis()
        {
            string specifier = "*.xyz";

            int filesCount = 10;
            var files = new List<string>();
            for (int i = 0; i < filesCount; i++)
            {
                files.Add(Path.GetFullPath($@".{Path.DirectorySeparatorChar}File{i}.txt"));
            }

            var propertiesDictionary = new PropertiesDictionary();
            propertiesDictionary.SetProperty(TestRule.ErrorsCount, (uint)15);
            propertiesDictionary.SetProperty(TestRule.Behaviors, TestRuleBehaviors.LogError);

            using var tempFile = new TempFile(".xml");
            propertiesDictionary.SaveToXml(tempFile.Name);

            var mockStream = new Mock<Stream>();
            mockStream.Setup(m => m.CanRead).Returns(true);
            mockStream.Setup(m => m.CanSeek).Returns(true);
            mockStream.Setup(m => m.ReadByte()).Returns('a');

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryGetFiles(It.IsAny<string>(), specifier)).Returns(files);
            mockFileSystem.Setup(x => x.FileExists(It.Is<string>(s => s.EndsWith(specifier)))).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(),
                                                                It.IsAny<string>(),
                                                                It.IsAny<SearchOption>())).Returns(files);
            mockFileSystem.Setup(x => x.FileOpenRead(It.IsAny<string>())).Returns(mockStream.Object);
            mockFileSystem.Setup(x => x.FileExists(tempFile.Name)).Returns(true);

            Output.WriteLine($"The seed that will be used is: {TestRule.s_seed}");

            for (int i = 0; i < 50; i++)
            {
                var options = new TestAnalyzeOptions
                {
                    Threads = 10,
                    TargetFileSpecifiers = new[] { specifier },
                    SarifOutputVersion = SarifVersion.Current,
                    TestRuleBehaviors = TestRuleBehaviors.LogError,
                    DataToInsert = new[] { OptionallyEmittedData.Hashes },
                    ConfigurationFilePath = tempFile.Name
                };

                var command = new TestMultithreadedAnalyzeCommand(mockFileSystem.Object);
                command.DefaultPluginAssemblies = new Assembly[] { this.GetType().Assembly };

                int result = command.Run(options);

                command.ExecutionException?.InnerException.Should().BeNull();

                result.Should().Be(CommandBase.SUCCESS, $"Iteration: {i}, Seed: {TestRule.s_seed}");
            }
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_TargetFileSizeTestCases()
        {
            dynamic[] testCases = new[]
            {
                new {
                    expectedExitReason = ExitReason.InvalidCommandLineOption,
                    fileSize = (long)ulong.MinValue,
                    maxFileSize = int.MinValue
                },
                new {
                    expectedExitReason = ExitReason.InvalidCommandLineOption,
                    fileSize = (long)ulong.MinValue,
                    maxFileSize = -1
                },
                new {
                    expectedExitReason = ExitReason.InvalidCommandLineOption,
                    fileSize = (long)ulong.MinValue,
                    maxFileSize = 0
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSize = (long)ulong.MinValue,
                    maxFileSize = 1
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSize = (long)ulong.MinValue,
                    maxFileSize = 2000
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSize = (long)ulong.MinValue,
                    maxFileSize = 1000
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSize = (long)ulong.MinValue,
                    maxFileSize = int.MaxValue
                },
                new {
                    expectedExitReason = ExitReason.NoValidAnalysisTargets,
                    fileSize = (long)20000,
                    maxFileSize = 1
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSize = (long)20000,
                    maxFileSize = int.MaxValue
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSize = (long)10,
                    maxFileSize = 10
                },
                new {
                    expectedExitReason = ExitReason.InvalidCommandLineOption,
                    fileSize = long.MaxValue,
                    maxFileSize = int.MinValue
                },
                new {
                    expectedExitReason = ExitReason.InvalidCommandLineOption,
                    fileSize = long.MaxValue,
                    maxFileSize = 0
                },
                new {
                    expectedExitReason = ExitReason.NoValidAnalysisTargets,
                    fileSize = long.MaxValue,
                    maxFileSize = int.MaxValue
                },
            };

            foreach (dynamic testCase in testCases)
            {
                string specifier = "*.xyz";

                int filesCount = 10;
                var files = new List<string>();
                for (int i = 0; i < filesCount; i++)
                {
                    files.Add(Path.GetFullPath($@".{Path.DirectorySeparatorChar}File{i}.txt"));
                }

                var propertiesDictionary = new PropertiesDictionary();
                propertiesDictionary.SetProperty(TestRule.ErrorsCount, (uint)15);
                propertiesDictionary.SetProperty(TestRule.Behaviors, TestRuleBehaviors.LogError);

                using var tempFile = new TempFile(".xml");
                propertiesDictionary.SaveToXml(tempFile.Name);

                var mockStream = new Mock<Stream>();
                mockStream.Setup(m => m.CanRead).Returns(true);
                mockStream.Setup(m => m.CanSeek).Returns(true);
                mockStream.Setup(m => m.ReadByte()).Returns('a');

                var mockFileSystem = new Mock<IFileSystem>();
                mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
                mockFileSystem.Setup(x => x.DirectoryGetFiles(It.IsAny<string>(), specifier)).Returns(files);
                mockFileSystem.Setup(x => x.FileExists(It.Is<string>(s => s.EndsWith(specifier)))).Returns(true);
                mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(),
                                                                    It.IsAny<string>(),
                                                                    It.IsAny<SearchOption>())).Returns(files);
                mockFileSystem.Setup(x => x.FileOpenRead(It.IsAny<string>())).Returns(mockStream.Object);
                mockFileSystem.Setup(x => x.FileExists(tempFile.Name)).Returns(true);
                mockFileSystem.Setup(x => x.FileInfoLength(It.IsAny<string>())).Returns(testCase.fileSize);

                bool expectedToBeWithinLimits = testCase.maxFileSize == -1 ||
                    testCase.fileSize / 1024 < testCase.maxFileSize;

                Output.WriteLine($"The seed that will be used is: {TestRule.s_seed}");

                var options = new TestAnalyzeOptions
                {
                    TargetFileSpecifiers = new[] { specifier },
                    SarifOutputVersion = SarifVersion.Current,
                    TestRuleBehaviors = TestRuleBehaviors.LogError,
                    ConfigurationFilePath = tempFile.Name,
                    MaxFileSizeInKilobytes = testCase.maxFileSize
                };

                int expectedReturnCode = testCase.expectedExitReason == ExitReason.None ? 0 : 1;

                RunAnalyzeCommand(
                    options: options,
                    expectedReturnCode: expectedReturnCode,
                    fileSystem: mockFileSystem.Object,
                    multithreaded: true,
                    exitReason: testCase.expectedExitReason);

                RunAnalyzeCommand(
                    options: options,
                    expectedReturnCode: expectedReturnCode,
                    fileSystem: mockFileSystem.Object,
                    multithreaded: false,
                    exitReason: testCase.expectedExitReason);
            }
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
                    Quiet = true,
                    DataToInsert = new OptionallyEmittedData[] { OptionallyEmittedData.Hashes },
                    ConfigurationFilePath = TestAnalyzeCommand.DefaultPolicyName,
                    Recurse = true,
                    OutputFilePath = path,
                    PrettyPrint = true,
                    Force = true,
                    SarifOutputVersion = SarifVersion.OneZeroZero
                };

                var command = new TestAnalyzeCommand();
                command.DefaultPluginAssemblies = new Assembly[] { this.GetType().Assembly };
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

            Run run = AnalyzeFile(location, TestRuleBehaviors.LogError);

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
            resultCount.Should().Be((int)TestRule.ErrorsCount.DefaultValue());
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
                resultCount.Should().Be((int)TestRule.ErrorsCount.DefaultValue());
                run.Results.Count((result) => result.Level == FailureLevel.Error).Should().Be((int)TestRule.ErrorsCount.DefaultValue());

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
                run.Invocations[0].ToolConfigurationNotifications.Count((notification) => notification.Level == FailureLevel.Error).Should().Be(1);
                run.Invocations[0].ToolConfigurationNotifications.Count((notification) => notification.Descriptor.Id == Errors.ERR997_AllRulesExplicitlyDisabled).Should().Be(1);

                // Warnings: one per disabled rule.
                run.Invocations[0].ToolConfigurationNotifications.Count((notification) => notification.Level == FailureLevel.Warning).Should().Be(2);
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
                Quiet = true,
                DataToInsert = new OptionallyEmittedData[] { OptionallyEmittedData.Hashes },
                ConfigurationFilePath = configValue,
                Recurse = true,
                OutputFilePath = "",
            };

            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.FileExists(It.IsAny<string>())).Returns(defaultFileExists);

            var command = new TestAnalyzeCommand(mockFileSystem.Object);

            string fileName = command.GetConfigurationFileName(options);
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
                    Text = string.Format("Found an issue in {0} (full path is {1}", filePath, fileName)
                }
            };
        }

        [Fact]
        public void AnalyzeCommandBase_GetFileNameFromUriWorks()
        {
            const string expectedResult = "file.ext";

            var testCases = new List<(string, string)>
            {
                (null, null),
                (@"", string.Empty),
                (@"file.ext", expectedResult),
                (@"/home/username/path/file.ext", expectedResult),
                (@"nfs://servername/folder/file.ext", expectedResult),
                (@"file:///home/username/path/file.ext", expectedResult),
                (@"ftp://ftp.example.com/folder/file.ext", expectedResult),
                (@"smb://servername/Share/folder/file.ext", expectedResult),
                (@"dav://example.hostname.com/folder/file.ext", expectedResult),
                (@"file://hostname/home/username/path/file.ext", expectedResult),
                (@"ftp://username@ftp.example.com/folder/file.ext", expectedResult),
                (@"scheme://servername.example.com/folder/file.ext", expectedResult),
                (@"https://github.com/microsoft/sarif-sdk/file.ext", expectedResult),
                (@"ssh://username@servername.example.com/folder/file.ext", expectedResult),
                (@"scheme://username@servername.example.com/folder/file.ext", expectedResult),
            };

            var testCasesWithSlashReplaceable = new List<(string, string)>
            {
                (@"\", string.Empty),
                (@".\", string.Empty),
                (@"..\", string.Empty),
                (@"path\", string.Empty),
                (@"\path\", string.Empty),
                (@".\path\", string.Empty),
                (@"..\path\", string.Empty),
                (@"\file.ext", expectedResult),
                (@".\file.ext", expectedResult),
                (@"..\file.ext", expectedResult),
                (@"path\file.ext", expectedResult),
                (@"\path\file.ext", expectedResult),
                (@"..\path\file.ext", expectedResult),
                (@".\..\path\file.ext", expectedResult),
            };

            var testCasesWindowsOnly = new List<(string, string)>
            {
                (@"C:\path\file.ext", expectedResult),
                (@"C:/path\file.ext", expectedResult),
                (@"C:\path/file.ext", expectedResult),
                (@"\\hostname\path\file.ext", expectedResult),
                (@"\\hostname/path\file.ext", expectedResult),
                (@"file:///C:/path/file.ext", expectedResult),
                (@"file:///C:\path/file.ext", expectedResult),
                (@"\\hostname\c:\path\file.ext", expectedResult),
                (@"\\hostname/c:\path\file.ext", expectedResult),
                (@"nfs://servername/folder\file.ext", expectedResult),
                (@"file://hostname/C:/path/file.ext", expectedResult),
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                testCases.AddRange(testCasesWithSlashReplaceable);
                testCases.AddRange(testCasesWindowsOnly);
            }
            else
            {
                testCases.AddRange(testCasesWithSlashReplaceable.Select(t => (t.Item1.Replace(@"\", @"/"), t.Item2)));
            }

            var sb = new StringBuilder();

            foreach ((string, string) testCase in testCases)
            {
                Uri uri = testCase.Item1 != null ? new Uri(testCase.Item1, UriKind.RelativeOrAbsolute) : null;
                string expectedFileName = testCase.Item2;

                string actualFileName = AnalyzeCommandBase<TestAnalysisContext, AnalyzeOptionsBase>.GetFileNameFromUri(uri);

                if (!Equals(actualFileName, expectedFileName))
                {
                    sb.AppendFormat("Incorrect file name returned for uri '{0}'. Expected '{1}' but saw '{2}'.", uri, expectedFileName, actualFileName).AppendLine();
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

            testCase.Files = ComprehensiveKindAndLevelsByFilePath;
            testCase.Verbose = false;
            RunResultsCachingTestCase(testCase, multithreaded: true);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase, multithreaded: true);
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

            testCase.Files = ComprehensiveKindAndLevelsByFilePath;
            testCase.Verbose = false;
            RunResultsCachingTestCase(testCase, multithreaded: true);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase, multithreaded: true);
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

            testCase.Files = ComprehensiveKindAndLevelsByFilePath;
            testCase.Verbose = false;
            RunResultsCachingTestCase(testCase, multithreaded: true);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase, multithreaded: true);
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

            testCase.Files = ComprehensiveKindAndLevelsByFilePath;
            testCase.Verbose = false;
            RunResultsCachingTestCase(testCase, multithreaded: true);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase, multithreaded: true);
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

            testCase.Files = ComprehensiveKindAndLevelsByFilePath;
            testCase.Verbose = false;
            RunResultsCachingTestCase(testCase, multithreaded: true);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase, multithreaded: true);
        }

        [Fact]
        public void AnalyzeCommandBase_AutomationDetailsTests()
        {
            const string whiteSpace = " ";
            const string automationId = "automation-id";
            var automationGuid = Guid.NewGuid();

            TestAnalyzeOptions[] enhancedOptions = new[]
            {
                new TestAnalyzeOptions
                {
                    AutomationId = automationId
                },
                new TestAnalyzeOptions
                {
                    AutomationGuid = automationGuid
                },
                new TestAnalyzeOptions
                {
                    AutomationId = automationId,
                    AutomationGuid = automationGuid
                },
                new TestAnalyzeOptions
                {
                },
                new TestAnalyzeOptions
                {
                    AutomationId = string.Empty,
                    AutomationGuid = null
                },
                new TestAnalyzeOptions
                {
                    AutomationId = string.Empty,
                    AutomationGuid = automationGuid
                },
                new TestAnalyzeOptions
                {
                    AutomationId = null,
                    AutomationGuid = null
                },
                new TestAnalyzeOptions
                {
                    AutomationId = whiteSpace,
                    AutomationGuid = Guid.Empty
                },
                new TestAnalyzeOptions
                {
                    AutomationId = string.Empty,
                    AutomationGuid = null
                },
                new TestAnalyzeOptions
                {
                    AutomationId = null,
                    AutomationGuid = Guid.Empty
                },
                new TestAnalyzeOptions
                {
                    AutomationId = null,
                    AutomationGuid = automationGuid
                },
                new TestAnalyzeOptions
                {
                    AutomationGuid = Guid.Empty
                },
                new TestAnalyzeOptions
                {
                    AutomationId = null
                },
            };

            foreach (TestAnalyzeOptions enhancedOption in enhancedOptions)
            {
                var testCase = new ResultsCachingTestCase
                {
                    Files = ComprehensiveKindAndLevelsByFileName,
                    PersistLogFileToDisk = true,
                };

                RunResultsCachingTestCase(testCase, enhancedOptions: enhancedOption);

                testCase.Files = ComprehensiveKindAndLevelsByFilePath;
                RunResultsCachingTestCase(testCase, multithreaded: true, enhancedOptions: enhancedOption);
            }
        }

        [Fact(Timeout = 5000, Skip = "TBD: this Coyote test will be enabled in a future nightly pipeline test run.")]
        public void AnalyzeCommandBase_ShouldGenerateSameResultsWhenRunningSingleAndMultithreaded_CoyoteTest()
        {
            var logger = new CoyoteTestOutputLogger(this.Output);
            Configuration config = Configuration.Create().WithTestingIterations(10).WithMaxSchedulingSteps(100);
            var engine = TestingEngine.Create(config, AnalyzeCommandBase_ShouldGenerateSameResultsWhenRunningSingleAndMultiThread_CoyoteHelper);
            engine.Logger = logger;

            string TestLogDirectory = ".";

            engine.Run();
            TestReport report = engine.TestReport;

            if (engine.TryEmitReports(TestLogDirectory, "AnalyzeCommandBase_ShouldGenerateSameResultsWhenRunningSingleAndMultiThread_CoyoteTest_Log", out IEnumerable<string> repoPaths))
            {
                foreach (string item in repoPaths)
                {
                    Output.WriteLine("See log file: {0}", item);
                }
            }

            Assert.True(report.NumOfFoundBugs == 0, $"Coyote found {report.NumOfFoundBugs} bug(s).");
        }

        [Fact]
        public void AnalyzeCommandBase_ShouldGenerateSameResultsWhenRunningSingleAndMultithreaded()
        {
            int[] scenarios = SetupScenarios();
            AnalyzeScenarios(scenarios);
        }

        [Fact]
        public void AnalyzeCommandBase_ShouldOnlyLogArtifactsWhenResultsAreFound()
        {
            const int expectedNumberOfArtifacts = 2;
            const int expectedNumberOfResultsWithErrors = 1;
            const int expectedNumberOfResultsWithWarnings = 1;
            var files = new List<string>
            {
                $@"{rootDir}Error.dll",
                $@"{rootDir}Warning.dll",
                $@"{rootDir}Note.dll",
                $@"{rootDir}Pass.dll",
                $@"{rootDir}NotApplicable.exe",
                $@"{rootDir}Informational.sys",
                $@"{rootDir}Open.cab",
                $@"{rootDir}Review.dll",
                $@"{rootDir}NoIssues.dll",
            };

            foreach (bool multithreaded in new bool[] { false, true })
            {
                var resultsCachingTestCase = new ResultsCachingTestCase
                {
                    Files = files,
                    PersistLogFileToDisk = true,
                    FileSystem = CreateDefaultFileSystemForResultsCaching(files, generateSameInput: false)
                };

                var options = new TestAnalyzeOptions
                {
                    TestRuleBehaviors = resultsCachingTestCase.TestRuleBehaviors,
                    OutputFilePath = resultsCachingTestCase.PersistLogFileToDisk ? Guid.NewGuid().ToString() : null,
                    TargetFileSpecifiers = new string[] { Guid.NewGuid().ToString() },
                    Kind = new List<ResultKind> { ResultKind.Fail },
                    Level = new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error },
                    DataToInsert = new OptionallyEmittedData[] { OptionallyEmittedData.Hashes },
                };

                Run run = RunAnalyzeCommand(options, resultsCachingTestCase, multithreaded: multithreaded);

                // Hashes is enabled and we should expect to see two artifacts because we have:
                // one result with Error level and one result with Warning level.
                run.Artifacts.Should().HaveCount(expectedNumberOfArtifacts);
                run.Results.Count(r => r.Level == FailureLevel.Error).Should().Be(expectedNumberOfResultsWithErrors);
                run.Results.Count(r => r.Level == FailureLevel.Warning).Should().Be(expectedNumberOfResultsWithWarnings);
            }
        }

        [Fact]
        public void AnalyzeCommandBase_ShouldNotThrowException_WhenAnalyzingSameFileBasedOnTwoTargetFileSpecifiers()
        {
            var files = new List<string>
            {
                $@"{rootDir}Error.dll"
            };

            Action action = () =>
            {
                foreach (bool multithreaded in new bool[] { false, true })
                {
                    var resultsCachingTestCase = new ResultsCachingTestCase
                    {
                        Files = files,
                        PersistLogFileToDisk = true,
                        FileSystem = CreateDefaultFileSystemForResultsCaching(files, generateSameInput: true)
                    };

                    var options = new TestAnalyzeOptions
                    {
                        TestRuleBehaviors = resultsCachingTestCase.TestRuleBehaviors,
                        OutputFilePath = resultsCachingTestCase.PersistLogFileToDisk ? Guid.NewGuid().ToString() : null,
                        TargetFileSpecifiers = new string[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() },
                        Kind = new List<ResultKind> { ResultKind.Fail },
                        Level = new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error },
                        DataToInsert = new OptionallyEmittedData[] { OptionallyEmittedData.Hashes },
                    };

                    TestRule.s_testRuleBehaviors = resultsCachingTestCase.TestRuleBehaviors.AccessibleOutsideOfContextOnly();
                    RunAnalyzeCommand(options,
                                      resultsCachingTestCase.FileSystem,
                                      resultsCachingTestCase.ExpectedReturnCode,
                                      multithreaded: multithreaded);
                }
            };

            action.Should().NotThrow();
        }

        [Test]
        private void AnalyzeCommandBase_ShouldGenerateSameResultsWhenRunningSingleAndMultiThread_CoyoteHelper()
        {
            int[] scenarios = SetupScenarios(true);
            AnalyzeScenarios(scenarios);
        }

        private int[] SetupScenarios(bool IsCoyoteTest = false)
        {
            Coyote.Random.Generator random = Coyote.Random.Generator.Create();

            return IsCoyoteTest ? new int[] { (random.NextInteger(10) + 1) } : new int[] { 10, 50, 100 };
        }

        private void AnalyzeScenarios(int[] scenarios)
        {
            foreach (int scenario in scenarios)
            {
                var singleThreadTargets = new List<string>();
                var multiThreadTargets = new List<string>();

                for (int i = 0; i < scenario; i++)
                {
                    singleThreadTargets.Add($"Error.{i}.cpp");
                    multiThreadTargets.Add($@"{rootDir}Error.{i}.cpp");
                }

                for (int i = 0; i < scenario / 2; i++)
                {
                    singleThreadTargets.Add($"Warning.{i}.cpp");
                    multiThreadTargets.Add($@"{rootDir}Warning.{i}.cpp");
                }

                for (int i = 0; i < scenario / 5; i++)
                {
                    singleThreadTargets.Add($"Note.{i}.cpp");
                    multiThreadTargets.Add($@"{rootDir}Note.{i}.cpp");
                }

                var testCase = new ResultsCachingTestCase
                {
                    Files = singleThreadTargets,
                    PersistLogFileToDisk = true,
                    FileSystem = null
                };

                var options = new TestAnalyzeOptions
                {
                    TestRuleBehaviors = testCase.TestRuleBehaviors,
                    OutputFilePath = testCase.PersistLogFileToDisk ? Guid.NewGuid().ToString() : null,
                    TargetFileSpecifiers = new string[] { Guid.NewGuid().ToString() },
                    Kind = new List<ResultKind> { ResultKind.Fail },
                    Level = new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error },
                    DataToInsert = new OptionallyEmittedData[] { OptionallyEmittedData.Hashes },
                };

                Run runSingleThread = RunAnalyzeCommand(options, testCase);

                testCase.FileSystem = null;
                testCase.Files = multiThreadTargets;
                Run runMultithreaded = RunAnalyzeCommand(options, testCase, multithreaded: true);

                runMultithreaded.Results.Should().BeEquivalentTo(runSingleThread.Results);
                runMultithreaded.Artifacts.Should().BeEquivalentTo(runSingleThread.Artifacts);
            }
        }

        [Fact]
        public void AnalyzeCommandBase_MultithreadedShouldUseCacheIfFilesAreTheSame()
        {
            // Generating 20 files with different names but same content.
            // Generally, we expect the test analyzer to produce a result 
            // based on the file name. Because every file is a duplicate 
            // of every other in this case, though, we expect to see a 
            // result for every file, because the first one analyzed 
            // produces a result and therefore every identical file (by
            // file hash, not by file name) will also produce that result.
            RunMultithreadedAnalyzeCommand(ComprehensiveKindAndLevelsByFilePath,
                                           generateDuplicateScanTargets: true,
                                           expectedResultCode: 0,
                                           expectedResultCount: 20);

            // Generating 20 files with different names and content.
            // For this case, our expected result count matches the default
            // behavior of the test analyzer and our default analyzer settings.
            // By default, our analysis produces output for errors and warnings
            // and there happen to be 7 files that comprises these failure levels.
            RunMultithreadedAnalyzeCommand(ComprehensiveKindAndLevelsByFilePath,
                                           generateDuplicateScanTargets: false,
                                           expectedResultCode: 0,
                                           expectedResultCount: 7);
        }

        private static readonly IList<string> ComprehensiveKindAndLevelsByFileName = new List<string>
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
        };

        private static readonly string rootDir = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}";

        private static readonly IList<string> ComprehensiveKindAndLevelsByFilePath = new List<string>
        {
            

            // Every one of these files will be regarded as identical in content by level/kind. So every file
            // with 'Error' as a prefix should produce an error result, whether using results caching or not.
            // We distinguish file names as this is required in the actual scenario, i.e., when 'replaying'
            // cached results we must retain the unique fully qualified directory + file name for each copy.
            // In actual production systems, this differentiation mostly occurs by directory name (i.e., copies
            // of files tend to have the same name but appear in different directories). For a source code scanner,
            // however, two files in the same directory may hash the same (an empty file that produces no scan
            // results is an obvious case).
            $"{rootDir}Error.1.of.5.cpp",
            $"{rootDir}Error.2.of.5.cs",
            $"{rootDir}Error.3.of.5.exe",
            $"{rootDir}Error.4.of.5.h",
            $"{rootDir}Error.5.of.5.sys",
            $"{rootDir}Warning.1.of.2.java",
            $"{rootDir}Warning.2.of.2.cs",
            $"{rootDir}Note.1.of.3.dll",
            $"{rootDir}Note.2.of.3.exe",
            $"{rootDir}Note.3.of.3jar",
            $"{rootDir}Pass.1.of.4.cs",
            $"{rootDir}Pass.2.of.4.cpp",
            $"{rootDir}Pass.3.of.4.exe",
            $"{rootDir}Pass.4.of.4.dll",
            $"{rootDir}NotApplicable.1.of.2.js",
            $"{rootDir}NotApplicable.2.of.2.exe",
            $"{rootDir}Informational.1.of.1.sys",
            $"{rootDir}Open.1.of.1.cab",
            $"{rootDir}Review.1.of.2.txt",
            $"{rootDir}Review.2.of.2.dll"
        };

        private static void RunResultsCachingTestCase(ResultsCachingTestCase testCase,
                                                      bool multithreaded = false,
                                                      TestAnalyzeOptions enhancedOptions = null)
        {
            // This makes sure that we will reinitialize the mock file system. This
            // allows callers to reuse test case instances, by adjusting specific
            // property values. The mock file system cannot be reused in this way, once
            // a test executes, it must be reset.
            testCase.FileSystem = null;

            var options = new TestAnalyzeOptions
            {
                TestRuleBehaviors = testCase.TestRuleBehaviors,
                OutputFilePath = testCase.PersistLogFileToDisk ? Guid.NewGuid().ToString() : null,
                TargetFileSpecifiers = new string[] { Guid.NewGuid().ToString() },
                Kind = new List<ResultKind> { ResultKind.Fail },
                Level = new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error },
            };

            EnhanceOptions(options, enhancedOptions);

            if (testCase.Verbose)
            {
                options.Kind = new List<ResultKind> { ResultKind.Informational, ResultKind.Open, ResultKind.Review, ResultKind.Fail, ResultKind.Pass, ResultKind.NotApplicable, ResultKind.None };
                options.Level = new List<FailureLevel> { FailureLevel.Error, FailureLevel.Warning, FailureLevel.Note, FailureLevel.None };
            }

            Run runWithoutCaching = RunAnalyzeCommand(options, testCase, multithreaded);

            options.DataToInsert = new OptionallyEmittedData[] { OptionallyEmittedData.Hashes };
            Run runWithCaching = RunAnalyzeCommand(options, testCase, multithreaded);

            // Core static analysis results
            runWithCaching.Results.Count.Should().Be(runWithoutCaching.Results.Count);

            for (int i = 0; i < runWithCaching.Results.Count; i++)
            {
                Result withCache = runWithCaching.Results[i];
                Result withoutCache = runWithCaching.Results[i];

                withCache.Level.Should().Be(withoutCache.Level);
                withCache.RuleId.Should().Be(withoutCache.RuleId);
                withCache.Message.Should().BeEquivalentTo(withoutCache.Message);

                if (testCase.PersistLogFileToDisk)
                {
                    withCache.Locations.Count.Should().Be(withoutCache.Locations.Count);
                    withCache.Locations[0].PhysicalLocation.ArtifactLocation.Uri.Should().Be(withoutCache.Locations[0].PhysicalLocation.ArtifactLocation.Uri);
                }
            }

            if (testCase.PersistLogFileToDisk)
            {
                runWithCaching.Artifacts.Should().NotBeEmpty();

                if (string.IsNullOrWhiteSpace(options.AutomationId) && options.AutomationGuid == null)
                {
                    runWithCaching.AutomationDetails.Should().Be(null);
                }

                if (!string.IsNullOrWhiteSpace(options.AutomationId))
                {
                    runWithCaching.AutomationDetails.Id.Should().Be(options.AutomationId);
                }

                if (options.AutomationGuid != null)
                {
                    runWithCaching.AutomationDetails.Guid.Should().Be(options.AutomationGuid);
                }

                runWithCaching.AutomationDetails.Should().BeEquivalentTo(runWithoutCaching.AutomationDetails);
            }
            // Tool configuration errors, such as 'Could not locate scan target PDB.'
            runWithoutCaching.Invocations?[0].ToolConfigurationNotifications?.Should()
                .BeEquivalentTo(runWithCaching.Invocations?[0].ToolConfigurationNotifications);

            // Not yet explicitly tested
            runWithoutCaching.Invocations?[0].ToolExecutionNotifications?.Should()
                .BeEquivalentTo(runWithCaching.Invocations?[0].ToolExecutionNotifications);
        }

        private static void EnhanceOptions(TestAnalyzeOptions current, TestAnalyzeOptions enhancement)
        {
            current.AutomationId ??= enhancement?.AutomationId;
            current.AutomationGuid ??= enhancement?.AutomationGuid;
        }

        private static IFileSystem CreateDefaultFileSystemForResultsCaching(IList<string> files, bool generateSameInput = false)
        {
            // This helper creates a file system that generates unique or entirely
            // duplicate content for every file passed in the 'files' argument.

            string logFileContents = Guid.NewGuid().ToString();

            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>())).Returns(new string[0]);
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), SearchOption.TopDirectoryOnly)).Returns(files);
            mockFileSystem.Setup(x => x.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(files);

            for (int i = 0; i < files.Count; i++)
            {
                string fullyQualifiedName = Path.GetFileName(files[i]) == files[i]
                    ? Environment.CurrentDirectory + Path.DirectorySeparatorChar + files[i]
                    : files[i];
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullyQualifiedName);
                mockFileSystem.Setup(x => x.FileReadAllText(It.Is<string>(f => f == fullyQualifiedName))).Returns(logFileContents);

                mockFileSystem.Setup(x => x.FileOpenRead(It.Is<string>(f => f == fullyQualifiedName)))
                    .Returns(new NonDisposingDelegatingStream(new MemoryStream(Encoding.UTF8.GetBytes(generateSameInput ? logFileContents : fileNameWithoutExtension))));
            }
            return mockFileSystem.Object;
        }

        private static Run RunAnalyzeCommand(TestAnalyzeOptions options,
                                             ResultsCachingTestCase testCase,
                                             bool multithreaded = false)
        {
            Run run = null;
            SarifLog sarifLog;
            try
            {
                TestRule.s_testRuleBehaviors = testCase.TestRuleBehaviors.AccessibleOutsideOfContextOnly();
                sarifLog = RunAnalyzeCommand(options, testCase.FileSystem, testCase.ExpectedReturnCode, multithreaded);
                run = sarifLog.Runs[0];

                run.Results.Count.Should().Be(testCase.ExpectedResultsCount);
            }
            finally
            {
                TestRule.s_testRuleBehaviors = TestRuleBehaviors.None;
            }
            return run;
        }

        private static SarifLog RunAnalyzeCommand(TestAnalyzeOptions options,
                                                  IFileSystem fileSystem,
                                                  int expectedReturnCode,
                                                  bool multithreaded,
                                                  ExitReason exitReason = ExitReason.None)
        {
            // If no log file is specified, we will convert the console output into a log file
            bool captureConsoleOutput = string.IsNullOrEmpty(options.OutputFilePath);

            ITestAnalyzeCommand command;
            if (multithreaded)
            {
                command = new TestMultithreadedAnalyzeCommand(fileSystem) { _captureConsoleOutput = captureConsoleOutput };
            }
            else
            {
                command = new TestAnalyzeCommand(fileSystem) { _captureConsoleOutput = captureConsoleOutput };
            }
            command.DefaultPluginAssemblies = new Assembly[] { typeof(AnalyzeCommandBaseTests).Assembly };

            try
            {
                HashUtilities.FileSystem = fileSystem;
                command.Run(options).Should().Be(expectedReturnCode);
            }
            finally
            {
                HashUtilities.FileSystem = null;
            }

            if (exitReason != ExitReason.None)
            {
                var exception = command.ExecutionException as ExitApplicationException<ExitReason>;
                exception.Should().NotBeNull();
                exception.ExitReason.Should().Be(exitReason);
            }

            ConsoleLogger consoleLogger = multithreaded
                ? (command as TestMultithreadedAnalyzeCommand)._consoleLogger
                : (command as TestAnalyzeCommand)._consoleLogger;

            return captureConsoleOutput
                ? ConvertConsoleOutputToSarifLog(consoleLogger.CapturedOutput)
                : JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(options.OutputFilePath));
        }

        private static void RunMultithreadedAnalyzeCommand(IList<string> files,
                                                           bool generateDuplicateScanTargets,
                                                           int expectedResultCode,
                                                           int expectedResultCount)
        {
            var testCase = new ResultsCachingTestCase
            {
                Files = files,
                PersistLogFileToDisk = true,
                FileSystem = CreateDefaultFileSystemForResultsCaching(files, generateDuplicateScanTargets)
            };

            var options = new TestAnalyzeOptions
            {
                OutputFilePath = Guid.NewGuid().ToString(),
                TargetFileSpecifiers = new string[] { Guid.NewGuid().ToString() },
                Kind = new List<ResultKind> { ResultKind.Fail },
                Level = new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error },
                DataToInsert = new OptionallyEmittedData[] { OptionallyEmittedData.Hashes }
            };

            try
            {
                TestRule.s_testRuleBehaviors = testCase.TestRuleBehaviors.AccessibleOutsideOfContextOnly();

                var command = new TestMultithreadedAnalyzeCommand(testCase.FileSystem)
                {
                    DefaultPluginAssemblies = new Assembly[] { typeof(AnalyzeCommandBaseTests).Assembly }
                };

                HashUtilities.FileSystem = testCase.FileSystem;
                int result = command.Run(options);
                result.Should().Be(expectedResultCode);

                SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(options.OutputFilePath));
                sarifLog.Runs[0].Results.Count.Should().Be(expectedResultCount);

                HashSet<string> hashes = new HashSet<string>();
                foreach (Artifact artifact in sarifLog.Runs[0].Artifacts)
                {
                    hashes.Add(artifact.Hashes["sha-256"]);
                }

                int expectedUniqueFileHashCount = generateDuplicateScanTargets ? 1 : expectedResultCount;
                hashes.Count.Should().Be(expectedUniqueFileHashCount);
            }
            finally
            {
                TestRule.s_testRuleBehaviors = TestRuleBehaviors.None;
            }
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
                    return _fileSystem ??= CreateDefaultFileSystemForResultsCaching(Files);
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
                    ? Files.Count - Files.Count((f) => f.Contains("NotApplicable")) : 0);

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
            public bool NotificationsWillBeConvertedToErrorResults => TestRuleBehaviors == TestRuleBehaviors.RaiseTargetParseError && !PersistLogFileToDisk;
        }

        #endregion ResultsCachingTestsAndHelpers

        [Fact]
        public void CheckIncompatibleRules_DisableIncompatibleRuleAndContinueAnalysis()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001" },
                new TestRule { Id = "TEST1002" },
                new TestRule { Id = "TEST1003", IncompatibleRuleIds = new HashSet<string> { "TEST1001" }, IncompatibleRuleHandling = IncompatibleRuleHandling.DisableAndContinueAnalysis },
            };

            var disabledSkimmers = new HashSet<string>();

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();
            TestAnalyzeCommand command = CreateTestCommand(context, consoleLogger);

            command.CheckIncompatibleRules(skimmers, context, disabledSkimmers);

            disabledSkimmers.Count.Should().Be(1);
            disabledSkimmers.First().Should().Be("TEST1001");
            context.RuntimeErrors.Should().Be(RuntimeConditions.RuleIsIncompatibleWithAnotherRule);
            consoleLogger.CapturedOutput.Contains(Warnings.Wrn998_IncompatibleRuleDetected).Should().BeTrue();
        }

        [Fact]
        public void CheckIncompatibleRules_ExitAnalysis()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001" },
                new TestRule { Id = "TEST1002", IncompatibleRuleIds = new HashSet<string> { "TEST1003" }, IncompatibleRuleHandling = IncompatibleRuleHandling.ExitAnalysis },
                new TestRule { Id = "TEST1003" },
            };

            var disabledSkimmers = new HashSet<string>();

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();
            TestAnalyzeCommand command = CreateTestCommand(context, consoleLogger);

            ExitApplicationException<ExitReason> exception = Assert.Throws<ExitApplicationException<ExitReason>>(
                () => command.CheckIncompatibleRules(skimmers, context, disabledSkimmers));

            exception.ExitReason.Should().Be(ExitReason.IncompatibleRulesDetected);
            disabledSkimmers.Count.Should().Be(0);
            context.RuntimeErrors.Should().Be(RuntimeConditions.RuleIsIncompatibleWithAnotherRule);
            consoleLogger.CapturedOutput.Contains(Errors.ERR998_IncompatibleRuleDetected);
        }

        [Fact]
        public void CheckIncompatibleRules_Ignore()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001", IncompatibleRuleIds = new HashSet<string> { "TEST1002" }, IncompatibleRuleHandling = IncompatibleRuleHandling.Ignore },
                new TestRule { Id = "TEST1002" },
                new TestRule { Id = "TEST1003" },
            };

            var disabledSkimmers = new HashSet<string>();

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();
            TestAnalyzeCommand command = CreateTestCommand(context, consoleLogger);

            command.CheckIncompatibleRules(skimmers, context, disabledSkimmers);

            disabledSkimmers.Count.Should().Be(0);
            context.RuntimeErrors.Should().Be(RuntimeConditions.None);
            consoleLogger.CapturedOutput.Should().BeNull();
        }

        [Fact]
        public void CheckIncompatibleRules_IncompatibleRuleDoesNotExist()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001", IncompatibleRuleIds = new HashSet<string> { "NA9999" }, IncompatibleRuleHandling = IncompatibleRuleHandling.ExitAnalysis },
                new TestRule { Id = "TEST1002", IncompatibleRuleHandling = IncompatibleRuleHandling.Ignore },
                new TestRule { Id = "TEST1003", IncompatibleRuleHandling = IncompatibleRuleHandling.Ignore },
            };

            var disabledSkimmers = new HashSet<string>();

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();
            TestAnalyzeCommand command = CreateTestCommand(context, consoleLogger);

            command.CheckIncompatibleRules(skimmers, context, disabledSkimmers);

            disabledSkimmers.Count.Should().Be(0);
            context.RuntimeErrors.Should().Be(RuntimeConditions.None);
            consoleLogger.CapturedOutput.Should().BeNull();
        }

        [Fact]
        public void CheckIncompatibleRules_OriginalRuleAlreadyDisabled()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1002", IncompatibleRuleIds = new HashSet<string> { "TEST1001" }, IncompatibleRuleHandling = IncompatibleRuleHandling.DisableAndContinueAnalysis },
                new TestRule { Id = "TEST1001" },
                new TestRule { Id = "TEST1003" },
            };

            var disabledSkimmers = new HashSet<string>() { "TEST1002" };

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();
            TestAnalyzeCommand command = CreateTestCommand(context, consoleLogger);

            command.CheckIncompatibleRules(skimmers, context, disabledSkimmers);

            disabledSkimmers.Count.Should().Be(1);
            context.RuntimeErrors.Should().Be(RuntimeConditions.None);
            consoleLogger.CapturedOutput.Should().BeNull();
        }

        [Fact]
        public void CheckIncompatibleRules_AllIncompatibleRules()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001" },
                new TestRule { Id = "TEST1002", IncompatibleRuleIds = new HashSet<string> { "TEST1003" }, IncompatibleRuleHandling = IncompatibleRuleHandling.DisableAndContinueAnalysis },
                new TestRule { Id = "TEST1003", IncompatibleRuleIds = new HashSet<string> { "TEST1001", "TEST1002" }, IncompatibleRuleHandling = IncompatibleRuleHandling.DisableAndContinueAnalysis },
            };

            var disabledSkimmers = new HashSet<string>();

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();
            TestAnalyzeCommand command = CreateTestCommand(context, consoleLogger);

            command.CheckIncompatibleRules(skimmers, context, disabledSkimmers);

            disabledSkimmers.Count.Should().Be(3);
            context.RuntimeErrors.Should().Be(RuntimeConditions.RuleIsIncompatibleWithAnotherRule);
            consoleLogger.CapturedOutput.Contains(Warnings.Wrn998_IncompatibleRuleDetected).Should().BeTrue();
        }

        [Fact]
        public void CheckIncompatibleRules_IncompatibleRuleHandlingOverride()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001" },
                new TestRule { Id = "TEST1002", IncompatibleRuleIds = new HashSet<string> { "TEST1001" }, IncompatibleRuleHandling = IncompatibleRuleHandling.DisableAndContinueAnalysis },
                new TestRule { Id = "TEST1003", IncompatibleRuleIds = new HashSet<string> { "TEST1001" }, IncompatibleRuleHandling = IncompatibleRuleHandling.ExitAnalysis },
            };

            var disabledSkimmers = new HashSet<string>();

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();
            TestAnalyzeCommand command = CreateTestCommand(context, consoleLogger);

            ExitApplicationException<ExitReason> exception = Assert.Throws<ExitApplicationException<ExitReason>>(
                () => command.CheckIncompatibleRules(skimmers, context, disabledSkimmers));

            exception.ExitReason.Should().Be(ExitReason.IncompatibleRulesDetected);
            disabledSkimmers.Count.Should().Be(0);
            context.RuntimeErrors.Should().Be(RuntimeConditions.RuleIsIncompatibleWithAnotherRule);
            consoleLogger.CapturedOutput.Contains(Errors.ERR998_IncompatibleRuleDetected);
        }

        private TestAnalyzeCommand CreateTestCommand(TestAnalysisContext context, ConsoleLogger consoleLogger)
        {
            var command = new TestAnalyzeCommand();
            var logger = new AggregatingLogger();
            logger.Loggers.Add(consoleLogger);
            context.Logger = logger;

            return command;
        }

        private void PostUriTestHelper(string postUri, int expectedReturnCode, RuntimeConditions runtimeConditions)
        {
            string location = GetThisTestAssemblyFilePath();

            Run run = AnalyzeFile(
                location,
                TestRuleBehaviors.LogError,
                postUri: postUri,
                expectedReturnCode: expectedReturnCode,
                runtimeConditions: runtimeConditions);

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
            resultCount.Should().Be((int)TestRule.ErrorsCount.DefaultValue());
            run.Results[0].Kind.Should().Be(ResultKind.Fail);

            toolNotificationCount.Should().Be(0);
            configurationNotificationCount.Should().Be(0);
        }
    }
}
