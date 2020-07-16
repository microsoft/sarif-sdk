// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ExtensionsTests
    {
        [Fact]
        public void Extensions_Validate_RefersToDriver()
        {
            foreach (ToolComponentReferenceTestCase item in s_toolComponentReferenceTestCases)
            {
                // Act
                bool actualOutput = item.ToolComponentReference.RefersToDriver(item.DriverGuid);

                // Assert
                Assert.Equal(item.ExpectedOutput, actualOutput);
            }
        }

        private class ToolComponentReferenceTestCase
        {
            public ToolComponentReferenceTestCase(int index, string toolGuid, string driverGuid, bool expectedOutput)
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
            public string DriverGuid { get; }
            public bool ExpectedOutput { get; }
        }

        private static readonly ReadOnlyCollection<ToolComponentReferenceTestCase> s_toolComponentReferenceTestCases =
            new ReadOnlyCollection<ToolComponentReferenceTestCase>(new[] {
                new ToolComponentReferenceTestCase(-1, null, null, true),
                new ToolComponentReferenceTestCase(-1, "774707BC-6949-4DB5-826D-9FC0E38BFDEE", "774707BC-6949-4DB5-826D-9FC0E38BFDEE", true),
                new ToolComponentReferenceTestCase(-1, "774707BC-6949-4DB5-826D-9FC0E38BFDEF", "774707BC-6949-4DB5-826D-9FC0E38BFDEE", false),
                new ToolComponentReferenceTestCase(2, "774707BC-6949-4DB5-826D-9FC0E38BFDEE", "774707BC-6949-4DB5-826D-9FC0E38BFDEE", false)
        });
    }
}
