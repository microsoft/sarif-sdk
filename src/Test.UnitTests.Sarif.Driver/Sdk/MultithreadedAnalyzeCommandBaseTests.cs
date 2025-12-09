// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using FluentAssertions;
using FluentAssertions.Execution;

using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Moq;

using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class MultithreadedAnalyzeCommandBaseTests
    {
        private const int FAILURE = CommandBase.FAILURE;
        private const int SUCCESS = CommandBase.SUCCESS;

        private readonly ITestOutputHelper Output;

        public MultithreadedAnalyzeCommandBaseTests(ITestOutputHelper output)
        {
            this.Output = output;
            Output.WriteLine($"The seed that will be used is: {TestRule.s_seed}");
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_InvalidZipArchive()
        {
            var logger = new TestMessageLogger();

            // Create an empty/invalid zip file that will provoke a
            // System.IO.InvalidDataException: 'Central Directory corrupt.'
            // exception on attempting to initialize the ZipArchive.
            using var tempFile = new TempFile(requestedExtension: ".zip");
            File.WriteAllBytes(tempFile.Name, new byte[] { 0 });

            var artifact = new EnumeratedArtifact(new FileSystem())
            {
                Uri = new Uri(tempFile.Name),
            };

            var context = new TestAnalysisContext
            {
                TargetsProvider = new ArtifactProvider(new[] { artifact }),
                MaxFileSizeInKilobytes = 1,
                Logger = logger,
            };

            int result = new TestMultithreadedAnalyzeCommand().Run(options: null, ref context);
            result.Should().Be(FAILURE);

            logger.ConfigurationNotifications.Count.Should().Be(2);
            logger.ConfigurationNotifications[0].Level.Should().Be(FailureLevel.Error);

            string expected = $"{Path.GetFileName(tempFile.Name)}: error ERR1000.ParseError: An exception was raised attempting to open a zip archive or Open Packaging Conventions (OPC) document.";
            logger.ConfigurationNotifications[0].Message.Text.Should().Be(expected);

            context.RuntimeErrors.Should().Be(RuntimeConditions.NoValidAnalysisTargets | RuntimeConditions.TargetParseError);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_ShouldHandle_EnumeratedArtifactWithOrWithoutStream()
        {
            var logger = new TestMessageLogger();

            // Create a valid zip file on disk
            using var tempFile = new TempFile(requestedExtension: ".zip");
            using (ZipArchive zip = ZipFile.Open(tempFile.Name, ZipArchiveMode.Create))
            {
                ZipArchiveEntry entry = zip.CreateEntry("test.txt");
                using Stream entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream);
                writer.Write("Hello, world!");
            }

            var dummyAbsoluteUri = new Uri("https://example.com/valid.zip", UriKind.Absolute);
            var dummyRelativeUri = new Uri("test/valid.zip", UriKind.Relative);
            var validUri = new Uri(tempFile.Name);
            var streamContent = new MemoryStream(File.ReadAllBytes(tempFile.Name));

            var testArtifacts = new List<EnumeratedArtifact>
            {
                // 0. Only Dummy Absolute Uri (negative test case)
                new EnumeratedArtifact(new FileSystem())
                {
                    Uri = dummyAbsoluteUri,
                },
                
                // 1. Only Dummy Relative Uri (negative test case)
                new EnumeratedArtifact(new FileSystem())
                {
                    Uri = dummyRelativeUri,
                },

                // 2. Only Valid Uri
                new EnumeratedArtifact(new FileSystem())
                {
                    Uri = validUri
                },

                // 3. Valid Uri + Stream
                new EnumeratedArtifact(new FileSystem())
                {
                    Uri = validUri,
                    Stream = streamContent
                },

                // 4. Dummy Absolute Uri + Stream
                new EnumeratedArtifact(new FileSystem())
                {
                    Uri = dummyAbsoluteUri,
                    Stream = streamContent
                },

                // 5. Dummy Relative Uri + Stream
                new EnumeratedArtifact(new FileSystem())
                {
                    Uri = dummyRelativeUri,
                    Stream = streamContent,
                }
            };

            for (int i = 0; i < testArtifacts.Count; i++)
            {
                streamContent.Position = 0;
                EnumeratedArtifact artifact = testArtifacts[i];

                var contextFromBytes = new TestAnalysisContext
                {
                    TargetsProvider = new ArtifactProvider(new[] { artifact }),
                    MaxFileSizeInKilobytes = 1,
                    Logger = logger,
                };

                int resultFromBytes = new TestMultithreadedAnalyzeCommand().Run(options: null, ref contextFromBytes);

                if (i <= 1)
                {
                    resultFromBytes.Should().Be(FAILURE);
                    contextFromBytes.RuntimeErrors.Should().Be(RuntimeConditions.ExceptionInEngine);
                }
                else
                {
                    resultFromBytes.Should().Be(SUCCESS);
                    contextFromBytes.RuntimeErrors.Should().Be(RuntimeConditions.None);
                }
            }
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_SkipsOversizedOpcFilesBeforeOpening()
        {
            var logger = new TestMessageLogger();
            var mockFileSystem = new Mock<IFileSystem>();

            string opcFilePath = Path.Combine(Environment.CurrentDirectory, "oversized.pkg");
            mockFileSystem.Setup(x => x.FileExists(opcFilePath)).Returns(true);
            mockFileSystem.Setup(x => x.FileInfoLength(opcFilePath)).Returns(2048); // 2KB
            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(new[] { opcFilePath });

            var context = new TestAnalysisContext
            {
                TargetsProvider = null,
                MaxFileSizeInKilobytes = 1,
                Logger = logger,
                FileSystem = mockFileSystem.Object,
                TargetFileSpecifiers = new StringSet(new[] { "*.pkg" })
            };

            context.Policy.SetProperty(AnalyzeContextBase.OpcFileExtensionsProperty, new StringSet(new[] { ".pkg" }));

            var command = new TestMultithreadedAnalyzeCommand(mockFileSystem.Object);

            int result = command.Run(options: null, ref context);

            result.Should().Be(FAILURE);
            context.RuntimeErrors.Should().Be(RuntimeConditions.NoValidAnalysisTargets | RuntimeConditions.OneOrMoreFilesSkippedDueToExceedingSizeLimits);

            // Verify the OPC file was never opened (no file open attempt should have been made)
            mockFileSystem.Verify(x => x.FileOpenRead(It.IsAny<string>()), Times.Never);

            // Verify size limit warning was logged
            logger.ConfigurationNotifications.Should().Contain(n =>
                n.Descriptor.Id == Warnings.Wrn997_OneOrMoreFilesSkippedDueToExceedingSizeLimits);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_SkipsEmptyOpcFilesBeforeOpening()
        {
            var logger = new TestMessageLogger();
            var mockFileSystem = new Mock<IFileSystem>();

            string opcFilePath = Path.Combine(Environment.CurrentDirectory, "empty.pkg");
            mockFileSystem.Setup(x => x.FileExists(opcFilePath)).Returns(true);
            mockFileSystem.Setup(x => x.FileInfoLength(opcFilePath)).Returns(0); // Empty file
            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
                .Returns(new[] { opcFilePath });

            var context = new TestAnalysisContext
            {
                TargetsProvider = null,
                MaxFileSizeInKilobytes = 1000,
                Logger = logger,
                FileSystem = mockFileSystem.Object,
                TargetFileSpecifiers = new StringSet(new[] { "*.pkg" })
            };

            context.Policy.SetProperty(AnalyzeContextBase.OpcFileExtensionsProperty, new StringSet(new[] { ".pkg" }));

            var command = new TestMultithreadedAnalyzeCommand(mockFileSystem.Object);

            int result = command.Run(options: null, ref context);

            result.Should().Be(FAILURE);
            context.RuntimeErrors.Should().Be(RuntimeConditions.NoValidAnalysisTargets | RuntimeConditions.OneOrMoreEmptyFilesSkipped);

            // Verify the OPC file was never opened
            mockFileSystem.Verify(x => x.FileOpenRead(It.IsAny<string>()), Times.Never);

            // Verify empty file note was logged
            logger.ConfigurationNotifications.Should().Contain(n =>
                n.Descriptor.Id == Notes.Msg002_EmptyFileSkipped);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_ScanWithFilesThatExceedSizeLimitEmitsSkippedFilesWarning()
        {
            var logger = new TestMessageLogger();

            var artifact = new EnumeratedArtifact(new FileSystem())
            {
                Uri = new Uri(@"c:\testfile1.txt"),
                Contents = new string('x', 1024), // Within threshold.
            };

            var anotherArtifact = new EnumeratedArtifact(new FileSystem())
            {
                Uri = new Uri(@"c:\testfile2.txt"),
                Contents = new string('x', 1025), // Just exceeds 1k.
            };

            var context = new TestAnalysisContext
            {
                TargetsProvider = new ArtifactProvider(new[] { artifact, anotherArtifact }),
                MaxFileSizeInKilobytes = 1,
                Logger = logger,
            };

            int result = new TestMultithreadedAnalyzeCommand().Run(options: null, ref context);

            RuntimeConditions conditions = RuntimeConditions.OneOrMoreFilesSkippedDueToExceedingSizeLimits;

            using (new AssertionScope())
            {
                context.RuntimeErrors.Should().Be(conditions);

                logger.ConfigurationNotifications.Where(n => n.Level == FailureLevel.Note).Count().Should().Be(1);
                Notification note = logger.ConfigurationNotifications.Where(n => n.Level == FailureLevel.Note).First();
                note.Descriptor.Id.Should().Be(Notes.Msg002_FileExceedingSizeLimitSkipped);

                logger.ConfigurationNotifications.Where(n => n.Level == FailureLevel.Warning).Count().Should().Be(1);
                Notification warning = logger.ConfigurationNotifications.Where(n => n.Level == FailureLevel.Warning).First();
                warning.Descriptor.Id.Should().Be(Warnings.Wrn997_OneOrMoreFilesSkippedDueToExceedingSizeLimits);
            }
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_ScanWithOnlyFilesThatExceedSizeLimitEmitsSkippedFilesWarning()
        {
            var logger = new TestMessageLogger();

            var artifact = new EnumeratedArtifact(new FileSystem())
            {
                Uri = new Uri(@"c:\testfile1.txt"),
                Contents = new string('x', 1025), // Just exceeds 1k.
            };

            var anotherArtifact = new EnumeratedArtifact(new FileSystem())
            {
                Uri = new Uri(@"c:\testfile2.txt"),
                Contents = new string('x', 1025), // Just exceeds 1k.
            };

            EnumeratedArtifact[] allArtifacts = new[] { artifact, anotherArtifact };

            var context = new TestAnalysisContext
            {
                TargetsProvider = new ArtifactProvider(allArtifacts),
                MaxFileSizeInKilobytes = 1,
                Logger = logger,
            };

            int result = new TestMultithreadedAnalyzeCommand().Run(options: null, ref context);

            RuntimeConditions conditions = RuntimeConditions.NoValidAnalysisTargets |
                                           RuntimeConditions.OneOrMoreFilesSkippedDueToExceedingSizeLimits;

            using (new AssertionScope())
            {
                context.RuntimeErrors.Should().Be(conditions);

                logger.ConfigurationNotifications.Where(n => n.Level == FailureLevel.Note).Count().Should().Be(allArtifacts.Length);
                Notification note = logger.ConfigurationNotifications.Where(n => n.Level == FailureLevel.Note).First();
                note.Descriptor.Id.Should().Be(Notes.Msg002_FileExceedingSizeLimitSkipped);

                logger.ConfigurationNotifications.Where(n => n.Level == FailureLevel.Warning).Count().Should().Be(1);
                Notification warning = logger.ConfigurationNotifications.Where(n => n.Level == FailureLevel.Warning).First();
                warning.Descriptor.Id.Should().Be(Warnings.Wrn997_OneOrMoreFilesSkippedDueToExceedingSizeLimits);

                logger.ConfigurationNotifications.Where(n => n.Level == FailureLevel.Error).Count().Should().Be(1);
                Notification error = logger.ConfigurationNotifications.Where(n => n.Level == FailureLevel.Error).First();
                error.Descriptor.Id.Should().Be(Errors.ERR997_NoValidAnalysisTargets);
            }
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_OptionsSettingsOverrideContextSettings()
        {
            // For every configuration knob, we choose an explicit, 
            // non-default value that differs between options and configuration.
            // This allows us to be sure that the explicit options setting
            // is honored.

            ResultKind optionsKind = ResultKind.Review;
            ResultKind contextKind = ResultKind.Open;

            FailureLevel optionsLevel = FailureLevel.Note;
            FailureLevel contextLevel = FailureLevel.Error;

            var options = new TestAnalyzeOptions
            {
                Kind = new[] { optionsKind, ResultKind.Fail },
                Level = new[] { optionsLevel },
                TimeoutInSeconds = 60,
            };

            var context = new TestAnalysisContext()
            {
                ResultKinds = new ResultKindSet(new[] { contextKind, ResultKind.Fail }),
                FailureLevels = new FailureLevelSet(new[] { contextLevel }),
            };

            var multithreadedAnalyzeCommand = new TestMultithreadedAnalyzeCommand();
            multithreadedAnalyzeCommand.InitializeGlobalContextFromOptions(options, ref context);

            context.ResultKinds.Should().BeEquivalentTo(new ResultKindSet(new[] { optionsKind, ResultKind.Fail }));
            context.FailureLevels.Should().BeEquivalentTo(new FailureLevelSet(new[] { optionsLevel }));
            context.TimeoutInMilliseconds.Should().Be(60000);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_EmptyOptionsSettingsDoNotOverrideContextSettings()
        {
            var options = new TestAnalyzeOptions
            {
            };

            FailureLevel contextLevel = FailureLevel.Error;
            var failureLevels = new FailureLevelSet(new[] { contextLevel });

            var context = new TestAnalysisContext()
            {
                FailureLevels = failureLevels
            };

            var multithreadedAnalyzeCommand = new TestMultithreadedAnalyzeCommand();
            multithreadedAnalyzeCommand.InitializeGlobalContextFromOptions(options, ref context);

            context.FailureLevels.Should().BeEquivalentTo(failureLevels);
            context.TimeoutInMilliseconds.Should().Be(int.MaxValue);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_PerTargetAnalyzeEventsAreReceived()
        {
            int filesCount = Directory.GetFiles(Environment.CurrentDirectory, "*").Length;

            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new List<string>(new[] { "*" })
            };

            var testRule = new TestRule();
            var logger = new TestMessageLogger();

            var context = new TestAnalysisContext
            {
                Logger = logger,
                MaxFileSizeInKilobytes = long.MaxValue,
            };

            var multithreadedAnalyzeCommand = new TestMultithreadedAnalyzeCommand();
            int result = multithreadedAnalyzeCommand.Run(options, ref context);

            logger.AnalyzingTargetCount.Should().Be(filesCount);
            logger.TargetAnalyzedCount.Should().Be(filesCount);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_RootContextIsDisposed()
        {
            var options = new TestAnalyzeOptions();

            TestAnalysisContext context = null;
            var multithreadedAnalyzeCommand = new TestMultithreadedAnalyzeCommand();
            int result = multithreadedAnalyzeCommand.Run(options, ref context);
            context.Disposed.Should().BeTrue();
        }

        private void ExceptionTestHelper(
            RuntimeConditions runtimeConditions,
            ExitReason expectedExitReason = ExitReason.None,
            TestAnalyzeOptions analyzeOptions = null)
        {
            analyzeOptions ??= new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = Array.Empty<string>(),
            };

            analyzeOptions.RichReturnCode = false;
            ExceptionTestHelperImplementation(
                runtimeConditions,
                expectedExitReason,
                analyzeOptions);

            analyzeOptions.RichReturnCode = true;
            ExceptionTestHelperImplementation(
                runtimeConditions,
                expectedExitReason,
                analyzeOptions);
        }

        private void ExceptionTestHelperImplementation(RuntimeConditions runtimeConditions,
                                                       ExitReason expectedExitReason,
                                                       TestAnalyzeOptions analyzeOptions)
        {
            TestRuleBehaviors? behaviors = analyzeOptions.TestRuleBehaviors;
            TestRule.s_testRuleBehaviors = behaviors != null ? behaviors.Value : TestRule.s_testRuleBehaviors;
            Assembly[] plugInAssemblies;

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

            var command = new TestMultithreadedAnalyzeCommand(FileSystem.Instance);

            command.DefaultPluginAssemblies = plugInAssemblies;

            TestAnalysisContext context = null;
            int result = command.Run(analyzeOptions, ref context);

            int expectedResult =
                (runtimeConditions & ~RuntimeConditions.Nonfatal) == RuntimeConditions.None ?
                    TestMultithreadedAnalyzeCommand.SUCCESS : TestMultithreadedAnalyzeCommand.FAILURE;

            context.RuntimeErrors.Should().Be(runtimeConditions);
            if (analyzeOptions.RichReturnCode == true)
            {
                result.Should().Be((int)runtimeConditions);
            }
            else
            {
                result.Should().Be(expectedResult);
            }

            if (expectedExitReason != ExitReason.None)
            {
                context.RuntimeExceptions.Should().NotBeEmpty();

                if (expectedExitReason != ExitReason.UnhandledExceptionInEngine)
                {
                    var eax = context.RuntimeExceptions[0] as ExitApplicationException<ExitReason>;
                    eax.Should().NotBeNull();
                    eax.ExitReason.Should().Be(expectedExitReason);
                }
            }
            else
            {
                context.RuntimeExceptions.Should().BeNull();
            }
            TestRule.s_testRuleBehaviors = TestRuleBehaviors.None;
        }

        [Fact]
        public void InvalidCommandLineOption()
        {
            var options = new TestAnalyzeOptions
            {
                TestRuleBehaviors =
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
                expectedExitReason: ExitReason.NoValidAnalysisTargets,
                analyzeOptions: options
                );
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
                expectedExitReason: ExitReason.UnhandledExceptionInEngine,
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
            var uri = new Uri(this.GetType().Assembly.Location);

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
                expectedExitReason: ExitReason.UnhandledExceptionInEngine,
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
            var options = new TestAnalyzeOptions()
            {
                TargetFileSpecifiers = new string[] { GetThisTestAssemblyFilePath() },
            };

            TestMultithreadedAnalyzeCommand.RaiseUnhandledExceptionInDriverCode = true;

            ExceptionTestHelper(
                RuntimeConditions.ExceptionInEngine,
                ExitReason.UnhandledExceptionInEngine,
                analyzeOptions: options);

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
                        OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                    };

                    ExceptionTestHelper(
                        RuntimeConditions.ExceptionCreatingOutputFile,
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
                    OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                };

                ExceptionTestHelper(
                    RuntimeConditions.ExceptionCreatingOutputFile,
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
                    analyzeOptions: options);
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
                BaselineFilePath = baselineFilePath
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
                    BaselineFilePath = path
                };

                ExceptionTestHelper(
                    RuntimeConditions.InvalidCommandLineOption,
                    expectedExitReason: ExitReason.InvalidCommandLineOption,
                    analyzeOptions: options);
            }
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_ReportsErrorOnInvalidInvocationPropertyName()
        {
            var options = new TestAnalyzeOptions()
            {
                InvocationPropertiesToLog = new string[] { "CommandLine", "NoSuchProperty" },
            };

            ExceptionTestHelper(
                RuntimeConditions.InvalidCommandLineOption,
                expectedExitReason: ExitReason.InvalidCommandLineOption,
                analyzeOptions: options);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_ReportsWarningOnUnsupportedPlatformForRule()
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
        public void MultithreadedAnalyzeCommandBase_ReportsWarningOnUnsupportedPlatformForRuleAndNoRulesLoaded()
        {
            string path = Path.GetTempFileName() + ".xml";
            PropertiesDictionary allRulesDisabledConfiguration = ExportConfigurationCommandBaseTests.s_allRulesDisabledConfiguration;

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

        public Run AnalyzeFile(string fileName,
                               TestRuleBehaviors? behaviors = null,
                               string configFileName = null,
                               RuntimeConditions runtimeConditions = RuntimeConditions.None,
                               int expectedReturnCode = TestMultithreadedAnalyzeCommand.SUCCESS,
                               string postUri = null,
                               HttpClientWrapper httpClientWrapper = null)
        {
            string path = Path.GetTempFileName();
            Run run = null;

            try
            {
                var options = new TestAnalyzeOptions
                {
                    TargetFileSpecifiers = new string[] { fileName },
                    Quiet = true,
                    ConfigurationFilePath = configFileName ?? TestMultithreadedAnalyzeCommand.DefaultPolicyName,
                    Recurse = true,
                    OutputFilePath = path,
                    SarifOutputVersion = SarifVersion.Current,
                    OutputFileOptions = new[] { FilePersistenceOptions.ForceOverwrite },
                    TestRuleBehaviors = behaviors,
                    PostUri = postUri,
                };

                var command = new TestMultithreadedAnalyzeCommand(FileSystem.Instance, httpClientWrapper);
                command.DefaultPluginAssemblies = new Assembly[] { this.GetType().Assembly };

                TestAnalysisContext context = null;
                int result = command.Run(options, ref context);

                context.RuntimeErrors.Should().Be(runtimeConditions);
                result.Should().Be(expectedReturnCode);

                SarifLog log = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(path));
                Assert.NotNull(log);
                Assert.Single<Run>(log.Runs);

                run = log.Runs.First();
            }
            finally
            {
                File.Delete(path);
            }

            return run;
        }

        [Fact]
        public void AnalyzeCommand_EncodedPaths()
        {
            var logger = new MemoryStreamSarifLogger(dataToInsert: OptionallyEmittedData.Hashes);
            var command = new TestMultithreadedAnalyzeCommand();
            string path = @"C:\new folder\repro%2DCopy2.txt";
            var uri = new Uri(path, UriKind.Absolute);

            string content = "foo foo";
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.IsSymbolicLink(path)).Returns(false);
            mockFileSystem.Setup(x => x.FileStreamLength(path)).Returns(content.Length);
            mockFileSystem.Setup(x => x.FileInfoLength(path)).Returns(content.Length);
            mockFileSystem.Setup(x => x.FileReadAllText(path)).Returns(content);
            mockFileSystem.Setup(x => x.FileOpenRead(path)).Returns(new MemoryStream(Encoding.UTF8.GetBytes(content)));

            var target = new EnumeratedArtifact(mockFileSystem.Object)
            {
                Uri = uri
            };

            var options = new TestAnalyzeOptions
            {
                PluginFilePaths = new[] { typeof(TestRule).Assembly.FullName },
            };

            var properties = new PropertiesDictionary();
            properties.SetProperty(TestRule.Behaviors, TestRuleBehaviors.LogError);

            var context = new TestAnalysisContext
            {
                TargetsProvider = new ArtifactProvider(new[] { target }),
                DataToInsert = OptionallyEmittedData.Hashes,
                Policy = properties,
                Logger = logger,
            };

            int result = command.Run(options: null, ref context);
            context.ValidateCommandExecution(result);

            var sarifLog = logger.ToSarifLog();
            sarifLog.Runs[0].Should().NotBeNull();
            sarifLog.Runs[0].Results[0].Should().NotBeNull();
            sarifLog.Runs[0].Results.Count.Should().Be(1);
        }

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void AnalyzeCommand_TracesInMemory()
        {
            var testOutput = new StringBuilder();

            var sarifOutput = new StringBuilder();

            foreach (DefaultTraces trace in new[] { DefaultTraces.None, DefaultTraces.ScanTime, DefaultTraces.RuleScanTime, DefaultTraces.PeakWorkingSet })
            {
                foreach (Uri uri in new[] { new Uri(@"c:\doesnotexist.txt"), new Uri(@"doesnotexist.txt", UriKind.Relative) })
                {
                    var command = new TestMultithreadedAnalyzeCommand();

                    var options = new TestAnalyzeOptions
                    {
                        Trace = new[] { trace.ToString() },
                    };

                    sarifOutput.Clear();
                    using var writer = new StringWriter(sarifOutput);

                    var logger = new SarifLogger(writer,
                                                 run: new Run { Tool = command.Tool },
                                                 levels: BaseLogger.ErrorWarningNote,
                                                 kinds: BaseLogger.Fail);

                    var target = new EnumeratedArtifact(FileSystem.Instance) { Uri = uri, Contents = "A" };

                    var context = new TestAnalysisContext
                    {
                        TargetsProvider = new ArtifactProvider(new[] { target }),
                        FailureLevels = BaseLogger.ErrorWarningNote,
                        ResultKinds = BaseLogger.Fail,
                        Logger = logger,
                    };

                    int result = command.Run(options, ref context);
                    context.ValidateCommandExecution(result);

                    SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(sarifOutput.ToString());

                    int validTargetsCount = 1;
                    Validate(sarifLog.Runs?[0], trace, validTargetsCount, testOutput);
                }

                testOutput.Length.Should().Be(0, $"test cases failed : {Environment.NewLine}{testOutput}");
            }
        }

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void AnalyzeCommand_Traces()
        {
            var sb = new StringBuilder();

            foreach (DefaultTraces trace in new[] { DefaultTraces.None, DefaultTraces.ScanTime, DefaultTraces.RuleScanTime, DefaultTraces.PeakWorkingSet })
            {
                var options = new TestAnalyzeOptions
                {
                    OutputFilePath = Guid.NewGuid().ToString(),
                    TargetFileSpecifiers = new string[] { Guid.NewGuid().ToString() },
                    Trace = new[] { trace.ToString() },
                    Level = new[] { FailureLevel.Warning, FailureLevel.Note },
                };

                Run run = RunMultithreadedAnalyzeCommand(ComprehensiveKindAndLevelsByFilePath,
                                                         generateDuplicateScanTargets: false,
                                                         expectedResultCode: SUCCESS,
                                                         expectedResultCount: WARNING_COUNT + NOTE_COUNT,
                                                         options);

                int validTargetsCount = ALL_COUNT - NOT_APPLICABLE_COUNT;
                Validate(run, trace, validTargetsCount, sb);
            }

            sb.Length.Should().Be(0, $"test cases failed : {Environment.NewLine}{sb}");
        }

        internal static void Validate(Run run, DefaultTraces trace, int validTargetsCount, StringBuilder sb)
        {
            run.Should().NotBeNull();

            IList<Notification> executionNotifications = run.Invocations[0].ToolExecutionNotifications;
            IList<Notification> configurationNotifications = run.Invocations[0].ToolConfigurationNotifications;

            switch (trace)
            {
                case DefaultTraces.None:
                {
                    if (executionNotifications?.Count > 0 || configurationNotifications?.Count > 0)
                    {
                        sb.AppendLine($"\t{trace} : observed notifications when tracing was disabled.");
                    }
                    break;
                }
                case DefaultTraces.ScanTime:
                {
                    // There is only one end-to-end scan time notification.
                    if (executionNotifications?.Count != 1)
                    {
                        sb.AppendLine($"\t{trace} : expected 1 notification but saw {executionNotifications?.Count ?? 0}.");
                        return;
                    }

                    if (executionNotifications?.Where(t => t.Message.Text.Contains("elapsed")).Count() != 1)
                    {
                        sb.AppendLine($"\t{trace} : did not observe term 'elapsed' in scan timing notifications.");
                    }
                    break;
                }
                case DefaultTraces.RuleScanTime:
                {
                    // We expect every rule to generate timing data for every applicable scan target.
                    int rulesCount = run.Tool.Driver.Rules.Count;
                    int expectedNotificationsCount = rulesCount * validTargetsCount;

                    // We expected timing data for every rule.
                    if (executionNotifications.Count != expectedNotificationsCount)
                    {
                        sb.AppendLine($"\t{trace} : expected {expectedNotificationsCount} notifications but saw {executionNotifications.Count}.");
                        return;
                    }

                    if (executionNotifications?.Where(t => t.Message.Text.Contains("elapsed")).Count() != expectedNotificationsCount)
                    {
                        sb.AppendLine($"\t{trace} : did not observe term 'elapsed' in rule timing notifications.");
                    }

                    if (executionNotifications?.GroupBy(t => t.AssociatedRule.Id).Count() != rulesCount)
                    {
                        sb.AppendLine($"\t{trace} : did not observe timing notifications for every rule.");
                    }

                    break;
                }
            }
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_DefaultEndToEndAnalysis()
        {
            string location = GetThisTestAssemblyFilePath();
            Run run = AnalyzeFile(location, TestRuleBehaviors.LogError);

            run.Invocations?[0].ToolExecutionNotifications.Should().BeNull();
            run.Invocations?[0].ToolConfigurationNotifications.Should().BeNull();

            // As configured by injected TestRuleBehaviors, we should
            // see an error per scan target (one file in this case).
            run.Results?.Count.Should().Be((int)TestRule.ErrorsCount.DefaultValue());
            run.Results[0].Kind.Should().Be(ResultKind.Fail);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_EndToEndAnalysisWithPostUri()
        {
            PostUriTestHelper(@"https://example.com", TestMultithreadedAnalyzeCommand.SUCCESS, RuntimeConditions.None);
            PostUriTestHelper(@"https://httpbin.org/get", TestMultithreadedAnalyzeCommand.FAILURE, RuntimeConditions.ExceptionPostingLogFile);
            PostUriTestHelper(@"https://host.does.not.exist", TestMultithreadedAnalyzeCommand.FAILURE, RuntimeConditions.ExceptionPostingLogFile);
        }

        [Fact]
        public void MultithreadedMultithreadedAnalyzeCommandBase_EndToEndMultithreadedAnalysis()
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
            mockFileSystem.Setup(x => x.FileInfoLength(It.IsAny<string>())).Returns(2048);
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

                var context = new TestAnalysisContext { FileSystem = mockFileSystem.Object };
                int result = command.Run(options, ref context);

                context.RuntimeExceptions?[0].InnerException.Should().BeNull();

                result.Should().Be(CommandBase.SUCCESS, $"Iteration: {i}, Seed: {TestRule.s_seed}");
            }
        }

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void MultithreadedMultithreadedAnalyzeCommandBase_TargetFileSizeTestCases()
        {
            var sb = new StringBuilder();

            dynamic[] testCases = new[]
            {
                new {
                    expectedExitReason = ExitReason.NoValidAnalysisTargets,
                    fileSizeInBytes = (long)1023,
                    maxFileSizeInKB = (long)0,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.NoValidAnalysisTargets,
                    fileSizeInBytes =(long)0,
                    maxFileSizeInKB = (long)0,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSizeInBytes = (long)ulong.MinValue + 1,
                    maxFileSizeInKB = (long)1,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSizeInBytes = (long)ulong.MinValue + 1,
                    maxFileSizeInKB = (long)2000,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.NoValidAnalysisTargets,
                    fileSizeInBytes = (long)ulong.MinValue,
                    maxFileSizeInKB = (long)1000,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.NoValidAnalysisTargets,
                    fileSizeInBytes = (long)ulong.MinValue,
                    maxFileSizeInKB = long.MaxValue,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSizeInBytes = (long)ulong.MinValue,
                    maxFileSizeInKB = long.MaxValue,
                    isSymbolicLink = true,
                    actualFileContent = "ABC123"
                },
                new {
                    expectedExitReason = ExitReason.NoValidAnalysisTargets,
                    fileSizeInBytes = (long)ulong.MinValue,
                    maxFileSizeInKB = long.MaxValue,
                    isSymbolicLink = true,
                    actualFileContent = ""
                },
                new {
                    expectedExitReason = ExitReason.NoValidAnalysisTargets,
                    fileSizeInBytes = (long)(1024 * 2),
                    maxFileSizeInKB = (long)1,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSizeInBytes = (long)(1024 * 2),
                    maxFileSizeInKB = (long)3,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSizeInBytes = (long)20000,
                    maxFileSizeInKB = long.MaxValue,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSizeInBytes = (long)1024,
                    maxFileSizeInKB = (long)1,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.NoValidAnalysisTargets,
                    fileSizeInBytes = long.MaxValue,
                    maxFileSizeInKB = (long)0,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
                },
                new {
                    expectedExitReason = ExitReason.None,
                    fileSizeInBytes =long.MaxValue - 1,
                    maxFileSizeInKB = long.MaxValue,
                    isSymbolicLink = false,
                    actualFileContent = (string)null
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

                if (testCase.actualFileContent != null)
                {
                    byte[] dataBytes = Encoding.UTF8.GetBytes(testCase.actualFileContent);
                    mockStream.Setup(m => m.Length).Returns(dataBytes.Length);
                    mockFileSystem.Setup(x => x.FileStreamLength(It.IsAny<string>())).Returns(dataBytes.Length);
                    int invocationCount = 0;

                    mockStream.Setup(stream => stream.Seek(0, SeekOrigin.Begin))
                        .Callback(() =>
                        {
                            invocationCount = 0;
                        });

                    mockStream.Setup(stream => stream.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                             .Returns((byte[] buffer, int offset, int count) =>
                             {
                                 if (invocationCount < dataBytes.Length)
                                 {
                                     Array.Copy(dataBytes, invocationCount, buffer, offset, Math.Min(count, dataBytes.Length - invocationCount));
                                     int bytesRead = Math.Min(count, dataBytes.Length - invocationCount);
                                     invocationCount += bytesRead;
                                     return bytesRead;
                                 }
                                 else
                                 {
                                     invocationCount = 0;
                                     return 0;
                                 }
                             });
                }

                mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
                mockFileSystem.Setup(x => x.DirectoryGetFiles(It.IsAny<string>(), specifier)).Returns(files);
                mockFileSystem.Setup(x => x.FileExists(It.Is<string>(s => s.EndsWith(specifier)))).Returns(true);
                mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(),
                                                                    It.IsAny<string>(),
                                                                    It.IsAny<SearchOption>())).Returns(files);
                mockFileSystem.Setup(x => x.FileOpenRead(It.IsAny<string>())).Returns(mockStream.Object);
                mockFileSystem.Setup(x => x.FileExists(tempFile.Name)).Returns(true);
                mockFileSystem.Setup(x => x.FileInfoLength(It.IsAny<string>())).Returns((long)testCase.fileSizeInBytes);
                mockFileSystem.Setup(x => x.IsSymbolicLink(It.IsAny<string>())).Returns((bool)testCase.isSymbolicLink);

                bool expectedToBeWithinLimits =
                    testCase.fileSizeInBytes != 0 &&
                    (testCase.maxFileSizeInKB == -1 ||
                     (testCase.fileSizeInBytes + 1023) / 1024 < testCase.maxFileSizeInKB);

                var options = new TestAnalyzeOptions
                {
                    TargetFileSpecifiers = new[] { specifier },
                    SarifOutputVersion = SarifVersion.Current,
                    TestRuleBehaviors = TestRuleBehaviors.LogError,
                    ConfigurationFilePath = tempFile.Name,
                    MaxFileSizeInKilobytes = testCase.maxFileSizeInKB
                };

                int expectedReturnCode = testCase.expectedExitReason == ExitReason.None ? 0 : 1;

                RunAnalyzeCommand(
                    options: options,
                    expectedReturnCode: expectedReturnCode,
                    fileSystem: mockFileSystem.Object,
                    exitReason: testCase.expectedExitReason);
            }
        }

        [Fact]
        public void MultithreadedMultithreadedAnalyzeCommandBase_ErrorWhenHashing()
        {
            string specifier = "*.xyz";
            var files = new List<string>
            {
                Path.GetFullPath($@".{Path.DirectorySeparatorChar}File1.txt")
            };

            var mockStream = new Mock<Stream>();
            mockStream.Setup(m => m.CanRead).Returns(true);
            mockStream.Setup(m => m.CanSeek).Returns(true);
            mockStream.Setup(m => m.ReadByte()).Returns('a');
            mockStream.Setup(m => m.Seek(It.IsAny<long>(), It.IsAny<SeekOrigin>())).Throws(new IOException());

            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(x => x.FileInfoLength(It.IsAny<string>())).Returns(2048);
            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryGetFiles(It.IsAny<string>(), specifier)).Returns(files);
            mockFileSystem.Setup(x => x.FileExists(It.Is<string>(s => s.EndsWith(specifier)))).Returns(true);
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(),
                                                                It.IsAny<string>(),
                                                                It.IsAny<SearchOption>())).Returns(files);
            mockFileSystem.Setup(x => x.FileOpenRead(It.IsAny<string>())).Returns(mockStream.Object);

            var options = new TestAnalyzeOptions
            {
                TargetFileSpecifiers = new[] { specifier },
                TestRuleBehaviors = TestRuleBehaviors.LogError,
                DataToInsert = new[] { OptionallyEmittedData.Hashes },
            };

            RunAnalyzeCommand(
                options: options,
                expectedReturnCode: 0,
                fileSystem: mockFileSystem.Object,
                exitReason: ExitReason.None);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_RunDefaultRules()
        {
            string location = GetThisTestAssemblyFilePath();

            Run run = AnalyzeFile(location, TestRuleBehaviors.LogError);

            run.Invocations?[0].ToolExecutionNotifications.Should().BeNull();
            run.Invocations?[0].ToolConfigurationNotifications.Should().BeNull();

            // As configured by the inject TestRuleBehaviors value, we should see
            // an error for every scan target (of which there is one file in this test).
            run.Results.Count.Should().Be((int)TestRule.ErrorsCount.DefaultValue());
            run.Results[0].Level.Should().Be(FailureLevel.Error);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_FireAllRules()
        {
            PropertiesDictionary configuration = ExportConfigurationCommandBaseTests.s_defaultConfiguration;
            string path = Path.GetTempFileName() + ".xml";

            configuration.SetProperty(TestRule.Behaviors, TestRuleBehaviors.LogError);

            try
            {
                configuration.SaveToXml(path);

                string location = GetThisTestAssemblyFilePath();

                Run run = AnalyzeFile(location, configFileName: path);

                run.Invocations?[0].ToolExecutionNotifications.Should().BeNull();
                run.Invocations?[0].ToolConfigurationNotifications.Should().BeNull();

                // As configured by injected TestRuleBehaviors, we should
                // see an error per scan target (one file in this case).
                run.Results?.Count.Should().Be((int)TestRule.ErrorsCount.DefaultValue());
                run.Results.Count((result) => result.Level == FailureLevel.Error).Should().Be((int)TestRule.ErrorsCount.DefaultValue());
            }
            finally
            {
                if (File.Exists(path)) { File.Delete(path); }
            }
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_EndToEndAnalysisWithExplicitlyDisabledRules()
        {
            string path = Path.GetTempFileName() + ".xml";
            PropertiesDictionary allRulesDisabledConfiguration = ExportConfigurationCommandBaseTests.s_allRulesDisabledConfiguration;

            try
            {
                allRulesDisabledConfiguration.SaveToXml(path);

                string location = GetThisTestAssemblyFilePath();

                Run run = AnalyzeFile(
                    location,
                    configFileName: path,
                    runtimeConditions: RuntimeConditions.RuleWasExplicitlyDisabled | RuntimeConditions.NoRulesLoaded,
                    expectedReturnCode: FAILURE);

                run.Invocations.Should().NotBeNull();
                run.Invocations.Count.Should().Be(1);

                // We raised a notification error, which means the invocation failed.
                run.Invocations[0].ExecutionSuccessful.Should().Be(false);

                // When rules are disabled, we expect a configuration warning for each
                // disabled check that documents it was turned off for the analysis.
                run.Results.Count.Should().Be(0);

                IList<Notification> toolExecutionNotifications = run.Invocations?[0].ToolExecutionNotifications;
                toolExecutionNotifications.Should().BeNull();

                IList<Notification> toolConfigurationNotifications = run.Invocations?[0].ToolConfigurationNotifications;
                toolConfigurationNotifications.Should().NotBeNull();

                // Three notifications. One for each disabled rule, i.e. SimpleTestRule
                // and SimpleTestRule + an error notification that all rules have been disabled
                toolConfigurationNotifications.Count.Should().Be(3);


                // Error: all rules were disabled
                toolConfigurationNotifications.Count((notification) => notification.Level == FailureLevel.Error).Should().Be(1);
                toolConfigurationNotifications.Count((notification) => notification.Descriptor.Id == Errors.ERR997_AllRulesExplicitlyDisabled).Should().Be(1);

                // Warnings: one per disabled rule.
                toolConfigurationNotifications.Count((notification) => notification.Level == FailureLevel.Warning).Should().Be(2);
                toolConfigurationNotifications.Where((notification) => notification.Descriptor.Id == Warnings.Wrn999_RuleExplicitlyDisabled).Count().Should().Be(2);
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
        public void MultithreadedAnalyzeCommandBase_LoadConfigurationFile(string configValue, bool defaultFileExists, string expectedFileName)
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

            var command = new TestMultithreadedAnalyzeCommand(mockFileSystem.Object);
            var context = new TestAnalysisContext { FileSystem = mockFileSystem.Object };
            context.ConfigurationFilePath = command.GetConfigurationFileName(configValue, context.FileSystem);

            if (string.IsNullOrEmpty(expectedFileName))
            {
                context.ConfigurationFilePath.Should().Be(null);
            }
            else
            {
                context.ConfigurationFilePath.Should().EndWith(expectedFileName);
            }
        }

        private static string GetThisTestAssemblyFilePath()
        {
            string filePath = typeof(MultithreadedAnalyzeCommandBaseTests).Assembly.Location;
            return filePath;
        }

        private static string GetSampleFileToTest()
        {
            string filePath = typeof(MultithreadedAnalyzeCommandBaseTests).Assembly.Location;
            filePath = Path.GetDirectoryName(filePath);
            filePath = Path.Combine(filePath, "SampleTestFile.txt");

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, $"{Guid.NewGuid()}");
            }

            return filePath;
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_UpdateLocationsAndMessageWithCurrentUri()
        {
            var uri = new Uri(@"c:\directory\test.txt", UriKind.RelativeOrAbsolute);
            Notification actualNotification = BuildTestNotification(uri);

            var updatedUri = new Uri(@"c:\updated\directory\newFileName.txt", UriKind.RelativeOrAbsolute);
            Notification expectedNotification = BuildTestNotification(updatedUri);

            MultithreadedAnalyzeCommandBase<TestAnalysisContext, AnalyzeOptionsBase>
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
        public void MultithreadedAnalyzeCommandBase_GetFileNameFromUriWorks()
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

                string actualFileName = MultithreadedAnalyzeCommandBase<TestAnalysisContext, AnalyzeOptionsBase>.GetFileNameFromUri(uri);

                if (!Equals(actualFileName, expectedFileName))
                {
                    sb.AppendFormat("Incorrect file name returned for uri '{0}'. Expected '{1}' but saw '{2}'.", uri, expectedFileName, actualFileName).AppendLine();
                }
            }

            sb.Length.Should().Be(0, because: "all URI to file name conversions should succeed but the following cases failed." + Environment.NewLine + sb.ToString());
        }

        #region ResultsCachingTestsAndHelpers

        [Fact]
        public void MultithreadedAnalyzeCommandBase_CachesErrors()
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
            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_CachesNotes()
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
        public void MultithreadedAnalyzeCommandBase_CachesNotificationsWithoutPersistingToLogFile()
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
            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_CachesNotificationsWhenPersistingToLogFile()
        {
            var testCase = new ResultsCachingTestCase
            {
                Files = ComprehensiveKindAndLevelsByFilePath,
                PersistLogFileToDisk = true,
                TestRuleBehaviors = TestRuleBehaviors.RaiseTargetParseError,
                ExpectedReturnCode = FAILURE,
            };

            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_CachesResultsWithoutPersistingToLogFile()
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
            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_CachesResultsWhenPersistingToLogFile()
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
            RunResultsCachingTestCase(testCase);

            testCase.Verbose = true;
            RunResultsCachingTestCase(testCase);
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_AutomationDetailsTests()
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
                    AutomationGuid = default
                },
                new TestAnalyzeOptions
                {
                    AutomationId = string.Empty,
                    AutomationGuid = automationGuid
                },
                new TestAnalyzeOptions
                {
                    AutomationId = null,
                    AutomationGuid = default
                },
                new TestAnalyzeOptions
                {
                    AutomationId = whiteSpace,
                    AutomationGuid = Guid.Empty
                },
                new TestAnalyzeOptions
                {
                    AutomationId = string.Empty,
                    AutomationGuid = default
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
                RunResultsCachingTestCase(testCase, enhancedOptions: enhancedOption);
            }
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_ShouldOnlyLogArtifactsWhenResultsAreFound()
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

            Run run = RunAnalyzeCommand(options, resultsCachingTestCase);

            // Hashes is enabled and we should expect to see two artifacts because we have:
            // one result with Error level and one result with Warning level.
            run.Artifacts.Should().HaveCount(expectedNumberOfArtifacts);
            run.Results.Count(r => r.Level == FailureLevel.Error).Should().Be(expectedNumberOfResultsWithErrors);
            run.Results.Count(r => r.Level == FailureLevel.Warning).Should().Be(expectedNumberOfResultsWithWarnings);

        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_ShouldNotThrowException_WhenAnalyzingSameFileBasedOnTwoTargetFileSpecifiers()
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

                    TestRule.s_testRuleBehaviors = resultsCachingTestCase.TestRuleBehaviors;
                    RunAnalyzeCommand(options,
                                      resultsCachingTestCase.FileSystem,
                                      resultsCachingTestCase.ExpectedReturnCode);
                }
            };

            action.Should().NotThrow();
        }

        [Fact]
        public void MultithreadedAnalyzeCommandBase_MultithreadedShouldUseCacheIfFilesAreTheSame()
        {
            // This test disabled until file caching is restored.
            /*
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
            */

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

        private const int OPEN_COUNT = 1;
        private const int ERROR_COUNT = 5;
        private const int NOTE_COUNT = 3;
        private const int PASS_COUNT = 4;
        private const int REVIEW_COUNT = 2;
        private const int WARNING_COUNT = 2;
        private const int INFORMATIONAL_COUNT = 1;
        private const int NOT_APPLICABLE_COUNT = 2;

        private const int ALL_COUNT =
            OPEN_COUNT + ERROR_COUNT + NOTE_COUNT + PASS_COUNT + REVIEW_COUNT +
            WARNING_COUNT + INFORMATIONAL_COUNT + NOT_APPLICABLE_COUNT;

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
            $"{rootDir}Informational.1.of.1.sys",
            $"{rootDir}Note.1.of.3.dll",
            $"{rootDir}Note.2.of.3.exe",
            $"{rootDir}Note.3.of.3.jar",
            $"{rootDir}NotApplicable.1.of.2.js",
            $"{rootDir}NotApplicable.2.of.2.exe",
            $"{rootDir}Open.1.of.1.cab",
            $"{rootDir}Pass.1.of.4.cs",
            $"{rootDir}Pass.2.of.4.cpp",
            $"{rootDir}Pass.3.of.4.exe",
            $"{rootDir}Pass.4.of.4.dll",
            $"{rootDir}Review.1.of.2.txt",
            $"{rootDir}Review.2.of.2.dll",
            $"{rootDir}Warning.1.of.2.java",
            $"{rootDir}Warning.2.of.2.cs",
        };

        private static void RunResultsCachingTestCase(ResultsCachingTestCase testCase,
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
                DataToInsert = new[] { OptionallyEmittedData.Hashes },
                OutputFilePath = testCase.PersistLogFileToDisk ? Guid.NewGuid().ToString() : null,
                TargetFileSpecifiers = new string[] { Guid.NewGuid().ToString() },
            };

            EnhanceOptions(options, enhancedOptions);

            if (testCase.Verbose)
            {
                options.Kind = new List<ResultKind> { ResultKind.Informational, ResultKind.Open, ResultKind.Review, ResultKind.Fail, ResultKind.Pass, ResultKind.NotApplicable, ResultKind.None };
                options.Level = new List<FailureLevel> { FailureLevel.Error, FailureLevel.Warning, FailureLevel.Note, FailureLevel.None };
            }

            Run runWithoutCaching = RunAnalyzeCommand(options, testCase);

            options.DataToInsert = new OptionallyEmittedData[] { OptionallyEmittedData.Hashes };
            Run runWithCaching = RunAnalyzeCommand(options, testCase);

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

                if (string.IsNullOrWhiteSpace(options.AutomationId) && options.AutomationGuid == default)
                {
                    runWithCaching.AutomationDetails.Should().Be(null);
                }

                if (!string.IsNullOrWhiteSpace(options.AutomationId))
                {
                    runWithCaching.AutomationDetails.Id.Should().Be(options.AutomationId);
                }

                if (options.AutomationGuid != default)
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
            current.AutomationGuid = enhancement == null ? default : enhancement.AutomationGuid;
        }

        private static IFileSystem CreateDefaultFileSystemForResultsCaching(IList<string> files, bool generateSameInput = false)
        {
            // This helper creates a file system that generates unique or entirely
            // duplicate content for every file passed in the 'files' argument.

            string logFileContents = Guid.NewGuid().ToString();

            var mockFileSystem = new Mock<IFileSystem>();

            mockFileSystem.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
            mockFileSystem.Setup(x => x.FileInfoLength(It.IsAny<string>())).Returns(2048);
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>())).Returns(new string[0]);
            mockFileSystem.Setup(x => x.DirectoryEnumerateFiles(It.IsAny<string>(), It.IsAny<string>(), SearchOption.TopDirectoryOnly)).Returns(files);
            mockFileSystem.Setup(x => x.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(files);

            for (int i = 0; i < files.Count; i++)
            {
                string fullyQualifiedName = Path.GetFileName(files[i]) == files[i]
                    ? Environment.CurrentDirectory + Path.DirectorySeparatorChar + files[i]
                    : files[i];
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullyQualifiedName);

                mockFileSystem.Setup(x => x.FileExists(It.Is<string>(f => f == fullyQualifiedName))).Returns(true);

                mockFileSystem.Setup(x => x.FileReadAllText(It.Is<string>(f => f == fullyQualifiedName))).Returns(logFileContents);

                mockFileSystem.Setup(x => x.FileOpenRead(It.Is<string>(f => f == fullyQualifiedName)))
                    .Returns(new NonDisposingDelegatingStream(new MemoryStream(Encoding.UTF8.GetBytes(generateSameInput ? logFileContents : fileNameWithoutExtension))));
            }
            return mockFileSystem.Object;
        }

        private static Run RunAnalyzeCommand(TestAnalyzeOptions options,
                                             ResultsCachingTestCase testCase)
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

        private static SarifLog RunAnalyzeCommand(TestAnalyzeOptions options,
                                                  IFileSystem fileSystem,
                                                  int expectedReturnCode,
                                                  ExitReason exitReason = ExitReason.None)
        {
            // If no log file is specified, we will convert the console output into a log file
            bool captureConsoleOutput = string.IsNullOrEmpty(options.OutputFilePath);

            var command = new TestMultithreadedAnalyzeCommand(fileSystem) { _captureConsoleOutput = captureConsoleOutput };
            command.DefaultPluginAssemblies = new Assembly[] { typeof(MultithreadedAnalyzeCommandBaseTests).Assembly };

            var context = new TestAnalysisContext { FileSystem = fileSystem };
            context.OpcFileExtensions = new StringSet();
            int result = command.Run(options, ref context);

            result.Should().Be(expectedReturnCode);

            if (exitReason != ExitReason.None)
            {
                var exception = context.RuntimeExceptions[0] as ExitApplicationException<ExitReason>;
                exception.Should().NotBeNull();
                exception.ExitReason.Should().Be(exitReason);
            }

            ConsoleLogger consoleLogger = (command as TestMultithreadedAnalyzeCommand)._consoleLogger;

            return captureConsoleOutput
                ? ConvertConsoleOutputToSarifLog(consoleLogger.CapturedOutput)
                : JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(options.OutputFilePath));
        }

        private static Run RunMultithreadedAnalyzeCommand(IList<string> files,
                                                           bool generateDuplicateScanTargets,
                                                           int expectedResultCode,
                                                           int expectedResultCount,
                                                           TestAnalyzeOptions options = null)
        {
            var testCase = new ResultsCachingTestCase
            {
                Files = files,
                PersistLogFileToDisk = true,
                FileSystem = CreateDefaultFileSystemForResultsCaching(files, generateDuplicateScanTargets)
            };

            options ??= new TestAnalyzeOptions
            {
                OutputFilePath = Guid.NewGuid().ToString(),
                TargetFileSpecifiers = new string[] { Guid.NewGuid().ToString() },
                Kind = new List<ResultKind> { ResultKind.Fail },
                Level = new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error },
                DataToInsert = new OptionallyEmittedData[] { OptionallyEmittedData.Hashes }
            };

            try
            {
                TestRule.s_testRuleBehaviors = testCase.TestRuleBehaviors;

                var command = new TestMultithreadedAnalyzeCommand(testCase.FileSystem)
                {
                    DefaultPluginAssemblies = new Assembly[] { typeof(MultithreadedAnalyzeCommandBaseTests).Assembly }
                };

                var context = new TestAnalysisContext { FileSystem = testCase.FileSystem };

                context.Policy.SetProperty(AnalyzeContextBase.OpcFileExtensionsProperty, new StringSet());
                context.Policy.SetProperty(AnalyzeContextBase.BinaryFileExtensionsProperty, new StringSet());

                int result = command.Run(options, ref context);

                if (expectedResultCode == CommandBase.SUCCESS)
                {
                    context.ValidateCommandExecution(result);
                }

                SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(File.ReadAllText(options.OutputFilePath));

                if (expectedResultCode == 0) { (context.RuntimeErrors & ~RuntimeConditions.Nonfatal).Should().Be(0); }
                result.Should().Be(expectedResultCode);
                sarifLog.Runs[0].Results.Count.Should().Be(expectedResultCount);

                if (options.InsertProperties?.Where(p => p == "Hashes").Any() == true)
                {
                    var hashes = new HashSet<string>();
                    foreach (Artifact artifact in sarifLog.Runs[0].Artifacts)
                    {
                        hashes.Add(artifact.Hashes["sha-256"]);
                    }

                    int expectedUniqueFileHashCount = generateDuplicateScanTargets ? 1 : expectedResultCount;
                    hashes.Count.Should().Be(expectedUniqueFileHashCount);
                }
                return sarifLog.Runs[0];
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

                // Currently, we enable the TestRule by default as well as FunctionlessTestRule.
                // FunctionlessTest rule never emits a result, exception for one case, it
                // honors the 'not applicable' designation to drop analysis for that scenario.
                RulesCount = 2;
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

            public int RulesCount;

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
                ? (Files.Where((f) => f.Contains("NotApplicable")).Count() * RulesCount)
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
        public void CheckIncompatibleRules_ExitAnalysis()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001" },
                new TestRule { Id = "TEST1002", IncompatibleRuleIds = new HashSet<string> { "TEST1003" } },
                new TestRule { Id = "TEST1003" },
            };

            var disabledSkimmers = new HashSet<string>();

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();

            this.RunCheckIncompatibleRulesTests(skimmers, disabledSkimmers, context, consoleLogger, true,
                ExitReason.IncompatibleRulesDetected, RuntimeConditions.OneOrMoreRulesAreIncompatible,
                Errors.ERR997_IncompatibleRulesDetected);
        }

        [Fact]
        public void CheckIncompatibleRules_NoIncompatibleRules()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001" },
                new TestRule { Id = "TEST1002" },
                new TestRule { Id = "TEST1003" },
            };

            var disabledSkimmers = new HashSet<string>();

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();

            this.RunCheckIncompatibleRulesTests(skimmers, disabledSkimmers, context, consoleLogger,
                false, ExitReason.None, RuntimeConditions.None, expectedErrorCode: null);
        }

        [Fact]
        public void CheckIncompatibleRules_IncompatibleRuleDoesNotExist()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001", IncompatibleRuleIds = new HashSet<string> { "NA9999" } },
                new TestRule { Id = "TEST1002" },
                new TestRule { Id = "TEST1003" },
            };

            var disabledSkimmers = new HashSet<string>();

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();

            this.RunCheckIncompatibleRulesTests(skimmers, disabledSkimmers, context, consoleLogger,
                false, ExitReason.None, RuntimeConditions.None, expectedErrorCode: null);
        }

        [Fact]
        public void CheckIncompatibleRules_RulesAlreadyDisabled()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001" },
                new TestRule { Id = "TEST1002", IncompatibleRuleIds = new HashSet<string> { "TEST1001" } },
                new TestRule { Id = "TEST1003" },
            };

            var disabledSkimmers = new HashSet<string>() { "TEST1002" };

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();

            this.RunCheckIncompatibleRulesTests(skimmers, disabledSkimmers, context, consoleLogger,
                false, ExitReason.None, RuntimeConditions.None, expectedErrorCode: null);
        }

        [Fact]
        public void CheckIncompatibleRules_MultipleIncompatibleRules()
        {
            TestRule[] skimmers = new[]
            {
                new TestRule { Id = "TEST1001" },
                new TestRule { Id = "TEST1002", IncompatibleRuleIds = new HashSet<string> { "TEST1003" } },
                new TestRule { Id = "TEST1003", IncompatibleRuleIds = new HashSet<string> { "TEST1001", "TEST1002" } },
            };

            var disabledSkimmers = new HashSet<string>();

            var consoleLogger = new ConsoleLogger(false, "TestTool") { CaptureOutput = true };
            var context = new TestAnalysisContext();

            this.RunCheckIncompatibleRulesTests(skimmers, disabledSkimmers, context, consoleLogger,
                true, ExitReason.IncompatibleRulesDetected, RuntimeConditions.OneOrMoreRulesAreIncompatible,
                Errors.ERR997_IncompatibleRulesDetected);
        }

        private void RunCheckIncompatibleRulesTests(IEnumerable<TestRule> skimmers, HashSet<string> disabledSkimmers,
            TestAnalysisContext context, ConsoleLogger consoleLogger, bool expectExpcetion, ExitReason expectedExitReason,
            RuntimeConditions expectedRuntimeConditions, string expectedErrorCode)
        {
            ITestAnalyzeCommand command = this.CreateTestCommand(context, consoleLogger);

            if (expectExpcetion)
            {
                ExitApplicationException<ExitReason> exception = Assert.Throws<ExitApplicationException<ExitReason>>(
                    () => command.CheckIncompatibleRules(skimmers, context, disabledSkimmers));

                exception.ExitReason.Should().Be(expectedExitReason);
            }

            context.RuntimeErrors.Should().Be(expectedRuntimeConditions);

            if (expectedErrorCode == null)
            {
                consoleLogger.CapturedOutput.Should().BeNull();
            }
            else
            {
                consoleLogger.CapturedOutput.Contains(expectedErrorCode);
            }
        }

        private ITestAnalyzeCommand CreateTestCommand(TestAnalysisContext context, ConsoleLogger consoleLogger)
        {
            ITestAnalyzeCommand command = new TestMultithreadedAnalyzeCommand(FileSystem.Instance);

            var logger = new AggregatingLogger();
            logger.Loggers.Add(consoleLogger);
            context.Logger = logger;

            return command;
        }

        private void PostUriTestHelper(string postUri, int expectedReturnCode, RuntimeConditions runtimeConditions)
        {
            var mockHttpClient = new Mock<HttpClientWrapper>();
            mockHttpClient.Setup(client => client
                .PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync((string uriString, HttpContent content) =>
                {
                    var uri = new Uri(uriString);

                    // health check request (with query parameter)
                    string query = uri.Query;
                    if (query.Contains("healthcheck=true"))
                    {
                        return new HttpResponseMessage(HttpStatusCode.Accepted);
                    }

                    // health check request (without content)
                    string contentString = content?.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(contentString))
                    {
                        return new HttpResponseMessage(HttpStatusCode.UnprocessableEntity);
                    }

                    // anything other than example.com should fail
                    if (!uri.Host.Equals("example.com", StringComparison.OrdinalIgnoreCase))
                    {
                        return new HttpResponseMessage(HttpStatusCode.NotFound);
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            string location = GetThisTestAssemblyFilePath();

            Run run = AnalyzeFile(
                location,
                TestRuleBehaviors.LogError,
                postUri: postUri,
                expectedReturnCode: expectedReturnCode,
                runtimeConditions: runtimeConditions,
                httpClientWrapper: mockHttpClient.Object);

            run.Invocations?[0].ToolExecutionNotifications.Should().BeNull();
            run.Invocations?[0].ToolConfigurationNotifications.Should().BeNull();

            // As configured by injected TestRuleBehaviors, we should
            // see an error per scan target (one file in this case).
            run.Results.Count.Should().Be((int)TestRule.ErrorsCount.DefaultValue());
            run.Results[0].Kind.Should().Be(ResultKind.Fail);
        }
    }
}
#pragma warning restore CS0618
