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
        private const string InputFormat =
@"{
  ""version"": ""1.0.0"",
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
                  ""id"": PLACEHOLDER,
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

        private void RunTestCase(string idText, int expectedId = 0, bool valid = true)
        {
            string input = InputFormat.Replace("PLACEHOLDER", idText);

            SarifLog log = null;
            bool actualValid = false;

            try
            {
                log = JsonConvert.DeserializeObject<SarifLog>(input);
                actualValid = true;
            }
            catch (Exception)
            {
            }

            actualValid.Should().Be(valid);

            if (actualValid)
            {
                log.Runs[0].Results[0].CodeFlows[0].Locations[0].Id.Should().Be(expectedId);
            }
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_ConvertsPositiveInteger()
        {
            RunTestCase("1", expectedId: 1);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsZero()
        {
            RunTestCase("0", valid: false);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsNegativeInteger()
        {
            RunTestCase("-1", valid: false);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_ConvertsStringPositiveInteger()
        {
            RunTestCase("\"1\"", expectedId: 1);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsStringZero()
        {
            RunTestCase("\"0\"", valid: false);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsStringNegativeInteger()
        {
            RunTestCase("\"-1\"", valid: false);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsStringPositiveIntegerWithLeadingNonDigits()
        {
            RunTestCase("\"x1\"", valid: false);
        }

        [TestMethod]
        public void AnnotatedCodeLocationIdConverter_RejectsStringPositiveIntegerWithTrailingNonDigits()
        {
            RunTestCase("\"1x\"", valid: false);
        }
    }
}
