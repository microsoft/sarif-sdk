// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class SerializedPropertyInfoTests
    {
        [Fact]
        public void SerializedPropertyInfo_ComparerTests()
        {
            Guid testGuid = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;

            var testCases = new[]
            {
                new
                {
                    Left = new SerializedPropertyInfo(null, isString:false),
                    Right = new SerializedPropertyInfo(null, isString:false),
                    ExpectedEqualsResult = true,
                    ExpectedCompareResult = 0
                },
                new
                {
                    Left = new SerializedPropertyInfo(null, isString:false),
                    Right = new SerializedPropertyInfo(null, isString:true),
                    ExpectedEqualsResult = false,
                    ExpectedCompareResult = -1
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(true), isString:false),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(true), isString:false),
                    ExpectedEqualsResult = true,
                    ExpectedCompareResult = 0
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(true), isString:false),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(false), isString:false),
                    ExpectedEqualsResult = false,
                    ExpectedCompareResult = 1
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(10), isString:false),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(10), isString:false),
                    ExpectedEqualsResult = true,
                    ExpectedCompareResult = 0
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(-1), isString:false),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(-10), isString:false),
                    ExpectedEqualsResult = false,
                    ExpectedCompareResult = -1 // "-1" < "-10"
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject("abc"), isString:true),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject("abc"), isString:true),
                    ExpectedEqualsResult = true,
                    ExpectedCompareResult = 0
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject("abc"), isString:true),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject("cba"), isString:true),
                    ExpectedEqualsResult = false,
                    ExpectedCompareResult = -1
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(testGuid), isString:true),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(testGuid), isString:true),
                    ExpectedEqualsResult = true,
                    ExpectedCompareResult = 0
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(testGuid), isString:true),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(Guid.Empty), isString:true),
                    ExpectedEqualsResult = false,
                    ExpectedCompareResult = 1
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(now), isString:true),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(now), isString:true),
                    ExpectedEqualsResult = true,
                    ExpectedCompareResult = 0
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(now), isString:true),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(default(DateTime)), isString:true),
                    ExpectedEqualsResult = false,
                    ExpectedCompareResult = 1
                },
                new
                {
                    // "99" compares "\"99\""
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(99), isString:false),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject("99"), isString:true),
                    ExpectedEqualsResult = false,
                    ExpectedCompareResult = 1
                },
                new
                {
                    // serialzed string: "[\"a\",\"b\",\"c\"]"
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(new[] { "a", "b", "c" }), isString:false),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(new[] { "a", "b", "c" }), isString:false),
                    ExpectedEqualsResult = true,
                    ExpectedCompareResult = 0
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(new[] { "a", "b", "c" }), isString:false),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(new[] { "c", "b", "a" }), isString:false),
                    ExpectedEqualsResult = false,
                    ExpectedCompareResult = -1
                },
                new
                {
                    // serialzed string: "{\"a\":1,\"b\":true}"
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(new { a = 1, b = true }), isString:false),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(new { a = 1, b = true }), isString:false),
                    ExpectedEqualsResult = true,
                    ExpectedCompareResult = 0
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(new { a = 1, b = true }), isString:false),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(new { a = 1, b = false }), isString:false),
                    ExpectedEqualsResult = false,
                    ExpectedCompareResult = 1
                },
                new
                {
                    Left = new SerializedPropertyInfo(JsonConvert.SerializeObject(new { a = 1, b = true }), isString:false),
                    Right = new SerializedPropertyInfo(JsonConvert.SerializeObject(new { b = false, a = 1 }), isString:false),
                    ExpectedEqualsResult = false,
                    ExpectedCompareResult = -1
                },
            };

            foreach (var testCase in testCases)
            {
                bool equalsResult = SerializedPropertyInfo.ValueComparer.Equals(testCase.Left, testCase.Right);
                equalsResult.Should().Be(
                    testCase.ExpectedEqualsResult,
                    $"left: {testCase.Left.SerializedValue} right: {testCase.Right.SerializedValue}");

                int compareResult = SerializedPropertyInfo.Comparer.Compare(testCase.Left, testCase.Right);
                compareResult.Should().Be(
                    testCase.ExpectedCompareResult,
                    $"left: {testCase.Left.SerializedValue} right: {testCase.Right.SerializedValue}");
            }
        }
    }
}
