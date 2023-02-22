// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using FluentAssertions;

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

            Assert.Throws<ArgumentException>(() => new BaseLoggerTestConcrete(new[] { FailureLevel.Error }.ToImmutableHashSet(),
                                                                    new List<ResultKind> { ResultKind.Informational }.ToImmutableHashSet()));
            //  The rest are fine.
            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new[] { FailureLevel.Error }.ToImmutableHashSet(),
                                                                new List<ResultKind> { ResultKind.Informational, ResultKind.Fail }.ToImmutableHashSet());

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new[] { FailureLevel.Note }.ToImmutableHashSet(),
                                                                BaseLogger.Fail);

            baseLoggerTestConcrete = new BaseLoggerTestConcrete(new[] { FailureLevel.None }.ToImmutableHashSet(),
                                                                new List<ResultKind> { ResultKind.Informational }.ToImmutableHashSet());

            //  If there are no uncaught exceptions, the test passes.
        }
    }
}
