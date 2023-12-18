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
                                                                              new ResultKindSet(new[] { ResultKind.Informational })));
            //  The rest are fine.
            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new FailureLevelSet(new[] { FailureLevel.Error }),
                                                                new ResultKindSet(new[] { ResultKind.Informational, ResultKind.Fail }));

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new FailureLevelSet(new[] { FailureLevel.Note }),
                                                                BaseLogger.Fail);

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new FailureLevelSet(new[] { FailureLevel.None }),
                                                                new ResultKindSet(new[] { ResultKind.Informational, ResultKind.Fail }));

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new FailureLevelSet(new[] { FailureLevel.None }),
                                                                new ResultKindSet(new[] { ResultKind.Fail }));

            //  If there are no uncaught exceptions, the test passes.
        }
    }
}
