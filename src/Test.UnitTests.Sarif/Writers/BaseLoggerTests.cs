// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Writers
{
    public class BaseLoggerTests
    {
        [Fact]
        public void BaseLogger_ShouldCorrectlyValidateParameters()
        {
            BaseLoggerTestConcrete baseLoggerTestConcrete = null;

            Assert.Throws<ArgumentException>(() => new BaseLoggerTestConcrete(new List<FailureLevel> { FailureLevel.Error },
                                                                    new List<ResultKind> { ResultKind.Informational }));
            //  The rest are fine.
            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel> { FailureLevel.Error },
                                                                new List<ResultKind> { ResultKind.Informational, ResultKind.Fail });

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel> { FailureLevel.Note },
                                                                new List<ResultKind> { ResultKind.Fail });

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel> { FailureLevel.None },
                                                                new List<ResultKind> { ResultKind.Informational });

            //  If there are no uncaught exceptions, the test passes.
        }

        [Fact]
        public void BaseLogger_ShouldLog()
        {
            Result result;
            ReportingConfiguration defaultConfiguration;

            var levels = new List<FailureLevel> { FailureLevel.Error, FailureLevel.Warning };

            result = new Result() { Level = FailureLevel.Error };
            Assert.True(ShouldLog(result, levels));

            result = new Result() { Level = FailureLevel.Warning };
            Assert.True(ShouldLog(result, levels));

            result = new Result() { Level = FailureLevel.Note };
            Assert.False(ShouldLog(result, levels));

            result = new Result() { Level = FailureLevel.None };
            Assert.False(ShouldLog(result, levels));

            result = new Result() { };
            Assert.True(ShouldLog(result, levels));

            levels = new List<FailureLevel> { FailureLevel.Error };
            Assert.False(ShouldLog(result, levels));

            result = new Result() { Level = FailureLevel.Error };

            defaultConfiguration = new ReportingConfiguration() { Enabled = true, Level = FailureLevel.Note };
            Assert.True(ShouldLog(result, levels, defaultConfiguration));

            result = new Result() { };

            // Result.Level not set, Default Configuration Error, Log Level Error => log
            defaultConfiguration = new ReportingConfiguration() { Enabled = true, Level = FailureLevel.Error };
            Assert.True(ShouldLog(result, levels, defaultConfiguration));

            // Result.Level not set, Default Configuration Note, Log Level Error => do not log
            defaultConfiguration = new ReportingConfiguration() { Enabled = true, Level = FailureLevel.Note };
            Assert.False(ShouldLog(result, levels, defaultConfiguration));

            // Result.Level not set, Default Configuration disabled, Log Level Error => defult to warning, do not log
            defaultConfiguration = new ReportingConfiguration() { Enabled = false, Level = FailureLevel.Note };
            Assert.False(ShouldLog(result, levels, defaultConfiguration));

            // Result.Level not set, Default Configuration not set, Log Level Error => defult to warning, do not log
            Assert.False(ShouldLog(result, levels));

            levels = new List<FailureLevel> { FailureLevel.Warning };

            // Result.Level not set, Default Configuration Error, Log Level Warning => do not log
            defaultConfiguration = new ReportingConfiguration() { Enabled = true, Level = FailureLevel.Error };
            Assert.False(ShouldLog(result, levels, defaultConfiguration));

            // Result.Level not set, Default Configuration disabled, Log Level Warning => defult to warning, log
            defaultConfiguration = new ReportingConfiguration() { Enabled = false, Level = FailureLevel.Note };
            Assert.True(ShouldLog(result, levels, defaultConfiguration));

            // Result.Level not set, Default Configuration not set, Log Level Warning => defult to warning, log
            Assert.True(ShouldLog(result, levels));
        }

        private static bool ShouldLog(Result result, IEnumerable<FailureLevel> levels,
            ReportingConfiguration defaultConfiguration = null, ReportingConfiguration overrideConfiguration = null)
        {
            bool shouldLog = false;

            const string sampleRuleId = "SampleRuleId";
            const string sampleMessage = "Sample Message";
            result.RuleId = sampleRuleId;
            result.Message = new Message() { Text = sampleMessage };

            List<ConfigurationOverride> ruleConfigurationOverrides = null;

            if (overrideConfiguration != null)
            {
                var ruleConfigurationOverride = new ConfigurationOverride()
                {
                    Descriptor = new ReportingDescriptorReference()
                    {
                        Index = 0
                    },
                    Configuration = overrideConfiguration
                };
                ruleConfigurationOverrides = new List<ConfigurationOverride>() { ruleConfigurationOverride };
            }

            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                using (var logger = new SarifLogger(
                    streamWriter,
                    logFilePersistenceOptions: LogFilePersistenceOptions.PrettyPrint,
                    dataToRemove: OptionallyEmittedData.NondeterministicProperties,
                    closeWriterOnDispose: false,
                    run: new Run() { Invocations = new List<Invocation> { new Invocation() { RuleConfigurationOverrides = ruleConfigurationOverrides } } },
                    levels: levels,
                    kinds: new List<ResultKind> { ResultKind.None, ResultKind.NotApplicable, ResultKind.Pass,
                    ResultKind.Fail, ResultKind.Review, ResultKind.Open, ResultKind.Informational }))
                {
                    shouldLog = logger.ShouldLog(result, new ReportingDescriptor { Id = sampleRuleId, DefaultConfiguration = defaultConfiguration });
                }

                // Important. Force streamwriter to commit everything.
                streamWriter.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
            }

            return shouldLog;
        }
    }
}
