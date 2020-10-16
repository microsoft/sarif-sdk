// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class LogPipelineSerializationTests
    {
        [Fact]
        public void SerializeDeserializePipeline_WorksAsExpected()
        {
            SarifLogPipeline preserialized = new SarifLogPipeline(
                new List<SarifLogActionTuple>()
                 { new SarifLogActionTuple(){Action=SarifLogAction.RebaseUri, Parameters=new string[] {"SrcRoot", @"C:\src\"} },
                   new SarifLogActionTuple(){Action=SarifLogAction.Merge, Parameters=new string[0]}
                });

            string result = JsonConvert.SerializeObject(preserialized);

            SarifLogPipeline deserialized = JsonConvert.DeserializeObject<SarifLogPipeline>(result);

            deserialized.Should().BeEquivalentTo(preserialized);
        }
    }
}
