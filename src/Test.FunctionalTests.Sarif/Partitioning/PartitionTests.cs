// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Test.FunctionalTests.Sarif.Partitioning
{
    public class PartitionTests : FileDiffingFunctionalTests
    {
        public PartitionTests(ITestOutputHelper outputHelper, bool testProducesSarifCurrentVersion = true) :
            base(outputHelper, testProducesSarifCurrentVersion)
        { }

        protected override string IntermediateTestFolder => @"Partitioning";

        [Fact]
        public void Partition_WithTrivialPartitionFunction_ReturnsLogWithAllResultsAndRunLevelArrayContentsFromAllResults()
        {
            PartitioningVisitor<string>.PartitionFunction partitionFunction = result => "default";

            RunTest(
                inputResourceNames: new List<string> { "Partition.sarif" },
                expectedOutputResourceNames: new Dictionary<string, string>
                {
                    ["default"] = "TrivialPartitionFunction.sarif"
                },
                parameter: partitionFunction);
        }

        protected override IDictionary<string, string> ConstructTestOutputsFromInputResources(
            IEnumerable<string> inputResourceNames,
            object parameter)
        {
            // In these tests there is a single input resource and multiple output resources.
            inputResourceNames.Count().Should().Be(1);

            var partionFunction = (PartitioningVisitor<string>.PartitionFunction)parameter;

            string inputText = GetResourceText(inputResourceNames.First());
            SarifLog inputLog = JsonConvert.DeserializeObject<SarifLog>(inputText);

            IDictionary<string, SarifLog> outputLogDictionary = SarifPartitioner.Partition(inputLog, partionFunction);

            IDictionary<string, string> outputLogFileContentsDictionary = outputLogDictionary.ToDictionary(
                pair => pair.Key,
                pair => JsonConvert.SerializeObject(pair.Value, Formatting.Indented));

            return outputLogFileContentsDictionary;
        }
    }
}
