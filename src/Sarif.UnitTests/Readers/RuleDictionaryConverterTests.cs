﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class RuleDictionaryConverterTests : JsonTests
    {
        [Fact]
        public void RuleDictionaryConverter_WritesRule()
        {
            string expected =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""rules"": {
        ""CA1000.1"": {
          ""id"": ""CA1000""
        }
      }
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run();

                uut.Initialize(id: null, automationId: null);

                uut.WriteTool(DefaultTool);

                uut.WriteRules(new Dictionary<string, IRule>
                {
                    ["CA1000.1"] = new Rule { Id = "CA1000" }
                });
            });

            actual.Should().BeCrossPlatformEquivalent(expected);

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Equal("CA1000", sarifLog.Runs[0].Rules["CA1000.1"].Id);
        }
    }
}
