// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class StackTests
    {
        [TestMethod]
        public void Stack_Create()
        {
            Stack stack = Stack.Create(new StackTrace());
            Console.WriteLine(stack);
        }
    }
}
