// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class ResultLevelKindTests
    {
        [Fact]
        public void ResultLevelKind_ValidateConditions()
        {
            var obj = new ResultLevelKind();
            obj.Kind.Should().Be(ResultKind.Fail);
            obj.Level.Should().Be(FailureLevel.None);

            obj.Kind = ResultKind.Informational;
            obj.Kind.Should().Be(ResultKind.Informational);
            obj.Level.Should().Be(FailureLevel.None);

            obj.Level = FailureLevel.Warning;
            obj.Kind.Should().Be(ResultKind.Fail);
            obj.Level.Should().Be(FailureLevel.Warning);
        }
    }
}
