// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using FluentAssertions;
using System.Text;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    [TestClass]
    public class ValidatingVisitorTests
    {
        [TestMethod]
        public void ValidatingVisitor_InvalidUri()
        {
            // NOTE that for validating tests, we need to deserialize SARIF text 
            // without benefit of the many converters that transform specified
            // JSON values to enum equivalents, etc. The Sarif text below and 
            // for all these tests is rendered, in some cases, as it the converter
            // had done some processing before conversion to eventual type. We
            // express the version as OneZeroZeroBeta5, for example, because the
            // default JSON.NET enum conversion will do the right thing as far
            // as converting this value to the C# OM.
            string input =
@"{
  ""version"": ""OneZeroZeroBetaFive"",
  ""runs"": [{
      ""tool"": { ""name"": ""toolName"" },
      ""rules"" : { ""id1"" : { ""id"" : ""id1"", ""helpUri"" : ""??[][][]??""} },
      ""files"": { ""!!not a valid uri!!"" : { ""uri"" : ""!!not a valid uri!!""} },
      ""results"": [{
            ""locations"": [{""resultFile"": { ""uri"": ""file:///C:/space space.cpp"" }}],
            ""stacks"": [{ ""frames"" : [{ ""uri"" : ""c:\\test.cpp"", ""fullyQualifiedLogicalName"" : ""fqn"" }]}],
            ""fixes"": [{ ""fileChanges"": [{ ""uri"" : ""file://c:/missing/slash"", ""replacements"" :[{ ""offset"" : 0, ""deletedLength"" : 1 }] }], ""description"" : ""ok""}]
        }]
      }]
}";

            // TODO do we want to validate that http:// Uris have a trailing slash?

            var sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            using (var visitor = new ValidatingVisitor(writer))
            {
                var inputLog = JsonConvert.DeserializeObject<SarifLog>(input);
                visitor.Visit(inputLog);
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };
            var resultsLog = JsonConvert.DeserializeObject<SarifLog>(sb.ToString(), settings);

            resultsLog.Runs[0].Results.Count.Should().Be(5);
        }
    }
}