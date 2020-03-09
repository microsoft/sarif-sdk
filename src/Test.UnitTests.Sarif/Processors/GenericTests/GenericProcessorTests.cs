// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class GenericProcessorTests
    {
        private List<int> GenerateRandomIntList()
        {
            Random r = new Random();
            int size = r.Next(1, 100);
            List<int> list = new List<int>(size);
            for (int i = 0; i < size; i++)
            {
                list.Add(r.Next());
            }
            return list;
        }

        [Fact]
        public void GenericMapStage_WorksAsExpected()
        {
            List<int> list = GenerateRandomIntList();

            TestMappingProcessor testMapper = new TestMappingProcessor();

            IEnumerable<int> mappedList = testMapper.Map(list.AsEnumerable());

            mappedList.SequenceEqual(list.Select(TestMappingProcessor.internalFunction));
        }

        [Fact]
        public void GenericReduceStage_WorksAsExpected()
        {
            List<int> list = GenerateRandomIntList();

            TestFoldProcessor testFold = new TestFoldProcessor();

            int result = testFold.Fold(list.AsEnumerable());

            result.Should().Be(list.Aggregate(TestFoldProcessor.internalFunction));
        }

        [Fact]
        public void GenericActionPipeline_WorksAsExpected()
        {
            List<int> list = GenerateRandomIntList();

            GenericActionPipeline<int> actionPipeline = new GenericActionPipeline<int>(new List<IActionWrapper<int>> { new TestMappingProcessor(), new TestFoldProcessor(), new TestMappingProcessor() });

            IEnumerable<int> result = actionPipeline.Act(list);

            result.Should().BeEquivalentTo((new List<int>() { list.Select(TestMappingProcessor.internalFunction).Aggregate(TestFoldProcessor.internalFunction) }).Select(TestMappingProcessor.internalFunction));
        }
    }
}
