// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Baseline;
using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Baseline
{
    public class WhereComparerTests
    {
        private static readonly Uri Uri1 = new Uri("https://github.com/microsoft/sarif-sdk/blob/master/src/Sarif/Errors.cs");
        private static readonly Uri Uri2 = new Uri("https://github.com/microsoft/sarif-sdk/blob/master/src/Sarif/Notes.cs");

        private static readonly Region SampleRegion = new Region { StartLine = 10, StartColumn = 5, EndLine = 11, EndColumn = 9 };

        private static readonly ArtifactLocation ArtifactLocationUri1 = new ArtifactLocation { Uri = Uri1 };
        private static readonly ArtifactLocation ArtifactLocationUri2 = new ArtifactLocation { Uri = Uri2 };

        private static readonly LogicalLocation LogicalLocation1 = new LogicalLocation { FullyQualifiedName = "Namespace1.Class1.Method1" };
        private static readonly LogicalLocation LogicalLocation2 = new LogicalLocation { FullyQualifiedName = "Namespace1.Class1.Method2" };

        [Fact]
        public void WhereComparer_Region()
        {
            // Reminder: CompareTo less than zero means left sorts before right.
            5.CompareTo(10).Should().BeLessThan(0);

            Region left = new Region(SampleRegion);
            Region right;

            // Equal
            right = new Region(left);
            WhereComparer.CompareTo(left, right).Should().Be(0);

            // Right StartLine > Left StartLine
            right = new Region(left) { StartLine = left.StartLine + 1 };
            WhereComparer.CompareTo(left, right).Should().BeLessThan(0);

            // Right StartLine > Left StartLine, lower precedence stuff less
            right = new Region(left) { StartLine = left.StartLine + 1, StartColumn = left.StartColumn - 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            WhereComparer.CompareTo(left, right).Should().BeLessThan(0);

            // Right StartColumn > Left StartColumn
            right = new Region(left) { StartColumn = left.StartColumn + 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            WhereComparer.CompareTo(left, right).Should().BeLessThan(0);

            // Right EndLine > Left EndLine
            right = new Region(left) { EndLine = left.EndLine + 1, EndColumn = left.EndColumn - 1 };
            WhereComparer.CompareTo(left, right).Should().BeLessThan(0);

            // Right EndColumn > Left EndColumn
            right = new Region(left) { EndColumn = left.EndColumn + 1 };
            WhereComparer.CompareTo(left, right).Should().BeLessThan(0);

            left.ByteOffset = 100;
            left.ByteLength = 10;

            // Right Offset bigger, supersedes others
            right = new Region(left) { ByteOffset = left.ByteOffset + 1, ByteLength = left.ByteLength - 1, StartLine = left.StartLine - 1, StartColumn = left.StartColumn - 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            WhereComparer.CompareTo(left, right).Should().BeLessThan(0);

            // Right Length bigger
            right = new Region(left) { ByteLength = left.ByteLength + 1, StartLine = left.StartLine - 1, StartColumn = left.StartColumn - 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            WhereComparer.CompareTo(left, right).Should().BeLessThan(0);

            left.ByteOffset = -1;
            left.ByteLength = -1;
            left.CharOffset = 100;
            left.CharLength = 10;

            // Right Offset bigger, supersedes others
            right = new Region(left) { CharOffset = left.CharOffset + 1, CharLength = left.CharLength - 1, StartLine = left.StartLine - 1, StartColumn = left.StartColumn - 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            WhereComparer.CompareTo(left, right).Should().BeLessThan(0);

            // Right Length bigger
            right = new Region(left) { CharLength = left.CharLength + 1, StartLine = left.StartLine - 1, StartColumn = left.StartColumn - 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            WhereComparer.CompareTo(left, right).Should().BeLessThan(0);

            // EndLine default resolution
            left = new Region() { StartLine = 10, StartColumn = 1, EndLine = 10, EndColumn = 55 };
            right = new Region(left) { EndLine = -1 };
            WhereComparer.CompareTo(left, right).Should().Be(0);
            WhereComparer.CompareTo(right, left).Should().Be(0);

            // Left.EndLine greater than Right defaulted EndLine
            left.EndLine += 1;
            WhereComparer.CompareTo(left, right).Should().BeGreaterThan(0);
            WhereComparer.CompareTo(right, left).Should().BeLessThan(0);

            // StartColumn default resolution
            left = new Region() { StartLine = 10, StartColumn = 1, EndLine = 10, EndColumn = 55 };
            right = new Region(left) { StartColumn = -1 };
            WhereComparer.CompareTo(left, right).Should().Be(0);
            WhereComparer.CompareTo(right, left).Should().Be(0);

            // Left.StartColumn greater than Right defaulted EndColumn
            left.StartColumn = 2;
            WhereComparer.CompareTo(left, right).Should().BeGreaterThan(0);
            WhereComparer.CompareTo(right, left).Should().BeLessThan(0);
        }

        [Fact]
        public void WhereComparer_ArtifactLocation()
        {
            Run run = new Run();
            run.Artifacts = new List<Artifact>();

            run.Artifacts.Add(new Artifact() { Location = new ArtifactLocation() });
            run.Artifacts.Add(new Artifact() { Location = ArtifactLocationUri1 });
            run.Artifacts.Add(new Artifact() { Location = ArtifactLocationUri2 });

            ArtifactLocation left = new ArtifactLocation(ArtifactLocationUri1);
            ArtifactLocation right = new ArtifactLocation(ArtifactLocationUri2);

            // Equal; run not needed if no index provided.
            WhereComparer.CompareTo(left, null, left, null).Should().Be(0);
            WhereComparer.CompareTo(left, null, right, null).Should().BeLessThan(0);

            // Equal; run not used if Uri on item itself.
            left.Index = 1;
            right.Index = 2;
            WhereComparer.CompareTo(left, null, left, null).Should().Be(0);
            WhereComparer.CompareTo(left, null, right, null).Should().BeLessThan(0);

            // Null sorts first if Artifact can't be retrieved
            left.Uri = null;
            WhereComparer.CompareTo(left, null, right, null).Should().BeLessThan(0);

            // Null sorts first if Artifact index out of range.
            left.Index = 100;
            WhereComparer.CompareTo(left, run, right, run).Should().BeLessThan(0);

            // Uris retrieved correctly if indices are valid.
            left = new ArtifactLocation() { Index = 1 };
            right = new ArtifactLocation() { Index = 2 };
            WhereComparer.CompareTo(left, run, left, run).Should().Be(0);
            WhereComparer.CompareTo(left, run, right, run).Should().BeLessThan(0);
        }

        [Fact]
        public void WhereComparer_PhysicalLocation()
        {
            PhysicalLocation left = new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation() { Uri = Uri1 }
            };

            PhysicalLocation right = new PhysicalLocation
            {
                ArtifactLocation = new ArtifactLocation { Uri = Uri2 }
            };

            // Sort by Uri only even if Region null
            WhereComparer.CompareTo(left, null, left, null).Should().Be(0);
            WhereComparer.CompareTo(left, null, right, null).Should().BeLessThan(0);

            left.Region = SampleRegion;
            PhysicalLocation leftLater = new PhysicalLocation(left) { Region = new Region(SampleRegion) { StartLine = SampleRegion.StartLine + 1 } };

            // Sort by Region if Uris match
            WhereComparer.CompareTo(left, null, leftLater, null).Should().BeLessThan(0);

            // Uri higher precedence than Region
            right.Region = SampleRegion;
            WhereComparer.CompareTo(leftLater, null, right, null).Should().BeLessThan(0);
        }

        [Fact]
        public void WhereComparer_LogicalLocation()
        {
            // Null sorts first.
            WhereComparer.CompareTo(null, null, LogicalLocation1, null).Should().BeLessThan(0);

            // Tie.
            WhereComparer.CompareTo(LogicalLocation1, null, LogicalLocation1, null).Should().Be(0);

            // Sort by Fully Qualified Path.
            WhereComparer.CompareTo(LogicalLocation2, null, LogicalLocation1, null).Should().BeGreaterThan(0);
        }

        [Fact]
        public void WhereComparer_ListOfLogicalLocation()
        {
            List<LogicalLocation> empty = new List<LogicalLocation>();

            List<LogicalLocation> left = new List<LogicalLocation>
            {
                LogicalLocation1
            };

            List<LogicalLocation> right = new List<LogicalLocation>
            {
                LogicalLocation1,
                LogicalLocation2
            };

            // Null sorts first.
            WhereComparer.CompareTo(null, null, empty, null).Should().BeLessThan(0);

            // Empty before anything.
            WhereComparer.CompareTo(empty, null, left, null).Should().BeLessThan(0);

            // Tie
            WhereComparer.CompareTo(left, null, left, null).Should().Be(0);

            // Longer wins
            WhereComparer.CompareTo(left, null, right, null).Should().BeLessThan(0);

            // Later location wins over longer list
            List<LogicalLocation> three = new List<LogicalLocation> { LogicalLocation2 };
            WhereComparer.CompareTo(right, null, three, null).Should().BeLessThan(0);
        }
    }
}
