// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class AddFilesVisitorTests
    {
        [Fact]
        public void AddFilesVisitor_PopulateFilesObject()
        {
            string filePath = "file:///DOES NOT/EXIST.cpp";

            var run = new Run
            {
                Results = new List<Result>()
                {
                    new Result
                    {
                        Locations = new List<Location>()
                        {
                            new Location()
                            {
                                PhysicalLocation = new PhysicalLocation()
                                {
                                    ArtifactLocation = new ArtifactLocation
                                    {
                                        Uri = new Uri(filePath)
                                    }
                                }
                            }
                        }
                    }
                }
            };

            run.Artifacts.Should().BeNull();

            var visitor = new AddFileReferencesVisitor();
            visitor.Visit(run);

            run.Artifacts.Count.Should().Be(1);
            run.Artifacts[0].Should().NotBeNull();
        }

        [Fact]
        public void AddFilesVisitor_DoesNotPromoteInvocationWorkingDirectory()
        {
            // An invocation's workingDirectory is process context, not a scanned artifact. It must
            // stay a standalone artifactLocation and never land in run.artifacts (which would be a
            // location-only entry flagged by SARIF2004). A result location in the same run is still
            // promoted as usual.
            var workingDirectory = new ArtifactLocation
            {
                Uri = new Uri("src/Sarif/Taxonomies/", UriKind.Relative),
                UriBaseId = "SRCROOT"
            };

            var run = new Run
            {
                Invocations = new List<Invocation>()
                {
                    new Invocation { WorkingDirectory = workingDirectory }
                },
                Results = new List<Result>()
                {
                    new Result
                    {
                        Locations = new List<Location>()
                        {
                            new Location()
                            {
                                PhysicalLocation = new PhysicalLocation()
                                {
                                    ArtifactLocation = new ArtifactLocation
                                    {
                                        Uri = new Uri("src/Sarif/Taxonomies/SampleCode.cs", UriKind.Relative),
                                        UriBaseId = "SRCROOT"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var visitor = new AddFileReferencesVisitor();
            visitor.Visit(run);

            // Only the result's artifact is promoted; the workingDirectory is not.
            run.Artifacts.Count.Should().Be(1);
            run.Artifacts[0].Location.Uri.OriginalString.Should().Be("src/Sarif/Taxonomies/SampleCode.cs");

            // The workingDirectory survives untouched and was never indexed.
            Invocation invocation = run.Invocations[0];
            invocation.WorkingDirectory.Should().BeSameAs(workingDirectory);
            invocation.WorkingDirectory.Index.Should().Be(-1);
        }
    }
}
