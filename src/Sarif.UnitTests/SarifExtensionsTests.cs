// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class SarifExtensionsTests
    {
        [TestMethod]
        public void SarifExtensions_IsSemanticVersioningCompatible()
        {
            var tests = new Dictionary<string, bool>()
            {
                { "1.2.3", true },
                { "1.0.0-alpha", true },
                { "1.0.0-alpha.1", true },
                { "1.0.0-0.3.7", true },
                { "1.0.0-x.7.z.92", true },
                { "1.0.0-alpha+001", true },
                { "1.0.0+20130313144700", true },
                { "1.0.0-beta+exp.sha.5114f85", true },

                { "1", false },
                { "1.2", false },
                { "1.2.3.4", false },
            };

            var sb = new StringBuilder();

            foreach (string input in tests.Keys)
            {
                bool expected = tests[input];

                if (expected != input.IsSemanticVersioningCompatible())
                {
                    // If we expected true but failed, the we have unexpectedly
                    // encountered an invalid case, otherwise we have unexpectedly
                    // encountered a valid case.
                    string actual = expected ? "invalid" : "valid";
                    sb.AppendLine("Value passed to IsSemanticVersioningCompatible was " +
                        "unexpectedly deemed " + actual + ". Input was: " + input);
                }
            }

            Assert.IsTrue(sb.Length == 0, sb.ToString());
        }
    }
}