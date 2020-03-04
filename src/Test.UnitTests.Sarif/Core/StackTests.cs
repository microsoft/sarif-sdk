// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class StackTests
    {
        [Fact(Skip = "Broken by new, unexpected framework behavior in StackTrace class. Issue #1163")]
        public void Stack_CreateFromStackTrace()
        {
            var dotNetStack = new StackTrace();
            Stack stack = new Stack(dotNetStack);

            // The .NET StackTrace.ToString() override must preserve a trailing NewLine
            // for compatibility reasons. We do not retain this behavior in ToString()
            // but provide a facility for adding the trailing NewLine
            stack.ToString(StackFormat.TrailingNewLine).Should().BeCrossPlatformEquivalentStrings(dotNetStack.ToString());
        }

        [Fact]
        public void Stack_CreateFromException()
        {
            bool caughtException = false;
            try
            {
                File.Create(Path.GetInvalidFileNameChars()[0].ToString(), 0);
            }
            catch (ArgumentException exception)
            {
                IList<Stack> stacks = Stack.CreateStacks(exception).ToList();

                stacks.Count.Should().Be(1);
                stacks[0].ToString().Should().BeCrossPlatformEquivalentStrings(exception.StackTrace);

                caughtException = true;
            }
            Assert.True(caughtException);
        }

        [Fact]
        public void Stack_CreateFromExceptionWithInnerException()
        {
            bool caughtException = false;
            try
            {
                File.Create(Path.GetInvalidFileNameChars()[0].ToString(), 0);
            }
            catch (ArgumentException exception)
            {
                Exception containerException = new InvalidOperationException("test exception", exception);

                IList<Stack> stacks = Stack.CreateStacks(containerException).ToList();

                stacks.Count.Should().Be(2);
                containerException.StackTrace.Should().Be(null);
                Assert.Equal("[No frames]", stacks[0].ToString());
                stacks[1].ToString().Should().BeCrossPlatformEquivalentStrings(exception.StackTrace);

                caughtException = true;
            }
            Assert.True(caughtException);
        }

        [Fact]
        public void Stack_CreateFromAggregatedExceptionWithInnerException()
        {
            bool caughtException = false;
            try
            {
                File.Create(Path.GetInvalidFileNameChars()[0].ToString(), 0);
            }
            catch (ArgumentException exception)
            {
                var innerException1 = new InvalidOperationException("Test exception 1.");
                var innerException2 = new InvalidOperationException("Test exception 2.", exception);

                var aggregated = new AggregateException(innerException1, innerException2);

                IList<Stack> stacks = Stack.CreateStacks(aggregated).ToList();

                stacks.Count.Should().Be(4);
                aggregated.StackTrace.Should().Be(null);

                Assert.Equal("[No frames]", stacks[0].ToString());
                Assert.Equal("[No frames]", stacks[1].ToString());
                Assert.Equal("[No frames]", stacks[2].ToString());
                stacks[3].ToString().Should().BeCrossPlatformEquivalentStrings(exception.StackTrace);

                Assert.Equal(aggregated.FormatMessage(), stacks[0].Message.Text);
                Assert.Equal(innerException1.FormatMessage(), stacks[1].Message.Text);
                Assert.Equal(innerException2.FormatMessage(), stacks[2].Message.Text);
                Assert.Equal(exception.FormatMessage(), stacks[3].Message.Text);

                caughtException = true;
            }
            Assert.True(caughtException);
        }
    }
}
