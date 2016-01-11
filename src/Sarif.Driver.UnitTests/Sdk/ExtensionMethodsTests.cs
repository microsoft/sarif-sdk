// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class ExtensionMethodsTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ExtensionMethodsTests(ITestOutputHelper output)
        {
            _testOutputHelper = output;
        }

        [Fact]
        public void GetFirstSentenceTests()
        {
            // TODO convert this to [Theory]
            var sb = new StringBuilder();

            var tests = new[]
            {
                new Tuple<string, string>("first (foo.dll) sentence. more text", "first (foo.dll) sentence."),
                new Tuple<string, string>("first 'foo.dll' sentence. more text", "first 'foo.dll' sentence."),
                new Tuple<string, string>("first (') sentence. more text", "first (') sentence."),
                new Tuple<string, string>("first '(' sentence. more text", "first '(' sentence."),
                new Tuple<string, string>("first sentence\n more text", "first sentence"),
                new Tuple<string, string>("first sentence\r more text", "first sentence"),
            };

            foreach (Tuple<string, string> test in tests)
            {
                string input = test.Item1;
                string expected = test.Item2;
                string actual = ExtensionMethods.GetFirstSentence(input);

                if (expected != actual)
                {
                    sb.AppendLine(string.Format("Input value: '{0}'", input));
                    sb.AppendLine(string.Format("Expected: '{0}'", expected));
                    sb.AppendLine(string.Format("Actual: '{0}'", actual));
                    sb.AppendLine();
                }
            }

            if (sb.Length > 0)
            {
                _testOutputHelper.WriteLine(sb.ToString());
            }

            Assert.Equal(0, sb.Length);
        }
    }
}
