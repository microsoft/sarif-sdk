// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public abstract class QueryCommandTestsBase
    {
        protected static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(QueryCommandTestsBase));

        protected void RunAndVerifyCount(int expectedCount, QueryOptions options)
        {
            options.ReturnCount = true;
            int exitCode = new QueryCommand().RunWithoutCatch(options);
            exitCode.Should().Be(expectedCount);
        }
    }
}
