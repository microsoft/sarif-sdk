// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Core
{
    public class SarifLogTests 
    {
        [Fact]
        public void SarifLog_DoesNotSerializeNonNullEmptyCollections()
        {
            var run = new Run
            {
                Graphs = new Dictionary<string, Graph>(),
                Artifacts = new List<Artifact>(),
                Invocations = new Invocation[] { },
                LogicalLocations = new List<LogicalLocation>()
            };

            run.Artifacts.Should().NotBeNull();
            run.Graphs.Should().NotBeNull();
            run.Invocations.Should().NotBeNull();

            run = SerializeAndDeserialize(run);

            // Certain non-null but entirely empty collections should not 
            // be persisted during serialization. As a result, these properties
            // should be null after round-tripping, reflecting the actual
            // (i.e., entirely absent) representation on disk when saved.

            run.Artifacts.Should().BeNull();
            run.Graphs.Should().BeNull();
            run.Invocations.Should().BeNull();
            run.LogicalLocations.Should().BeNull();

            // If arrays are non-empty but only contain object instances
            // that consist of nothing but default values, these also 
            // should not be persisted to disk

            run.Invocations = new List<Invocation>();
            run.Invocations.Add(new Invocation());

            run = SerializeAndDeserialize(run);
            run.Invocations.Should().BeNull();
        }

        private Run SerializeAndDeserialize(Run run)
        {
            return JsonConvert.DeserializeObject<Run>(JsonConvert.SerializeObject(run));
        }
    }
}
