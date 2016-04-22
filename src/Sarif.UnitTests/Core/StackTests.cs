// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;
using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class StackTests
    {
        [TestMethod]
        public void Stack_CreateFromStackTrace()
        {
            var dotNetStack = new StackTrace();
            Stack stack = new Stack(dotNetStack);

            // The .NET StackTrace.ToString() override must preserve a trailing NewLine
            // for compatibility reasons. We do not retain this behavior in ToString()
            // but provide a facility for adding the trailing NewLine
            Assert.AreEqual(dotNetStack.ToString(), stack.ToString(StackFormat.TrailingNewLine));
        }

        [TestMethod]
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
                Assert.AreEqual(exception.StackTrace, stacks[0].ToString());

                caughtException = true;
            }
            Assert.IsTrue(caughtException);
        }

        [TestMethod]
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
                Assert.AreEqual("[No frames]", stacks[0].ToString());
                Assert.AreEqual(exception.StackTrace, stacks[1].ToString());

                caughtException = true;
            }
            Assert.IsTrue(caughtException);
        }

        [TestMethod]
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

                Assert.AreEqual("[No frames]", stacks[0].ToString());
                Assert.AreEqual("[No frames]", stacks[1].ToString());
                Assert.AreEqual("[No frames]", stacks[2].ToString());
                Assert.AreEqual(exception.StackTrace, stacks[3].ToString());

                Assert.AreEqual(aggregated.FormatMessage(), stacks[0].Message);
                Assert.AreEqual(innerException1.FormatMessage(), stacks[1].Message);
                Assert.AreEqual(innerException2.FormatMessage(), stacks[2].Message);
                Assert.AreEqual(exception.FormatMessage(), stacks[3].Message);

                caughtException = true;
            }
            Assert.IsTrue(caughtException);
        }
    }
}
