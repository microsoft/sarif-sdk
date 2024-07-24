// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Moq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class OptionsInterpretterTests
    {
        [Fact]
        public void OptionsInterpretter_ChangesNothingWhenEnvVarsEmpty()
        {
            var mockEnvironmentVariableGetter = new Mock<IEnvironmentVariableGetter>();
            //  Don't setup any responses so they always return null

            var optionsInterpretter = new OptionsInterpretter(mockEnvironmentVariableGetter.Object);

            var beforeAndAfter = new List<ValidateOptions>(2);

            for (int i = 0; i < beforeAndAfter.Capacity; i++)
            {
                beforeAndAfter.Add(new ValidateOptions
                {
                    DataToInsert = new List<OptionallyEmittedData> { OptionallyEmittedData.Hashes, OptionallyEmittedData.EnvironmentVariables },
                    DataToRemove = new List<OptionallyEmittedData> { OptionallyEmittedData.VersionControlDetails },
                    Kind = new List<ResultKind> { ResultKind.Fail },
                    Level = new List<FailureLevel> { FailureLevel.Error, FailureLevel.Warning }
                });
            }

            optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(beforeAndAfter[1]);

            Assert.True(beforeAndAfter[0].DataToInsert.SequenceEqual(beforeAndAfter[1].DataToInsert));
            Assert.True(beforeAndAfter[0].DataToRemove.SequenceEqual(beforeAndAfter[1].DataToRemove));
            Assert.True(beforeAndAfter[0].Kind.SequenceEqual(beforeAndAfter[1].Kind));
            Assert.True(beforeAndAfter[0].Level.SequenceEqual(beforeAndAfter[1].Level));
        }

        [Fact]
        public void OptionsInterpretter_CorrectlyAddsAdditiveEnvVars()
        {
            var mockEnvironmentVariableGetter = new Mock<IEnvironmentVariableGetter>();
            mockEnvironmentVariableGetter.Setup(x => x.GetEnvironmentVariable("SARIF_DATATOINSERT_ADDITION")).Returns("TextFiles;BinaryFiles;");
            //  Deliberately more delimeters than needed
            mockEnvironmentVariableGetter.Setup(x => x.GetEnvironmentVariable("SARIF_DATATOREMOVE_ADDITION")).Returns("ComprehensiveRegionProperties;RegionSnippets");
            mockEnvironmentVariableGetter.Setup(x => x.GetEnvironmentVariable("SARIF_KIND_ADDITION")).Returns("Informational;");
            //  Deliberately more delimeters than needed
            mockEnvironmentVariableGetter.Setup(x => x.GetEnvironmentVariable("SARIF_LEVEL_ADDITION")).Returns("Note");

            var optionsInterpretter = new OptionsInterpretter(mockEnvironmentVariableGetter.Object);

            var analyzeOptionsBase = new ValidateOptions
            {
                DataToInsert = new List<OptionallyEmittedData> { OptionallyEmittedData.Hashes, OptionallyEmittedData.EnvironmentVariables },
                DataToRemove = new List<OptionallyEmittedData> { OptionallyEmittedData.VersionControlDetails },
                Kind = new List<ResultKind> { ResultKind.Fail },
                Level = new List<FailureLevel> { FailureLevel.Error, FailureLevel.Warning }
            };

            optionsInterpretter.ConsumeEnvVarsAndInterpretOptions(analyzeOptionsBase);

            analyzeOptionsBase.DataToInsert.Should().Contain(OptionallyEmittedData.TextFiles);
            analyzeOptionsBase.DataToInsert.Should().Contain(OptionallyEmittedData.BinaryFiles);
            analyzeOptionsBase.DataToRemove.Should().Contain(OptionallyEmittedData.ComprehensiveRegionProperties);
            analyzeOptionsBase.DataToRemove.Should().Contain(OptionallyEmittedData.RegionSnippets);
            analyzeOptionsBase.Kind.Should().Contain(ResultKind.Informational);
            analyzeOptionsBase.Level.Should().Contain(FailureLevel.Note);
        }
    }
}
