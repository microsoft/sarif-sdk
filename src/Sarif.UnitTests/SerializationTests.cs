// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void SerializeRules()
        {
            var expectedLog = new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Rules = new Dictionary<string, Rule>
                        {
                            { "testId" , new Rule() {  FullDescription = "description" } }
                        }
                    }
                }
            };

            var settings = new JsonSerializerSettings()
            {
                 ContractResolver = SarifContractResolver.Instance,
                 Formatting = Formatting.Indented
            };

            string sarifText = JsonConvert.SerializeObject(expectedLog, settings);

            sarifText = File.ReadAllText(@"D:\src\binskim\src\BinSkim.Driver.FunctionalTests\BaselineTestsData\Actual\ManagedInteropAssemblyForAtlTestLibrary.dll.sarif")

            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(sarifText, settings);

            Assert.AreEqual(expectedLog, actualLog);
        }
    }
}