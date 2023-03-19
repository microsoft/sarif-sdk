// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class TestUtilitiesExtensions
    {
        public static void ValidateCommandExecution(this IAnalysisContext context, int result)
        {
            // This method provides validation of execution success for happy path runs, 
            // i.e., where we don't expect to see anything unusual. The validation is
            // specifically ordered to provide the most information in the test output 
            // window. If we validate the success code, for example, we only know that
            // we returned FAILURE (1) not SUCCESS. If we validate the exceptions data
            // first, the test output window will show a meaningful message and stack.

            // For application exist exceptions, e.g., an unhandled exception in a rule,
            // the inner exception has the most useful details.
            context.RuntimeExceptions?[0].InnerException.Should().BeNull();
            context.RuntimeExceptions?[0].Should().BeNull();

            context.RuntimeErrors.Fatal().Should().Be(0);

            result.Should().Be(CommandBase.SUCCESS);
        }

        public static IList<T> Shuffle<T>(this IList<T> list, Random random)
        {
            if (list == null)
            {
                return null;
            }

            if (random == null)
            {
                // Random object with seed logged in test is required.
                throw new ArgumentNullException(nameof(random));
            }

            return list.OrderBy(item => random.Next()).ToList();
        }
    }
}
