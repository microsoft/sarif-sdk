// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifConsolidatorTests
    {
        private static readonly ResourceExtractor Extractor = new ResourceExtractor(typeof(SarifConsolidatorTests));

        // Sample objects to trim.
        // These are properties so they are re-created on each access, to avoid tests interfering with each other.

        private Uri SampleUri => new Uri("src/Program.cs", UriKind.Relative);

        // Region: Remove Endline if EndLine == StartLine; Remove offsets if Line/Column available.
        private Region SampleRegion => new Region() { StartLine = 100, StartColumn = 10, EndLine = 100, EndColumn = 15, ByteOffset = 1024, ByteLength = 5, CharOffset = 1024, CharLength = 5 };
        private Region SampleRegionTrimmed => new Region() { StartLine = 100, StartColumn = 10, EndColumn = 15 };

        // ArtifactLocation: Keep Index only if Index present
        private ArtifactLocation SampleArtifactLocation => new ArtifactLocation() { Index = 5, Uri = SampleUri, UriBaseId = "Root" };
        private ArtifactLocation SampleArtifactLocationTrimmed => new ArtifactLocation() { Index = 5 };

        // LogicalLocation: Keep Index only if Index present
        private LogicalLocation SampleLogicalLocation => new LogicalLocation()
        {
            Index = 10,
            DecoratedName = "Microsoft.Sarif.Class",
            Name = "Class",
            FullyQualifiedName = "Sarif.Sdk.Microsoft.Sarif.Class"
        };

        private LogicalLocation SampleLogicalLocationTrimmed => new LogicalLocation() { Index = 10 };

        // PhysicalLocation: Trim components
        private PhysicalLocation SamplePhysicalLocation => new PhysicalLocation()
        {
            ArtifactLocation = new ArtifactLocation(SampleArtifactLocation),
            Region = new Region(SampleRegion),
            ContextRegion = new Region(SampleRegion)
        };

        private PhysicalLocation SamplePhysicalLocationTrimmed => new PhysicalLocation()
        {
            ArtifactLocation = new ArtifactLocation(SampleArtifactLocationTrimmed),
            Region = new Region(SampleRegionTrimmed),
            ContextRegion = new Region(SampleRegionTrimmed)
        };

        // Location: Remove Id, trim components
        private Location SampleLocation => new Location()
        {
            Id = 5,
            PhysicalLocation = new PhysicalLocation(SamplePhysicalLocation),
            LogicalLocation = new LogicalLocation(SampleLogicalLocation)
        };

        private Location SampleLocationTrimmed => new Location()
        {
            Id = 5,
            PhysicalLocation = new PhysicalLocation(SamplePhysicalLocationTrimmed),
            LogicalLocation = new LogicalLocation(SampleLogicalLocationTrimmed)
        };

        // ThreadFlowLocation: Consolidate in Run.ThreadFlowLocations and reference by Index
        private ThreadFlowLocation SampleThreadFlowLocation => new ThreadFlowLocation() { ExecutionOrder = 3, Module = "Loader" };

        [Fact]
        public void SarifConsolidator_Nulls()
        {
            SarifConsolidator consolidator = new SarifConsolidator(new Run());

            // Null objects handled gracefully (so callers can pass properties which may or may not be set)
            consolidator.Trim((Region)null);
            consolidator.Trim((ArtifactLocation)null);
            consolidator.Trim((PhysicalLocation)null);
            consolidator.Trim((LogicalLocation)null);
            consolidator.Trim((IList<LogicalLocation>)null);
            consolidator.Trim((Location)null);
            consolidator.Trim((IList<Location>)null);
            consolidator.Trim((Result)null);
        }

        [Fact]
        public void SarifConsolidator_Region()
        {
            SarifConsolidator consolidator = new SarifConsolidator(new Run());

            // If line information, offsets removed. If EndLine == StartLine, EndLine removed
            Region r = new Region(SampleRegion);
            Region rExpected = new Region(SampleRegionTrimmed);

            consolidator.Trim(r);
            Assert.True(Region.ValueComparer.Equals(SampleRegionTrimmed, r));

            consolidator.Trim(r);
            Assert.True(Region.ValueComparer.Equals(rExpected, r));


            // Don't remove EndLine when EndLine != StartLine
            r.EndLine = 101;
            rExpected.EndLine = 101;

            consolidator.Trim(r);
            Assert.Equal(101, r.EndLine);
            Assert.True(Region.ValueComparer.Equals(rExpected, r));


            // Don't remove offsets when no line numbers
            r = new Region() { ByteOffset = 1024, ByteLength = 10 };
            rExpected = new Region(r);

            consolidator.Trim(r);
            Assert.Equal(1024, r.ByteOffset);
            Assert.True(Region.ValueComparer.Equals(rExpected, r));

            Region everythingRegion = new Region() { StartLine = 10, StartColumn = 12, EndLine = 13, EndColumn = 15, ByteOffset = 100, ByteLength = 10, CharOffset = 100, CharLength = 10 };

            // Trim to ByteOffset only
            r = new Region(everythingRegion);
            consolidator.RegionComponentsToKeep = RegionComponents.ByteOffsetAndLength;
            consolidator.Trim(r);
            Assert.Equal(0, r.StartLine);
            Assert.Equal(-1, r.CharOffset);
            Assert.Equal(100, r.ByteOffset);

            // Trim to CharOffset only
            r = new Region(everythingRegion);
            consolidator.RegionComponentsToKeep = RegionComponents.CharOffsetAndLength;
            consolidator.Trim(r);
            Assert.Equal(0, r.StartLine);
            Assert.Equal(-1, r.ByteOffset);
            Assert.Equal(100, r.CharOffset);

            // Keep everything
            r = new Region(everythingRegion);
            consolidator.RegionComponentsToKeep = RegionComponents.Full;
            consolidator.Trim(r);
            Assert.True(Region.ValueComparer.Equals(r, everythingRegion));
        }

        [Fact]
        public void SarifConsolidator_ArtifactLocation()
        {
            SarifConsolidator consolidator = new SarifConsolidator(new Run());

            // If Index set, remove Uri and UriBaseId 
            ArtifactLocation a = new ArtifactLocation(SampleArtifactLocation);
            ArtifactLocation aExpected = new ArtifactLocation(SampleArtifactLocationTrimmed);

            consolidator.Trim(a);
            Assert.Null(a.Uri);
            Assert.True(ArtifactLocation.ValueComparer.Equals(aExpected, a));


            // Uri not removed when no Index
            a = new ArtifactLocation() { Uri = SampleUri, UriBaseId = "Root" };
            aExpected = new ArtifactLocation(a);

            consolidator.Trim(a);
            Assert.Equal(SampleUri, a.Uri);
            Assert.True(ArtifactLocation.ValueComparer.Equals(aExpected, a));
        }

        [Fact]
        public void SarifConsolidator_Location()
        {
            SarifConsolidator consolidator = new SarifConsolidator(new Run());

            // Id removed, inner components trimmed
            Location loc = new Location(SampleLocation);
            Location locExpected = new Location(SampleLocationTrimmed);

            consolidator.Trim(loc);
            Assert.True(Location.ValueComparer.Equals(locExpected, loc));
        }

        [Fact]
        public void SarifConsolidator_Locations()
        {
            SarifConsolidator consolidator = new SarifConsolidator(new Run());

            // Duplicate locations removed, inner Locations trimmed
            List<Location> locations = new List<Location>()
            {
                new Location(SampleLocation),
                new Location(SampleLocation)
            };

            IList<Location> result = consolidator.Trim(locations);
            Assert.Equal(1, result.Count);
            Assert.Null(result[0].PhysicalLocation.ArtifactLocation.UriBaseId);
            Assert.Equal(-1, result[0].PhysicalLocation.Region.CharOffset);
        }

        [Fact]
        public void SarifConsolidator_LogicalLocations()
        {
            SarifConsolidator consolidator = new SarifConsolidator(new Run());

            // Duplicate locations removed, inner Locations trimmed
            List<LogicalLocation> locations = new List<LogicalLocation>()
            {
                new LogicalLocation(SampleLogicalLocation),
                new LogicalLocation(SampleLogicalLocation)
            };

            IList<LogicalLocation> result = consolidator.Trim(locations);
            Assert.Equal(1, result.Count);
            Assert.Null(result[0].Name);
        }

        [Fact]
        public void SarifConsolidator_ThreadFlow()
        {
            ThreadFlowLocation tfl = new ThreadFlowLocation() { ExecutionOrder = 3, Module = "Loader" };

            // Pre-add a ThreadFlowLocation to the Run, to ensure it is considered for re-use
            Run run = new Run()
            {
                ThreadFlowLocations = new List<ThreadFlowLocation>()
                {
                    new ThreadFlowLocation(tfl)
                }
            };

            SarifConsolidator consolidator = new SarifConsolidator(run);

            // ThreadFlow: Unique Locations added to run; only indices on ThreadFlow
            ThreadFlow flow = new ThreadFlow()
            {
                Locations = new List<ThreadFlowLocation>()
                {
                    new ThreadFlowLocation(tfl),
                    new ThreadFlowLocation() { Location = new Location(SampleLocation) },
                    new ThreadFlowLocation(tfl)
                }
            };

            consolidator.Consolidate(flow);
            Assert.Equal(2, run.ThreadFlowLocations.Count);
            Assert.Equal(tfl.Module, run.ThreadFlowLocations[0].Module);
            Assert.Equal(5, run.ThreadFlowLocations[1].Location.PhysicalLocation.ArtifactLocation.Index);

            Assert.Equal(0, flow.Locations[0].Index);
            Assert.Equal(1, flow.Locations[1].Index);
            Assert.Equal(0, flow.Locations[0].Index);
        }

        [Fact]
        public void SarifConsolidator_Result()
        {
            Run run = new Run();
            SarifConsolidator consolidator = new SarifConsolidator(run);

            Result result = new Result()
            {
                Message = new Message() { Text = new string('Z', 500) },
                Locations = new List<Location>()
                {
                    new Location(SampleLocation),
                    new Location(SampleLocation)
                },
                RelatedLocations = new List<Location>()
                {
                    new Location(SampleLocation),
                    new Location(SampleLocation)
                },
                CodeFlows = new List<CodeFlow>(),
                Graphs = new List<Graph>(),
                GraphTraversals = new List<GraphTraversal>(),
                Stacks = new List<Stack>(),
                WebRequest = new WebRequest(),
                WebResponse = new WebResponse()
            };

            Result expected = new Result(result)
            {
                Locations = new List<Location>()
                {
                    new Location(SampleLocationTrimmed)
                },
                RelatedLocations = new List<Location>()
                {
                    new Location(SampleLocationTrimmed)
                },
            };

            consolidator.Trim(result);
            Assert.True(Result.ValueComparer.Equals(expected, result));
            Assert.NotNull(result.CodeFlows);

            consolidator.MessageLengthLimitChars = 128;
            consolidator.RemoveCodeFlows = true;
            consolidator.RemoveGraphs = true;
            consolidator.RemoveRelatedLocations = true;
            consolidator.RemoveStacks = true;
            consolidator.RemoveWebRequests = true;
            consolidator.RemoveWebResponses = true;

            consolidator.Trim(result);
            Assert.Equal(128 + 3, result.Message.Text.Length);  // Truncated + ellipse
            Assert.Null(result.CodeFlows);
            Assert.Null(result.Graphs);
            Assert.Null(result.GraphTraversals);
            Assert.Null(result.RelatedLocations);
            Assert.Null(result.Stacks);
            Assert.Null(result.WebRequest);
            Assert.Null(result.WebResponse);
        }
    }
}
