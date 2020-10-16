// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifExtensionsTests
    {
        [Fact]
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

            Assert.True(sb.Length == 0, sb.ToString());
        }

        [Fact]
        public void SarifExtensions_Result_GetMessageText_MessageText()
        {
            var result = new Result()
            {
                Message = new Message()
                {
                    Text = "The quick brown fox jumps over the lazy dog."
                }
            };

            string expected = "The quick brown fox jumps over the lazy dog.";
            string actual = result.GetMessageText(null);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SarifExtensions_Result_GetMessageText_WithArguments()
        {
            var result = new Result()
            {
                Message = new Message()
                {
                    Arguments = new List<string> { "fox", "dog" },
                    Id = "ruleStr1"
                },
            };

            var rule = new ReportingDescriptor()
            {
                MessageStrings = new Dictionary<string, MultiformatMessageString>()
                {
                    ["ruleStr1"] = new MultiformatMessageString { Text = "The quick brown {0} jumps over the lazy {1}. That {1} sure is lazy!" }
                }
            };

            string expected = "The quick brown fox jumps over the lazy dog. That dog sure is lazy!";
            string actual = result.GetMessageText(rule);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SarifExtensions_Result_GetMessageText_Concise()
        {
            var result = new Result()
            {
                Message = new Message()
                {
                    Arguments = new List<string> { "fox", "dog" },
                    Id = "ruleStr1"
                }
            };

            var rule = new ReportingDescriptor()
            {
                MessageStrings = new Dictionary<string, MultiformatMessageString>()
                {
                    ["ruleStr1"] = new MultiformatMessageString { Text = "The quick brown {0} jumps over the lazy {1}. That {1} sure is lazy!" }
                }
            };

            string expected = "The quick brown fox jumps over the lazy dog.";
            string actual = result.GetMessageText(rule, concise: true);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SarifExtensions_Result_GetMessageText_Concise_Truncated()
        {
            var result = new Result
            {
                Message = new Message
                {
                    Id = "ruleStr1"
                }
            };

            var rule = new ReportingDescriptor
            {
                MessageStrings = new Dictionary<string, MultiformatMessageString>
                {
                    ["ruleStr1"] = new MultiformatMessageString { Text = "First sentence is very long. Second sentence." }
                }
            };

            const string Expected = "First sentence is ve\u2026"; // \u2026 is Unicode "horizontal ellipsis".
            int maxLength = Expected.Length - 1;    // The -1 is for the ellipsis character.
            string actual = result.GetMessageText(rule, concise: true, maxLength);
            Assert.Equal(Expected, actual);
        }
    }
}