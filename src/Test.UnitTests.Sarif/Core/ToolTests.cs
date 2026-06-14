// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Core
{
    public class ToolTests
    {
        private class DottedQuadFileVersionTestCase
        {
            public DottedQuadFileVersionTestCase(string input, string expectedOutput)
            {
                Input = input;
                ExpectedOutput = expectedOutput;
            }

            public string Input { get; }
            public string ExpectedOutput { get; }
        }

        private static readonly ReadOnlyCollection<DottedQuadFileVersionTestCase> s_dottedQuadFileVersionTestCases =
            new ReadOnlyCollection<DottedQuadFileVersionTestCase>(new DottedQuadFileVersionTestCase[]
            {
                new DottedQuadFileVersionTestCase(
                    input: "",
                    expectedOutput: null),

                new DottedQuadFileVersionTestCase(
                    input: "11.22.33",
                    expectedOutput: null),

                new DottedQuadFileVersionTestCase(
                    input: "11.22.33.xx",
                    expectedOutput: null),

                new DottedQuadFileVersionTestCase(
                    input: "11.22..44",
                    expectedOutput: null),

                new DottedQuadFileVersionTestCase(
                    input: "prefix 11.22.33.44",
                    expectedOutput: null),

                new DottedQuadFileVersionTestCase(
                    input: "11.22.33.44",
                    expectedOutput: "11.22.33.44"),

                new DottedQuadFileVersionTestCase(
                    input: "11.22.33.44 suffix",
                    expectedOutput: "11.22.33.44"),
            });

        [Fact]
        public void Tool_ParseFileVersion_ExtractsDottedQuadFileVersion()
        {
            var sb = new StringBuilder();

            foreach (DottedQuadFileVersionTestCase testCase in s_dottedQuadFileVersionTestCases)
            {
                string actualOutput = Tool.ParseFileVersion(testCase.Input);

                bool succeeded = (testCase.ExpectedOutput == null && actualOutput == null)
                    || (actualOutput?.Equals(testCase.ExpectedOutput, StringComparison.Ordinal) == true);

                if (!succeeded)
                {
                    sb.AppendLine($"    Input: {Utilities.SafeFormat(testCase.Input)} Expected: {Utilities.SafeFormat(testCase.ExpectedOutput)} Actual: {Utilities.SafeFormat(actualOutput)}");
                }
            }

            sb.Length.Should().Be(0,
                $"all test cases should pass, but the following test cases failed:\n{sb.ToString()}");
        }

        [Fact]
        public void Tool_CreateFromAssemblyData_DoesNotThrowWhenAssemblyLocationIsEmpty()
        {
            // A dynamic (in-memory) assembly reports an empty Location, exactly like a single-file
            // publish. CreateFromAssemblyData must not crash in FileVersionInfo.GetVersionInfo("")
            // and must derive the tool name from the assembly name instead of the (empty) path.
            Assembly assembly = BuildDynamicAssembly("DynamicToolAssembly", new Version(5, 0, 7, 0));
            assembly.Location.Should().BeEmpty();

            Tool tool = Tool.CreateFromAssemblyData(assembly);

            tool.Driver.Name.Should().Be("DynamicToolAssembly");
        }

        [Fact]
        public void Tool_CreateFromAssemblyData_FallsBackToAssemblyVersionAttributesUnderSingleFile()
        {
            // With no on-disk version resource available, identity comes from the assembly's own
            // version attributes.
            Assembly assembly = BuildDynamicAssembly(
                "DynamicToolAssembly",
                new Version(5, 0, 7, 0),
                informationalVersion: "5.0.7+deadbeef",
                company: "Microsoft",
                product: "Microsoft SARIF SDK");

            Tool tool = Tool.CreateFromAssemblyData(assembly);

            tool.Driver.Version.Should().Be("5.0.7.0");
            tool.Driver.SemanticVersion.Should().Be("5.0.7+deadbeef");
            tool.Driver.Organization.Should().Be("Microsoft");
            tool.Driver.Product.Should().Be("Microsoft SARIF SDK");
        }

        private static Assembly BuildDynamicAssembly(
            string name,
            Version version,
            string informationalVersion = null,
            string company = null,
            string product = null)
        {
            var assemblyName = new AssemblyName(name) { Version = version };
            AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            AddStringAttribute(builder, typeof(AssemblyFileVersionAttribute), version.ToString());
            if (informationalVersion != null) { AddStringAttribute(builder, typeof(AssemblyInformationalVersionAttribute), informationalVersion); }
            if (company != null) { AddStringAttribute(builder, typeof(AssemblyCompanyAttribute), company); }
            if (product != null) { AddStringAttribute(builder, typeof(AssemblyProductAttribute), product); }

            return builder;
        }

        private static void AddStringAttribute(AssemblyBuilder builder, Type attributeType, string value)
        {
            ConstructorInfo ctor = attributeType.GetConstructor(new[] { typeof(string) });
            builder.SetCustomAttribute(new CustomAttributeBuilder(ctor, new object[] { value }));
        }
    }
}
