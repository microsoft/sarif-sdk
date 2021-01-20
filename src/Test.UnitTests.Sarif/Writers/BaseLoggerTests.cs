// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Writers
{
    public class BaseLoggerTests
    {
        [Fact]
        public void BaseLogger_ShouldAcceptEmptyAndNullKindsAndFailures()
        {
            //  null levels, null kinds
            BaseLoggerTestConcrete baseLoggerTestConcrete = new BaseLoggerTestConcrete(null, null);
            AssertLevelAndKindAreNonEmpty(baseLoggerTestConcrete);

            //  null levels, empty kinds
            baseLoggerTestConcrete = new BaseLoggerTestConcrete(null, new List<ResultKind> { });
            AssertLevelAndKindAreNonEmpty(baseLoggerTestConcrete);

            //  empty levels, null kinds
            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel> { }, null);
            AssertLevelAndKindAreNonEmpty(baseLoggerTestConcrete);

            //  empty levels, empty kinds
            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel> { }, new List<ResultKind> { });
            AssertLevelAndKindAreNonEmpty(baseLoggerTestConcrete);
        }

        private void AssertLevelAndKindAreNonEmpty(BaseLoggerTestConcrete baseLoggerTestConcrete)
        {
            baseLoggerTestConcrete.FailureLevelsPublicViewer.Should().NotBeEmpty();
            baseLoggerTestConcrete.ResultKindPublicViewer.Should().NotBeEmpty();
        }

        [Fact]
        public void BaseLogger_ShouldCorrectlyDefault()
        {
            BaseLoggerTestConcrete baseLoggerTestConcrete = new BaseLoggerTestConcrete(null, null);

            baseLoggerTestConcrete.ResultKindPublicViewer.Count.Should().Be(1);
            baseLoggerTestConcrete.ResultKindPublicViewer[0].Should().Be(ResultKind.Fail);

            baseLoggerTestConcrete.FailureLevelsPublicViewer.Count.Should().Be(2);
            baseLoggerTestConcrete.FailureLevelsPublicViewer.Should().Contain(FailureLevel.Warning);
            baseLoggerTestConcrete.FailureLevelsPublicViewer.Should().Contain(FailureLevel.Error);
        }

        [Fact]
        public void BaseLogger_ShouldCorrectlyValidateParameters()
        {
            BaseLoggerTestConcrete baseLoggerTestConcrete = null;

            try
            {
                baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel> { FailureLevel.Error },
                                                                    new List<ResultKind> { ResultKind.Informational });
                Assert.True(false, "Expected exception not thrown, BaseLogger did not validate correctly.");
            }
            catch (ArgumentException)
            {}
            //  The rest are fine.
            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel> { FailureLevel.Error },
                                                                new List<ResultKind> { ResultKind.Informational, ResultKind.Fail });

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel> { FailureLevel.None },
                                                                new List<ResultKind> { ResultKind.Fail });

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel> { FailureLevel.Note },
                                                                new List<ResultKind> { ResultKind.Fail });

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel> { FailureLevel.None },
                                                                new List<ResultKind> { ResultKind.Informational });

            //  If there are no uncaught exceptions, the test passes.
        }
    }
}
