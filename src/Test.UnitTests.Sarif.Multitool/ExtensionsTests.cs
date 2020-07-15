// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ExtensionsTests
    {
        [Theory]
        [InlineData(-1, "", "", true)]
        [InlineData(-1, "774707BC-6949-4DB5-826D-9FC0E38BFDEE", "774707BC-6949-4DB5-826D-9FC0E38BFDEE", true)]
        [InlineData(-1, "774707BC-6949-4DB5-826D-9FC0E38BFDEF", "774707BC-6949-4DB5-826D-9FC0E38BFDEE", false)]
        [InlineData(2, "774707BC-6949-4DB5-826D-9FC0E38BFDEF", "774707BC-6949-4DB5-826D-9FC0E38BFDEE", false)]
        public void Extensions_Validate_RefersToDriver(int index, string ruleGuid, string driverGuid, bool expectedOutput)
        {
            var toolComponent = new ToolComponentReference
            {
                Index = index,
                Guid = ruleGuid
            };

            Assert.Equal(expectedOutput, toolComponent.RefersToDriver(driverGuid));
        }
    }
}
