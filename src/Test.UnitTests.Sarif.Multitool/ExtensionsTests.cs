// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ExtensionsTests
    {
        [Fact]
        public void Extensions_Validate_RefersToDriver()
        {
            foreach (Tuple<ToolComponentReference, string, bool> item in ToolComponentTestList)
            {
                Assert.Equal(item.Item3, item.Item1.RefersToDriver(item.Item2));
            }

        }

        public Tuple<ToolComponentReference, string, bool>[] ToolComponentTestList = new[]
        {
            new Tuple<ToolComponentReference, string, bool>(new ToolComponentReference{Index = -1, Guid = "" }, "", true),
            new Tuple<ToolComponentReference, string, bool>(new ToolComponentReference{Index = -1, Guid = "774707BC-6949-4DB5-826D-9FC0E38BFDEE" }, "774707BC-6949-4DB5-826D-9FC0E38BFDEE", true),
            new Tuple<ToolComponentReference, string, bool>(new ToolComponentReference{Index = -1, Guid = "774707BC-6949-4DB5-826D-9FC0E38BFDEF" }, "774707BC-6949-4DB5-826D-9FC0E38BFDEE", false),
            new Tuple<ToolComponentReference, string, bool>(new ToolComponentReference{Index = 2, Guid = "774707BC-6949-4DB5-826D-9FC0E38BFDEE" }, "774707BC-6949-4DB5-826D-9FC0E38BFDEE", false),
        };
    }
}
