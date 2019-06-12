using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    public class WhereComparerTests
    {
        private static readonly Uri Uri1 = new Uri("https://github.com/microsoft/sarif-sdk/blob/master/src/Sarif/Errors.cs");
        private static readonly Uri Uri2 = new Uri("https://github.com/microsoft/sarif-sdk/blob/master/src/Sarif/Notes.cs");

        private static readonly Region SampleRegion = new Region() { StartLine = 10, StartColumn = 5, EndLine = 11, EndColumn = 9 };

        private static readonly ArtifactLocation ArtifactLocationUri1 = new ArtifactLocation() { Uri = Uri1 };
        private static readonly ArtifactLocation ArtifactLocationUri2 = new ArtifactLocation() { Uri = Uri2 };

        private static readonly LogicalLocation LogicalLocation1 = new LogicalLocation() { FullyQualifiedName = "Namespace1.Class1.Method1" };
        private static readonly LogicalLocation LogicalLocation2 = new LogicalLocation() { FullyQualifiedName = "Namespace1.Class1.Method2" };

        [Fact]
        public void WhereComparer_Region()
        {
            // Reminder: CompareTo less than zero means left sorts before right.
            Assert.True(5.CompareTo(10) < 0);

            Region left = new Region(SampleRegion);
            Region right;

            // Equal
            right = new Region(left);
            Assert.True(WhereComparer.CompareTo(left, right) == 0);

            // Right StartLine > Left StartLine
            right = new Region(left) { StartLine = left.StartLine + 1 };
            Assert.True(WhereComparer.CompareTo(left, right) < 0);

            // Right StartLine > Left StartLine, lower precedence stuff less
            right = new Region(left) { StartLine = left.StartLine + 1, StartColumn = left.StartColumn - 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            Assert.True(WhereComparer.CompareTo(left, right) < 0);

            // Right StartColumn > Left StartColumn
            right = new Region(left) { StartColumn = left.StartColumn + 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            Assert.True(WhereComparer.CompareTo(left, right) < 0);

            // Right EndLine > Left EndLine
            right = new Region(left) { EndLine = left.EndLine + 1, EndColumn = left.EndColumn - 1 };
            Assert.True(WhereComparer.CompareTo(left, right) < 0);

            // Right EndColumn > Left EndColumn
            right = new Region(left) { EndColumn = left.EndColumn + 1 };
            Assert.True(WhereComparer.CompareTo(left, right) < 0);

            left.ByteOffset = 100;
            left.ByteLength = 10;

            // Right Offset bigger, supercedes others
            right = new Region(left) { ByteOffset = left.ByteOffset + 1, ByteLength = left.ByteLength - 1, StartLine = left.StartLine - 1, StartColumn = left.StartColumn - 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            Assert.True(WhereComparer.CompareTo(left, right) < 0);

            // Right Length bigger
            right = new Region(left) { ByteLength = left.ByteLength + 1, StartLine = left.StartLine - 1, StartColumn = left.StartColumn - 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            Assert.True(WhereComparer.CompareTo(left, right) < 0);

            left.ByteOffset = -1;
            left.ByteLength = -1;
            left.CharOffset = 100;
            left.CharLength = 10;

            // Right Offset bigger, supercedes others
            right = new Region(left) { CharOffset = left.CharOffset + 1, CharLength = left.CharLength - 1, StartLine = left.StartLine - 1, StartColumn = left.StartColumn - 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            Assert.True(WhereComparer.CompareTo(left, right) < 0);

            // Right Length bigger
            right = new Region(left) { CharLength = left.CharLength + 1, StartLine = left.StartLine - 1, StartColumn = left.StartColumn - 1, EndLine = left.EndLine - 1, EndColumn = left.EndColumn - 1 };
            Assert.True(WhereComparer.CompareTo(left, right) < 0);
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

            // Equal; run not needed if no index provided
            Assert.True(WhereComparer.CompareTo(left, null, left, null) == 0);
            Assert.True(WhereComparer.CompareTo(left, null, right, null) < 0);

            // Equal; run not used if Uri on item itself
            left.Index = 1;
            right.Index = 2;
            Assert.True(WhereComparer.CompareTo(left, null, left, null) == 0);
            Assert.True(WhereComparer.CompareTo(left, null, right, null) < 0);

            // Null sorts first if Artifact can't be retrieved
            left.Uri = null;
            Assert.True(WhereComparer.CompareTo(left, null, right, null) < 0);

            // Null sorts first if Artifact index out of range
            left.Index = 100;
            Assert.True(WhereComparer.CompareTo(left, run, right, run) < 0);

            // Uris retrieved correctly if indices value
            left = new ArtifactLocation() { Index = 1 };
            right = new ArtifactLocation() { Index = 2 };
            Assert.True(WhereComparer.CompareTo(left, run, left, run) == 0);
            Assert.True(WhereComparer.CompareTo(left, run, right, run) < 0);
        }

        [Fact]
        public void WhereComparer_PhysicalLocation()
        {
            PhysicalLocation left = new PhysicalLocation()
            {
                ArtifactLocation = new ArtifactLocation() { Uri = Uri1 }
            };

            PhysicalLocation right = new PhysicalLocation()
            {
                ArtifactLocation = new ArtifactLocation() { Uri = Uri2 }
            };

            // Sort by Uri only even if Region null
            Assert.True(WhereComparer.CompareTo(left, null, left, null) == 0);
            Assert.True(WhereComparer.CompareTo(left, null, right, null) < 0);

            left.Region = SampleRegion;
            PhysicalLocation leftLater = new PhysicalLocation(left) { Region = new Region(SampleRegion) { StartLine = SampleRegion.StartLine + 1 } };

            // Sort by Region if Uris match
            Assert.True(WhereComparer.CompareTo(left, null, leftLater, null) < 0);

            // Uri higher precedence than Region
            right.Region = SampleRegion;
            Assert.True(WhereComparer.CompareTo(leftLater, null, right, null) < 0);
        }

        [Fact]
        public void WhereComparer_LogicalLocation()
        {
            // Null sorts first
            Assert.True(WhereComparer.CompareTo(null, null, LogicalLocation1, null) < 0);

            // Tie
            Assert.True(WhereComparer.CompareTo(LogicalLocation1, null, LogicalLocation1, null) == 0);

            // Sort by Fully Qualified Path
            Assert.True(WhereComparer.CompareTo(LogicalLocation2, null, LogicalLocation1, null) > 0);
        }

        [Fact]
        public void WhereComparer_ListOfLogicalLocation()
        {
            List<LogicalLocation> empty = new List<LogicalLocation>();

            List<LogicalLocation> left = new List<LogicalLocation>()
            {
                LogicalLocation1
            };

            List<LogicalLocation> right = new List<LogicalLocation>()
            {
                LogicalLocation1,
                LogicalLocation2
            };

            // Null sorts first
            Assert.True(WhereComparer.CompareTo(null, null, empty, null) < 0);

            // Empty before anything
            Assert.True(WhereComparer.CompareTo(empty, null, left, null) < 0);

            // Tie
            Assert.True(WhereComparer.CompareTo(left, null, left, null) == 0);

            // Longer wins
            Assert.True(WhereComparer.CompareTo(left, null, right, null) < 0);

            // Later location wins over longer list
            List<LogicalLocation> three = new List<LogicalLocation>() { LogicalLocation2 };
            Assert.True(WhereComparer.CompareTo(right, null, three, null) < 0);
        }
    }
}
