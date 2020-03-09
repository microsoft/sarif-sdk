// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class AssertionExtensions
    {
        /// <summary>
        /// Asserts that two strings are identical except for their end of line conventions.
        /// </summary>
        /// <remarks>
        /// This is useful for comparing logs that might have been generated on platforms with different
        /// end of line conventions.
        /// </remarks>
        public static AndConstraint<StringAssertions> BeCrossPlatformEquivalentStrings(
            this StringAssertions assertion,
            string expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(
                    string.Equals(RemoveLineEndings(expected), RemoveLineEndings(assertion.Subject), StringComparison.Ordinal)
                )
                .BecauseOf(because, becauseArgs)
                .FailWith(TestUtilitiesResources.BeCrossPlatformEquivalentError);

            return new AndConstraint<StringAssertions>(assertion);
        }

        /// <summary>
        /// An async version of FluentAssertions's Should().Throw&ltT>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of exception expected.
        /// </typeparam>
        /// <param name="testCode">
        /// An async method, anonymous function, or lambda which is expected to throw
        /// an exception of type T.
        /// </param>
        /// <returns>
        /// A Task representing the completion of the specified code.
        /// </returns>
        /// <remarks>
        /// Based on https://gist.github.com/haacked/4616366. See https://haacked.com/archive/2013/01/24/async-lambdas.aspx/.
        /// </remarks>
        public async static Task<T> ShouldThrowAsync<T>(this Func<Task> testCode) where T : Exception
        {
            try
            {
                await testCode();

                // If the test code did not throw as expected, then invoke Should().Throw<T>
                // on a synchronous lambda that does not throw any exceptions. As a result,
                // FluentAssertions will complain that the code we supplied did not throw.
                Action failureAction = () => { };
                failureAction.Should().Throw<T>(); // Use xUnit's default behavior.
            }
            catch (T exception)
            {
                return exception;
            }

            // The compiler doesn't know that the call to Should().Throw<T> above will
            // always throw, so it doesn't know that execution will never reach here.
            return null;
        }

        /// <summary>
        /// An async version of FluentAssertions's ShouldNotThrow.
        /// </summary>
        /// <param name="testCode">
        /// An async method, anonymous function, or lambda which is not expected to throw an exception.
        /// </param>
        /// <returns>
        /// A Task representing the completion of the specified code.
        /// </returns>
        /// <remarks>
        /// Based on https://gist.github.com/haacked/4616366. See https://haacked.com/archive/2013/01/24/async-lambdas.aspx/.
        /// </remarks>
        public async static Task ShouldNotThrowAsync(this Func<Task> testCode)
        {
            try
            {
                await testCode();
            }
            catch (Exception exception)
            {
                // If the test code unexpectedly threw, then invoke Should().NotThrow()
                // on a synchronous lambda that does throw an exception. As a result,
                // FluentAssertions will complain that the code we supplied threw.
                Action failureAction = () => throw exception;
                failureAction.Should().NotThrow();
            }
        }

        private static string RemoveLineEndings(string input)
        {
            return Regex.Replace(input, @"\s+", "");
        }

        public static AndConstraint<StringAssertions> BeCrossPlatformEquivalent<T>(
            this StringAssertions assertion,
            string expected, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(
                    FileDiffingFunctionalTests.AreEquivalent<T>(actualSarif: assertion.Subject, expected)
                )
                .BecauseOf(because, becauseArgs)
                .FailWith(TestUtilitiesResources.BeCrossPlatformEquivalentError);

            return new AndConstraint<StringAssertions>(assertion);
        }
    }
}
