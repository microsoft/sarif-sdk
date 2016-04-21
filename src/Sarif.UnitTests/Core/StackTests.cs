// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class StackTests
    {
        [TestMethod]
        public void Stack_CreateFromStackTrace()
        {
            var dotNetStack = new StackTrace();
            Stack stack = Stack.Create(dotNetStack);

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
                IEnumerable<Stack> stacks = Stack.Create(exception);
                Assert.AreEqual(1, stacks.Count());
                Assert.AreEqual(exception.StackTrace, stacks.First().ToString());
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

                IEnumerable<Stack> stacks = Stack.Create(containerException);
                Assert.AreEqual(2, stacks.Count());

                Assert.AreEqual(exception.StackTrace, stacks.ElementAt(1).ToString());
                Assert.AreEqual(containerException.StackTrace, stacks.First().ToString());
                caughtException = true;

            }
            Assert.IsTrue(caughtException);
        }
    }
}
