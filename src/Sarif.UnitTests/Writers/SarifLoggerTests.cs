// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifLoggerTests : JsonTests
    {
        private readonly ITestOutputHelper output;

        public SarifLoggerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void SarifLogger_RedactedCommandLine()
        {
            var sb = new StringBuilder();

            // On a developer's machine, the script BuildAndTest.cmd runs the tests with a particular command line. 
            // Under AppVeyor, the appveyor.yml file simply specifies the names of the test assemblies, and AppVeyor 
            // constructs and executes its own, different command line. So, based on our knowledge of each of those 
            // command lines, we select a different token to redact in each of those cases.
            //
            //
            // Sample test execution command-line from within VS. We will redact the 'TestExecution' role data
            //
            // "C:\PROGRAM FILES (X86)\MICROSOFT VISUAL STUDIO 14.0\COMMON7\IDE\COMMONEXTENSIONS\MICROSOFT\TESTWINDOW\te.processhost.managed.exe"
            // /role=TestExecution /wexcommunication_connectionid=2B1B7D58-C573-45E8-8968-ED321963F0F6
            // /stackframecount=50 /wexcommunication_protocol=ncalrpc
            //
            // Sample test execution from command-line when running test script. Will redact hostProcessId
            //
            // "C:\Program Files (x86\\Microsoft Visual Studio 14.0\Common7\IDE\QTAgent32_40.exe\" 
            // /agentKey a144e450-ac06-46d0-8365-c21ea7872d23 /hostProcessId 8024 /hostIpcPortName 
            // eqt -60284c64-6bc1-3ecc-fb5f-a484bb1a2475"
            // 
            // Sample test execution from Appveyor will redact 'Appveyor'
            //
            // pathToExe   = C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\Extensions
            // commandLine = vstest.console  /logger:Appveyor "C:\projects\sarif-sdk\bld\bin\Sarif.UnitTests\AnyCPU_Release\Sarif.UnitTests.dll"

            using (var textWriter = new StringWriter(sb))
            {
                string[] tokensToRedact = new string[] { };
                string pathToExe = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
                string commandLine = Environment.CommandLine;
                string lowerCaseCommandLine = commandLine.ToLower();

                if (lowerCaseCommandLine.Contains("testhost.dll") || lowerCaseCommandLine.Contains("\\xunit.console"))
                {
                    int index = commandLine.LastIndexOf("\\");
                    string argumentToRedact = commandLine.Substring(0, index + 1);
                    tokensToRedact = new string[] { argumentToRedact };
                }
                else if (pathToExe.IndexOf(@"\Extensions", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    string appVeyor = "Appveyor";
                    if (commandLine.IndexOf(appVeyor, StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        // For Appveyor builds, redact the string Appveyor.
                        tokensToRedact = new string[] { appVeyor };
                    }
                    else
                    {
                        // The calling assembly lives in an \Extensions directory that hangs off
                        // the directory of the test driver (the location of which we can't retrieve
                        // from Assembly.GetEntryAssembly() as we are running in an AppDomain).
                        pathToExe = pathToExe.Substring(0, pathToExe.Length - @"\Extensions".Length);
                        tokensToRedact = new string[] {  pathToExe };
                    }
                }
                else
                {
                    string argumentToRedact = commandLine.Split(new string[] { @"/agentKey" }, StringSplitOptions.None)[1].Trim();
                    argumentToRedact = argumentToRedact.Split(' ')[0];
                    tokensToRedact = new string[] { argumentToRedact };
                }

                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: null,
                    loggingOptions: LoggingOptions.None,
                    prereleaseInfo: null,
                    invocationTokensToRedact: tokensToRedact,
                    invocationPropertiesToLog: new List<string> { "CommandLine" })) { }

                string result = sb.ToString();
                result.Split(new string[] { SarifConstants.RemovedMarker }, StringSplitOptions.None)
                    .Length.Should().Be(tokensToRedact.Length + 1, "redacting n tokens gives you n+1 removal markers");
            }
        }



        [Theory]
        // These values are emitted both verbose and non-verbose
        [InlineData(ResultLevel.Error, true, true)]
        [InlineData(ResultLevel.Error, false, true)]
        [InlineData(ResultLevel.Warning, true, true)]
        [InlineData(ResultLevel.Warning, false, true)]
        [InlineData(ResultLevel.Default, true, true)]
        [InlineData(ResultLevel.Default, false, true)]

        // These result levels only emitted in verbose logging mode
        [InlineData(ResultLevel.NotApplicable, true, true)]
        [InlineData(ResultLevel.NotApplicable, false, false)]
        [InlineData(ResultLevel.Note, true, true)]
        [InlineData(ResultLevel.Note, false, false)]
        [InlineData(ResultLevel.Open, true, true)]
        [InlineData(ResultLevel.Open, false, false)]
        [InlineData(ResultLevel.Pass, true, true)]
        [InlineData(ResultLevel.Pass, false, false)]

        public void SarifLogger_ShouldLog(ResultLevel resultLevel, bool verboseLogging, bool expectedReturn)
        {
            LoggingOptions loggingOptions = verboseLogging ? LoggingOptions.Verbose : LoggingOptions.None;

            var sb = new StringBuilder();
            var logger = new SarifLogger(new StringWriter(sb), loggingOptions);
            bool result = logger.ShouldLog(resultLevel);
            result.Should().Be(expectedReturn);
        }

        [Fact]
        public void SarifLogger_ShouldLogRecognizesAllResultLevels()
        {
            LoggingOptions loggingOptions = LoggingOptions.Verbose;
            var sb = new StringBuilder();
            var logger = new SarifLogger(new StringWriter(sb), loggingOptions);

            foreach (object resultLevelObject in Enum.GetValues(typeof(ResultLevel)))
            {
                // The point of this test is that every defined enum value
                // should pass a call to ShouldLog and will not raise an 
                // exception because the enum value isn't recognized
                logger.ShouldLog((ResultLevel)resultLevelObject);
            }
        }


        [Fact]
        public void SarifLogger_WritesSarifLoggerVersion()
        {
            var sb = new StringBuilder();

            using (var textWriter = new StringWriter(sb))
            {
                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: new string[] { @"foo.cpp" },
                    loggingOptions: LoggingOptions.None,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: null))
                {
                    string ruleId = "RuleId";
                    var rule = new Rule() { Id = ruleId };

                    var result = new Result()
                    {
                        RuleId = ruleId
                    };

                    sarifLogger.Log(rule, result);
                }
            }

            string output = sb.ToString();
            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(output);

            string sarifLoggerLocation = typeof(SarifLogger).Assembly.Location;
            string expectedVersion = FileVersionInfo.GetVersionInfo(sarifLoggerLocation).FileVersion;

            sarifLog.Runs[0].Tool.SarifLoggerVersion.Should().Be(expectedVersion);
        }



        [Fact]
        public void SarifLogger_WritesRunProperties()
        {
            string propertyName = "numberValue";
            double propertyValue = 3.14;
            string logicalId = nameof(logicalId) + ":" + Guid.NewGuid().ToString();
            string baselineInstanceGuid = nameof(baselineInstanceGuid) + ":" + Guid.NewGuid().ToString();
            string automationLogicalId = nameof(automationLogicalId) + ":" + Guid.NewGuid().ToString();
            string architecture = nameof(architecture) + ":" + "x86";
            var conversion = new Conversion() { Tool = DefaultTool };
            var utcNow = DateTime.UtcNow;
            var versionControlUri = new Uri("https://www.github.com/contoso/contoso");
            var versionControlDetails = new VersionControlDetails() { Uri = versionControlUri, Timestamp = DateTime.UtcNow };
            string originalUriBaseIdKey = "testBase";
            Uri originalUriBaseIdValue = new Uri("https://sourceserver.contoso.com");
            var originalUriBaseIds = new Dictionary<string, Uri>() { { originalUriBaseIdKey, originalUriBaseIdValue } };
            string defaultFileEncoding = "UTF7";
            string richMessageMimeType = "sarif-markdown";
            string redactionToken = "[MY_REDACTION_TOKEN]";

            var sb = new StringBuilder();

            var run = new Run();

            using (var textWriter = new StringWriter(sb))
            {
                run.SetProperty(propertyName, propertyValue);

                run.LogicalId = logicalId;
                run.BaselineInstanceGuid = baselineInstanceGuid;
                run.AutomationLogicalId = automationLogicalId;
                run.Architecture = architecture;
                run.Conversion = conversion;
                run.VersionControlProvenance = new[] { versionControlDetails };
                run.OriginalUriBaseIds = originalUriBaseIds;
                run.DefaultFileEncoding = defaultFileEncoding;
                run.RichMessageMimeType = richMessageMimeType;
                run.RedactionToken = redactionToken;

                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    run: run,
                    invocationPropertiesToLog: null))
                {
                }
            }

            string output = sb.ToString();
            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(output);

            run = sarifLog.Runs[0];

            run.GetProperty<double>(propertyName).Should().Be(propertyValue);
            run.LogicalId.Should().Be(logicalId);
            run.BaselineInstanceGuid.Should().Be(baselineInstanceGuid);
            run.AutomationLogicalId.Should().Be(automationLogicalId);
            run.Architecture.Should().Be(architecture);
            run.Conversion.Tool.ShouldBeEquivalentTo(DefaultTool);
            //run.VersionControlProvenance[0].Timestamp.ShouldBeEquivalentTo(utcNow);
            run.VersionControlProvenance[0].Uri.ShouldBeEquivalentTo(versionControlUri);
            run.OriginalUriBaseIds[originalUriBaseIdKey].Should().Be(originalUriBaseIdValue);
            run.DefaultFileEncoding.Should().Be(defaultFileEncoding);
            run.RichMessageMimeType.Should().Be(richMessageMimeType);
            run.RedactionToken.Should().Be(redactionToken);
        }

        [Fact]
        public void SarifLogger_WritesFileData()
        {
            var sb = new StringBuilder();
            string file;

            using (var tempFile = new TempFile(".cpp"))
            using (var textWriter = new StringWriter(sb))
            {
                file = tempFile.Name;
                File.WriteAllText(file, "#include \"windows.h\";");

                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: new string[] { file },
                    dataToInsert: OptionallyEmittedData.Hashes,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: null))
                {
                    string ruleId = "RuleId";
                    var rule = new Rule() { Id = ruleId };

                    var result = new Result()
                    {
                        RuleId = ruleId
                    };

                    sarifLogger.Log(rule, result);
                }
            }

            string logText = sb.ToString();

            string fileDataKey = new Uri(file).AbsoluteUri;

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(logText);
            sarifLog.Runs[0].Files[fileDataKey].MimeType.Should().Be(MimeType.Cpp);
            sarifLog.Runs[0].Files[fileDataKey].Hashes[0].Algorithm.Should().Be("md5");
            sarifLog.Runs[0].Files[fileDataKey].Hashes[0].Value.Should().Be("4B9DC12934390387862CC4AB5E4A2159");
            sarifLog.Runs[0].Files[fileDataKey].Hashes[1].Algorithm.Should().Be("sha-1");
            sarifLog.Runs[0].Files[fileDataKey].Hashes[1].Value.Should().Be("9B59B1C1E3F5F7013B10F6C6B7436293685BAACE");
            sarifLog.Runs[0].Files[fileDataKey].Hashes[2].Algorithm.Should().Be("sha-256");
            sarifLog.Runs[0].Files[fileDataKey].Hashes[2].Value.Should().Be("0953D7B3ADA7FED683680D2107EE517A9DBEC2D0AF7594A91F058D104B7A2AEB");
        }

        [Fact]
        public void SarifLogger_WritesFileDataWithUnrecognizedEncoding()
        {
            var sb = new StringBuilder();
            string file;
            string fileText = "using System;";

            using (var tempFile = new TempFile(".cs"))
            using (var textWriter = new StringWriter(sb))
            {
                file = tempFile.Name;
                File.WriteAllText(file, fileText);

                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: new string[] { file },
                    dataToInsert: OptionallyEmittedData.TextFiles,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: null,
                    defaultFileEncoding: "ImaginaryEncoding"))
                {
                    string ruleId = "RuleId";
                    var rule = new Rule() { Id = ruleId };

                    var result = new Result()
                    {
                        RuleId = ruleId
                    };

                    sarifLogger.Log(rule, result);
                }
            }

            string logText = sb.ToString();

            string fileDataKey = new Uri(file).AbsoluteUri;
            byte[] fileBytes = Encoding.Default.GetBytes(fileText);

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(logText);
            FileData fileData = sarifLog.Runs[0].Files[fileDataKey];
            fileData.MimeType.Should().Be(MimeType.CSharp);
            fileData.Contents.Binary.Should().Be(Convert.ToBase64String(fileBytes));
            fileData.Contents.Text.Should().BeNull();
        }

        [Fact]
        public void SarifLogger_ScrapesFilesFromResult()
        {
            var sb = new StringBuilder();

            using (var textWriter = new StringWriter(sb))
            {
                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: null,
                    dataToInsert: OptionallyEmittedData.Hashes,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: null))
                {
                    string ruleId = "RuleId";
                    var rule = new Rule() { Id = ruleId };

                    var result = new Result()
                    {
                        RuleId = ruleId,
                        AnalysisTarget = new FileLocation { Uri = @"file:///file0.cpp" },
                        Locations = new[]
                        {
                            new Location
                            {
                                PhysicalLocation = new PhysicalLocation
                                {
                                    FileLocation = new FileLocation
                                    {
                                        Uri = @"file:///file1.cpp"
                                    }
                                }
                            },
                        },
                        Fixes = new[]
                        {
                            new Fix
                            {
                                FileChanges = new[]
                                {
                                   new FileChange
                                   {
                                    FileLocation = new FileLocation
                                    {
                                        Uri = @"file:///file2.cpp"
                                    }
                                   }
                                }
                            }
                        },
                        RelatedLocations = new[]
                        {
                            new Location
                            {
                                PhysicalLocation = new PhysicalLocation
                                {
                                    FileLocation = new FileLocation
                                    {
                                        Uri = @"file:///file3.cpp"
                                    }
                                }
                            }
                        },
                        Stacks = new[]
                        {
                            new Stack
                            {
                                Frames = new[]
                                {
                                    new StackFrame
                                    {
                                        Location = new Location
                                        {
                                            PhysicalLocation = new PhysicalLocation
                                            {
                                                FileLocation = new FileLocation
                                                {
                                                    Uri = @"file:///file4.cpp"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        CodeFlows = new[]
                        {
                            new CodeFlow
                            {
                                ThreadFlows = new[]
                                {
                                    new ThreadFlow
                                    {
                                        Locations = new[]
                                        {
                                            new ThreadFlowLocation
                                            {
                                                Location = new Location
                                                {
                                                    PhysicalLocation = new PhysicalLocation
                                                    {
                                                        FileLocation = new FileLocation
                                                        {
                                                            Uri = @"file:///file5.cpp"
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };

                    sarifLogger.Log(rule, result);

                }
            }

            string logText = sb.ToString();
            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(logText);

            int fileCount = 6;

            for (int i = 0; i < fileCount; ++i)
            {
                string fileName = @"file" + i + ".cpp";
                string fileDataKey = "file:///" + fileName;
                sarifLog.Runs[0].Files.Should().ContainKey(fileDataKey, "file data for " + fileName + " should exist in files collection");
            }

            sarifLog.Runs[0].Files.Count.Should().Be(fileCount);
        }

        [Fact]
        public void SarifLogger_DoNotScrapeFilesFromNotifications()
        {
            var sb = new StringBuilder();

            using (var textWriter = new StringWriter(sb))
            {
                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: null,
                    dataToInsert: OptionallyEmittedData.Hashes,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: null))
                {                    
                    var toolNotification = new Notification
                    {
                        PhysicalLocation = new PhysicalLocation { FileLocation = new FileLocation { Uri = @"file:///file0.cpp" } }
                    };
                    sarifLogger.LogToolNotification(toolNotification);

                    var configurationNotification = new Notification
                    {
                        PhysicalLocation = new PhysicalLocation { FileLocation = new FileLocation { Uri = @"file:///file0.cpp" } }
                    };
                    sarifLogger.LogConfigurationNotification(configurationNotification);

                    string ruleId = "RuleId";
                    var rule = new Rule() { Id = ruleId };

                    var result = new Result()
                    {
                        RuleId = ruleId
                    };

                    sarifLogger.Log(rule, result);
                }
            }

            string logText = sb.ToString();
            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(logText);

            sarifLog.Runs[0].Files.Should().BeNull();
        }

        [Fact]
        public void SarifLogger_LogsStartAndEndTimesByDefault()
        {
            var sb = new StringBuilder();

            using (var textWriter = new StringWriter(sb))
            {
                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: null,
                    dataToInsert: OptionallyEmittedData.Hashes,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: null))
                {

                    string ruleId = "RuleId";
                    var rule = new Rule() { Id = ruleId };

                    var result = new Result()
                    {
                        RuleId = ruleId
                    };

                    sarifLogger.Log(rule, result);
                }
            }

            string logText = sb.ToString();
            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(logText);

            Invocation invocation = sarifLog.Runs[0].Invocations?[0];
            invocation.StartTime.Should().NotBe(DateTime.MinValue);
            invocation.EndTime.Should().NotBe(DateTime.MinValue);

            // Other properties should be empty.
            invocation.CommandLine.Should().BeNull();
            invocation.WorkingDirectory.Should().BeNull();
            invocation.ProcessId.Should().Be(0);
            invocation.ExecutableLocation.Should().BeNull();
        }

        [Fact]
        public void SarifLogger_LogsSpecifiedInvocationProperties()
        {
            var sb = new StringBuilder();

            using (var textWriter = new StringWriter(sb))
            {
                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: null,
                    dataToInsert: OptionallyEmittedData.Hashes,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: new[] { "WorkingDirectory", "ProcessId" }))
                {

                    string ruleId = "RuleId";
                    var rule = new Rule() { Id = ruleId };

                    var result = new Result()
                    {
                        RuleId = ruleId
                    };

                    sarifLogger.Log(rule, result);
                }
            }

            string logText = sb.ToString();
            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(logText);

            Invocation invocation = sarifLog.Runs[0].Invocations[0];

            // StartTime and EndTime should still be logged.
            invocation.StartTime.Should().NotBe(DateTime.MinValue);
            invocation.EndTime.Should().NotBe(DateTime.MinValue);

            // Specified properties should be logged.
            invocation.WorkingDirectory.Should().NotBeNull();
            invocation.ProcessId.Should().NotBe(0);

            // Other properties should be empty.
            invocation.CommandLine.Should().BeNull();
            invocation.ExecutableLocation.Should().BeNull();
        }

        [Fact]
        public void SarifLogger_TreatsInvocationPropertiesCaseInsensitively()
        {
            var sb = new StringBuilder();

            using (var textWriter = new StringWriter(sb))
            {
                using (var sarifLogger = new SarifLogger(
                    textWriter,
                    analysisTargets: null,
                    dataToInsert: OptionallyEmittedData.Hashes,
                    prereleaseInfo: null,
                    invocationTokensToRedact: null,
                    invocationPropertiesToLog: new[] { "WORKINGDIRECTORY", "prOCessID" }))
                {

                    string ruleId = "RuleId";
                    var rule = new Rule() { Id = ruleId };

                    var result = new Result()
                    {
                        RuleId = ruleId
                    };

                    sarifLogger.Log(rule, result);
                }
            }

            string logText = sb.ToString();
            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(logText);

            Invocation invocation = sarifLog.Runs[0].Invocations?[0];

            // Specified properties should be logged.
            invocation.WorkingDirectory.Should().NotBeNull();
            invocation.ProcessId.Should().NotBe(0);
        }

        [Fact]
        public void SarifLogger_ResultAndRuleIdMismatch()
        {
            var sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            using (var sarifLogger = new SarifLogger(writer, LoggingOptions.Verbose))
            {
                var rule = new Rule()
                {
                    Id = "ActualId"
                };

                var result = new Result()
                {
                    RuleId = "IncorrectRuleId",
                    Message = new Message { Text = "test message" }
                };

                Assert.Throws<ArgumentException>(() => sarifLogger.Log(rule, result));
            }
        }        

        [Fact]
        public void SarifLogger_LoggingOptions()
        {
            foreach (object loggingOptionsObject in Enum.GetValues(typeof(LoggingOptions)))
            {
                TestForLoggingOption((LoggingOptions)loggingOptionsObject);
            }
        }


        // This helper is intended to validate a single enum member only
        // and not arbitrary combinations of bits. One defined member,
        // All, contains all bits.
        private void TestForLoggingOption(LoggingOptions loggingOption)
        {
            string fileName = Path.GetTempFileName();

            try
            {
                SarifLogger logger;

                // Validates overload that accept a path argument.
                using (logger = new SarifLogger(fileName, loggingOption))
                {
                    ValidateLoggerForExclusiveOption(logger, loggingOption);
                };

                // Validates overload that accepts any 
                // TextWriter (for example, one instantiated over a
                // StringBuilder instance).
                var sb = new StringBuilder();
                var stringWriter = new StringWriter(sb);
                using (logger = new SarifLogger(stringWriter, loggingOption))
                {
                    ValidateLoggerForExclusiveOption(logger, loggingOption);
                };
            }            
            finally
            {
                if (File.Exists(fileName)) { File.Delete(fileName); }
            }
        }

        private void ValidateLoggerForExclusiveOption(SarifLogger logger, LoggingOptions loggingOptions)
        {
            switch (loggingOptions)
            {
                case LoggingOptions.None:
                {
                    logger.OverwriteExistingOutputFile.Should().BeFalse();
                    logger.PrettyPrint.Should().BeFalse();
                    logger.Verbose.Should().BeFalse();
                    break;
                }
                case LoggingOptions.OverwriteExistingOutputFile:
                {
                    logger.OverwriteExistingOutputFile.Should().BeTrue();
                    logger.PrettyPrint.Should().BeFalse();
                    logger.Verbose.Should().BeFalse();
                    break;
                }
                case LoggingOptions.PrettyPrint:
                {
                    logger.OverwriteExistingOutputFile.Should().BeFalse();
                    logger.PrettyPrint.Should().BeTrue();
                    logger.Verbose.Should().BeFalse();
                    break;
                }
                case LoggingOptions.Verbose:
                {
                    logger.OverwriteExistingOutputFile.Should().BeFalse();
                    logger.PrettyPrint.Should().BeFalse();
                    logger.Verbose.Should().BeTrue();
                    break;
                }
                case LoggingOptions.All:
                {
                    logger.OverwriteExistingOutputFile.Should().BeTrue();
                    logger.PrettyPrint.Should().BeTrue();
                    logger.Verbose.Should().BeTrue();
                    break;
                }
                default:
                {
                    throw new ArgumentException();
                }
            }
        }
    }
}
