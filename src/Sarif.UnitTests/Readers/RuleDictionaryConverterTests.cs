﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.TestUtilities;
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
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": null
      },
      ""resources"": {
        ""rules"": {
          ""CA1000.1"": {
            ""id"": ""CA1000""
          }
        }
      },
      ""results"": [
        {
          ""ruleId"": ""CA1000"",
          ""message"": {
            ""text"": ""Variable \""count\"" was used without being initialized.""
          }
        }
      ]
    }
  ]
}";
            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);

                uut.WriteRules(new Dictionary<string, IRule>
                {
                    ["CA1000.1"] = new Rule { Id = "CA1000" }
                });

                uut.WriteResult(new Result
                {
                    RuleId = "CA1000",
                    Message = new Message
                    {
                        Text = "Variable \"count\" was used without being initialized."
                    }
                });
            });

            actual.Should().BeCrossPlatformEquivalent(expected);

            var sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Equal("CA1000", sarifLog.Runs[0].Resources.Rules["CA1000.1"].Id);
        }
    }
}
