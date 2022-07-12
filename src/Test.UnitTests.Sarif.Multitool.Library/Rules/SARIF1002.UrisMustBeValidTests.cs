// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Multitool.Rules;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class UrisMustBeValidTests
    {
        [Fact]
        public void UrisMustBeValid_ShouldProduceExpectedResults()
        {
            var testSkimmer = new UrisMustBeValid();

        }
    }
}
