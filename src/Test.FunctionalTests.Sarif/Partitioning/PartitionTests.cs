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

        private class TestParameters
        {
            internal PartitionFunction<string> PartitionFunction { get; set; }
            internal bool DeepClone { get; set; }
        }

        [Fact]
        public void Partition_WithTrivialPartitionFunction_WithDeepClone_ReturnsLogWithAllResultsAndRunLevelArrayContentsFromAllResults()
        {
            Partition_WithTrivialPartitionFunction_ReturnsLogWithAllResultsAndRunLevelArrayContentsFromAllResults(deepClone: true);
        }

        [Fact]
        public void Partition_WithTrivialPartitionFunction_WithShallowCopy_ReturnsLogWithAllResultsAndRunLevelArrayContentsFromAllResults()
        {
            Partition_WithTrivialPartitionFunction_ReturnsLogWithAllResultsAndRunLevelArrayContentsFromAllResults(deepClone: false);
        }
        private void Partition_WithTrivialPartitionFunction_ReturnsLogWithAllResultsAndRunLevelArrayContentsFromAllResults(bool deepClone)
        {
            PartitionFunction<string> partitionFunction = result => "default";

            RunTest(
                inputResourceNames: new List<string> { "Partition.sarif" },
                expectedOutputResourceNames: new Dictionary<string, string>
                {
                    ["default"] = "TrivialPartitionFunction.sarif"
                },
                parameter: new TestParameters
                {
                    PartitionFunction = partitionFunction,
                    DeepClone = deepClone
                });
        }

        [Fact]
        public void Partition_ByRuleId_WithDeepClone_ProducesOneLogFilePerRule()
        {
            Partition_ByRuleId_ProducesOneLogFilePerRule(deepClone: true);
        }

        [Fact]
        public void Partition_ByRuleId_WithShallowCopy_ProducesOneLogFilePerRule()
        {
            Partition_ByRuleId_ProducesOneLogFilePerRule(deepClone: false);
        }

        private void Partition_ByRuleId_ProducesOneLogFilePerRule(bool deepClone)
        {
            PartitionFunction<string> partitionFunction = result => result.RuleId;

            RunTest(
                inputResourceNames: new List<string> { "Partition.sarif" },
                expectedOutputResourceNames: new Dictionary<string, string>
                {
                    ["TST0001"] = "TST0001.sarif",
                    ["TST0002"] = "TST0002.sarif",
                    ["TST9999"] = "TST9999.sarif"
                },
                parameter: new TestParameters
                {
                    PartitionFunction = partitionFunction,
                    DeepClone = deepClone
                });
        }

        protected override IDictionary<string, string> ConstructTestOutputsFromInputResources(
            IEnumerable<string> inputResourceNames,
            object parameter)
        {
            // In these tests there is a single input resource and multiple output resources.
            inputResourceNames.Count().Should().Be(1);

            string inputText = GetResourceText(inputResourceNames.First());
            SarifLog inputLog = JsonConvert.DeserializeObject<SarifLog>(inputText);

            var testParameters = (TestParameters)parameter;
            IDictionary<string, SarifLog> outputLogDictionary = SarifPartitioner.Partition(inputLog, testParameters.PartitionFunction, testParameters.DeepClone);

            IDictionary<string, string> outputLogFileContentsDictionary = outputLogDictionary.ToDictionary(
                pair => pair.Key,
                pair => JsonConvert.SerializeObject(pair.Value, Formatting.Indented));

            return outputLogFileContentsDictionary;
        }
    }
}
