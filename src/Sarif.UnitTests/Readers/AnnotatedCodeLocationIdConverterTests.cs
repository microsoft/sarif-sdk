// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    [TestClass]
    public class AnnotatedCodeLocationIdConverterTests: JsonTests
    {
        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_ConvertsPositiveInteger()
        {
            RunTestCase(idText: "1", expectedId: 1);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsZero()
        {
            RunTestCase(idText: "0", valid: false);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsNegativeInteger()
        {
            RunTestCase(idText: "-1", valid: false);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_ConvertsStringPositiveInteger()
        {
            RunTestCase(idText: "\"1\"", expectedId: 1);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsStringZero()
        {
            RunTestCase(idText: "\"0\"", valid: false);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsStringNegativeInteger()
        {
            RunTestCase(idText: "\"-1\"", valid: false);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsStringPositiveIntegerWithLeadingNonDigits()
        {
            RunTestCase(idText: "\"x1\"", valid: false);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsStringPositiveIntegerWithTrailingNonDigits()
        {
            RunTestCase(idText: "\"1x\"", valid: false);
        }

        private void RunTestCase(string idText, int expectedId = 0, bool valid = true)
        {
            SarifLog log = null;
            Action action = () =>
            {
                string input = MakeInputString(idText);
                log = JsonConvert.DeserializeObject<SarifLog>(input);
            };

            if (valid)
            {
                action.ShouldNotThrow();
                log.Runs[0].Results[0].CodeFlows[0].Locations[0].Id.Should().Be(expectedId);
            }
            else
            {
                action.ShouldThrow<ArgumentOutOfRangeException>();
            }
        }

        private static string MakeInputString(string idText)
        {
            return
@"{
  ""version"": """ + SarifFormatVersion + @""",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""TestTool""
      },
      ""results"": [
        {
          ""codeFlows"": [
            {
              ""locations"": [
                {
                  ""id"": " + idText + @",
                  ""physicalLocation"": {
                    ""uri"": ""file://C:/code/a.c""
                  }
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}";
        }
    }
}
