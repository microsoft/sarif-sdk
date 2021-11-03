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
        public void BaseLogger_ShouldCorrectlyValidateParameters()
        {
            BaseLoggerTestConcrete baseLoggerTestConcrete = null;

            Assert.Throws<ArgumentException>(() => new BaseLoggerTestConcrete(new List<FailureLevel?> { FailureLevel.Error },
                                                                    new List<ResultKind> { ResultKind.Informational }));
            //  The rest are fine.
            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel?> { FailureLevel.Error },
                                                                new List<ResultKind> { ResultKind.Informational, ResultKind.Fail });

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel?> { FailureLevel.Note },
                                                                new List<ResultKind> { ResultKind.Fail });

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new List<FailureLevel?> { FailureLevel.None },
                                                                new List<ResultKind> { ResultKind.Informational });

            //  If there are no uncaught exceptions, the test passes.
        }
    }
}
