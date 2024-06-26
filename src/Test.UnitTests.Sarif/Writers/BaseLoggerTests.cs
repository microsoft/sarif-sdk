// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

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

            Assert.Throws<ArgumentException>(() => new BaseLoggerTestConcrete(new FailureLevelSet(new[] { FailureLevel.Error }),
                                                                              new ResultKindSet(new[] { ResultKind.Informational }),
                                                                              0));
            //  The rest are fine.
            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new FailureLevelSet(new[] { FailureLevel.Error }),
                                                                new ResultKindSet(new[] { ResultKind.Informational, ResultKind.Fail }),
                                                                1);

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new FailureLevelSet(new[] { FailureLevel.Note }),
                                                                BaseLogger.Fail,
                                                                -1);

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new FailureLevelSet(new[] { FailureLevel.None }),
                                                                new ResultKindSet(new[] { ResultKind.Informational, ResultKind.Fail }),
                                                                10);

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new FailureLevelSet(new[] { FailureLevel.None }),
                                                                new ResultKindSet(new[] { ResultKind.Fail }),
                                                                0);

            //  If there are no uncaught exceptions, the test passes.
        }
    }
}
