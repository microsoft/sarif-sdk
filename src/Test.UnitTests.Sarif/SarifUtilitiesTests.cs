// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Castle.DynamicProxy.Generators;

using FluentAssertions;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifUtilitiesTests
    {
        [Fact]
        public void ConvertToSchemaUriTestV100()
        {
            Uri uri = SarifVersion.OneZeroZero.ConvertToSchemaUri();
            Assert.Equal(SarifUtilities.SarifSchemaUriBase + SarifUtilities.V1_0_0 + ".json", uri.ToString());
        }

        [Fact]
        public void ConvertToSchemaUriTestVCurrent()
        {
            Uri uri = SarifVersion.Current.ConvertToSchemaUri();
            Assert.Equal(SarifUtilities.SarifSchemaUri, uri.ToString());
        }

        private class GetEncodingFromNameTestCase
        {
            internal string TestCaseName { get; }
            internal string EncodingName { get; }
            internal Encoding ExpectedEncoding { get; }

            internal GetEncodingFromNameTestCase(
                string testCaseName,
                string encodingName,
                Encoding expectedEncoding)
            {
                TestCaseName = testCaseName;
                EncodingName = encodingName;
                ExpectedEncoding = expectedEncoding;
            }
        }

        private static readonly IReadOnlyCollection<GetEncodingFromNameTestCase> s_getEncodingFromNameTestCases = new ReadOnlyCollection<GetEncodingFromNameTestCase>(
            new List<GetEncodingFromNameTestCase>
            {
                new GetEncodingFromNameTestCase(
                    testCaseName: "Valid encoding name",
                    encodingName: "UTF-8",
                    expectedEncoding: Encoding.UTF8),
                new GetEncodingFromNameTestCase(
                    testCaseName: "Invalid encoding name",
                    encodingName: "INVALID",
                    expectedEncoding: null),
                new GetEncodingFromNameTestCase(
                    testCaseName: "Null encoding name",
                    encodingName: null,
                    expectedEncoding: null)
            });

        [Fact]
        public void GetEncodingFromNameProducesExpectedEncoding()
        {
            var sb = new StringBuilder();
            foreach (GetEncodingFromNameTestCase testCase in s_getEncodingFromNameTestCases)
            {
                Encoding actualEncoding = SarifUtilities.GetEncodingFromName(testCase.EncodingName);
                if (actualEncoding != testCase.ExpectedEncoding)
                {
                    sb.AppendLine($"    {testCase.TestCaseName}");
                }
            }

            sb.Length.Should().Be(0,
                $"expected all test cases to pass, but the following test cases failed:\n{sb}");
        }
    }
}
