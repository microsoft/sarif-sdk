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
    }
}
