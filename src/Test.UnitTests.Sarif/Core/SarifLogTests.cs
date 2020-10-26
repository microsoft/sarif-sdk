﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

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
                Graphs = new Graph[] { },
                Artifacts = new Artifact[] { },
                Invocations = new Invocation[] { },
                LogicalLocations = new LogicalLocation[] { }
            };

            run.Graphs.Should().NotBeNull();
            run.Artifacts.Should().NotBeNull();
            run.Invocations.Should().NotBeNull();
            run.LogicalLocations.Should().NotBeNull();

            run = SerializeAndDeserialize(run);

            // Certain non-null but entirely empty collections should not 
            // be persisted during serialization. As a result, these properties
            // should be null after round-tripping, reflecting the actual
            // (i.e., entirely absent) representation on disk when saved.

            run.Graphs.Should().BeNull();
            run.Artifacts.Should().BeNull();
            run.Invocations.Should().BeNull();
            run.LogicalLocations.Should().BeNull();

            // Invocations are special, they have a required property,
            // ExecutionSuccessful. This means even an entirely default instance
            // should be retained when serialized.
            run.Invocations = new Invocation[] { new Invocation() };

            run = SerializeAndDeserialize(run);

            run.Invocations.Should().NotBeNull();
        }

        [Fact]
        public void SarifLog_ApplyPoliciesShouldNotThrowWhenRunsDoesNotExist()
        {
            var sarifLog = new SarifLog();
            Action action = () => sarifLog.ApplyPolicies();

            action.Should().NotThrow();
        }

        private Run SerializeAndDeserialize(Run run)
        {
            string serializedJson = JsonConvert.SerializeObject(run);
            return JsonConvert.DeserializeObject<Run>(serializedJson);
        }
    }
}
