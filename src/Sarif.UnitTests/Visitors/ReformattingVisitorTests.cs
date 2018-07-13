// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.ObjectModel;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class ReformattingVisitorTests
    {
        //                                  0          10          20 
        //                                  01234567 8 901234 5 678901 2 345
        private const string s_testText = "line1\r\nline2\r\nline3\r\nline4";

        private static ReadOnlyCollection<Tuple<Region, string>> s_testCases =
            new ReadOnlyCollection<Tuple<Region, string>>(new Tuple<Region, string>[]
            {
                // Regions specified only by start line
                new Tuple<Region, string>(new Region() { StartLine = 1 }, "line1"),
                new Tuple<Region, string>(new Region() { StartLine = 2 }, "line2"),
                new Tuple<Region, string>(new Region() { StartLine = 3 }, "line3"),
                new Tuple<Region, string>(new Region() { StartLine = 4 }, "line4"),
                // Multiline regions, should only return first line
                new Tuple<Region, string>(new Region() { StartLine = 1, EndLine = 4 }, "line1"),
                new Tuple<Region, string>(new Region() { StartLine = 2, EndLine = 3 }, "line2"),
                new Tuple<Region, string>(new Region() { StartLine = 3, EndLine = 4 }, "line3"),
                new Tuple<Region, string>(new Region() { StartLine = 4, EndLine = 4 }, "line4"),
                // Char offsets. Should return full line, no matter what subset of line is referenced
                new Tuple<Region, string>(new Region() { CharOffset = 0, CharLength = 2 }, "line1"),
                new Tuple<Region, string>(new Region() { CharOffset = 9, CharLength = 1 }, "line2"),
                new Tuple<Region, string>(new Region() { CharOffset = 16, CharLength = 2 }, "line3"),
                new Tuple<Region, string>(new Region() { CharOffset = 22, CharLength = 4 }, "line4"),
                // Binary regions. No return value expected.
                new Tuple<Region, string>(new Region() { ByteOffset = 20, ByteLength = 5 }, null),
                // No explicit values is implicitly a binary region with insertion point at beginning of file
                // This may change soon. https://github.com/oasis-tcs/sarif-spec/issues/201
                new Tuple<Region, string>(new Region() { }, null),
            });

        [Fact]        
        public void ReformattingVisitor_InsertsCodeSnippets()
        {
            foreach (Tuple<Region, string> tuple in s_testCases)
            {
                Region region = tuple.Item1;
                string expectedResult = tuple.Item2;
                ReformattingVisitor.GetCompleteFirstLineAssociatedWithRegion(tuple.Item1, s_testText);

                if (expectedResult != null)
                {
                    region.Snippet.Should().NotBeNull();
                    region.Snippet.Text.Should().Be(expectedResult);
                }
                else
                {
                    region.Snippet.Should().BeNull();
                }
            }
        }
    }
}
