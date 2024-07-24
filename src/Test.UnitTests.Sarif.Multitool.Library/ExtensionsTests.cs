// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.Text;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ExtensionsTests
    {
        [Fact]
        public void Extensions_Validate_RefersToDriver()
        {
            var sb = new StringBuilder();

            foreach (ToolComponentReferenceTestCase item in s_toolComponentReferenceTestCases)
            {
                bool actualOutput = item.ToolComponentReference.RefersToDriver(item.DriverGuid);

                if (actualOutput != item.ExpectedOutput)
                {
                    sb.AppendLine($"    Input: {item.ToolComponentReference.Index} {item.ToolComponentReference.Guid?.ToString(SarifConstants.GuidFormat) ?? "null"} {item.DriverGuid?.ToString(SarifConstants.GuidFormat) ?? "null"} Expected: {item.ExpectedOutput} Actual: {actualOutput}");
                }
            }

            sb.Length.Should().Be(0,
                $"all test cases should pass, but the following test cases failed:\n{sb.ToString()}");
        }

        private class ToolComponentReferenceTestCase
        {
            public ToolComponentReferenceTestCase(int index, Guid? toolGuid, Guid? driverGuid, bool expectedOutput)
            {
                ToolComponentReference = new ToolComponentReference
                {
                    Index = index,
                    Guid = toolGuid
                };

                DriverGuid = driverGuid;
                ExpectedOutput = expectedOutput;
            }

            public ToolComponentReference ToolComponentReference { get; }
            public Guid? DriverGuid { get; }
            public bool ExpectedOutput { get; }
        }

        private static readonly ReadOnlyCollection<ToolComponentReferenceTestCase> s_toolComponentReferenceTestCases =
            new ReadOnlyCollection<ToolComponentReferenceTestCase>(new[] {
                new ToolComponentReferenceTestCase(-1, null, null, true),
                new ToolComponentReferenceTestCase(-1, Guid.Parse("774707BC-6949-4DB5-826D-9FC0E38BFDEE"), Guid.Parse("774707BC-6949-4DB5-826D-9FC0E38BFDEE"), true),
                new ToolComponentReferenceTestCase(-1, Guid.Parse("774707BC-6949-4DB5-826D-9FC0E38BFDEF"), Guid.Parse("774707BC-6949-4DB5-826D-9FC0E38BFDEE"), false),
                new ToolComponentReferenceTestCase(2, Guid.Parse("774707BC-6949-4DB5-826D-9FC0E38BFDEE"), Guid.Parse("774707BC-6949-4DB5-826D-9FC0E38BFDEE"), false)
        });
    }
}
