// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;

using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class ExportConfigurationCommandBaseTests
    {
        internal static PropertiesDictionary s_defaultConfiguration = CreateDefaultConfiguration();
        internal static PropertiesDictionary s_allRulesDisabledConfiguration = CreateAllRulesDisabledConfiguration();

        private static PropertiesDictionary CreateAllRulesDisabledConfiguration()
        {
            PropertiesDictionary configuration = CreateDefaultConfiguration();

            foreach (PropertiesDictionary properties in configuration.Values)
            {
                properties[DefaultDriverOptions.RuleEnabled.Name] = RuleEnabledState.Disabled;
            }

            return configuration;
        }

        public static PropertiesDictionary CreateDefaultConfiguration()
        {
            PropertiesDictionary configuration = new PropertiesDictionary();
            string path = Path.GetTempFileName() + ".xml";

            try
            {
                var options = new ExportConfigurationOptions
                {
                    FileFormat = FileFormat.Xml,
                    OutputFilePath = path,
                };

                var command = new TestExportConfigurationCommand();
                command.DefaultPlugInAssemblies = new Assembly[] { typeof(ExportConfigurationCommandBaseTests).Assembly };
                int result = command.Run(options);

                Assert.Equal(TestAnalyzeCommand.SUCCESS, result);
            }
            finally
            {
                if (File.Exists(path))
                {
                    configuration.LoadFromXml(path);
                    File.Delete(path);
                }
            }
            return configuration;
        }

        [Fact]
        public void ExportConfigurationCommandBase_VerifyDefaultConfiguration()
        {
            PropertiesDictionary configuration = CreateDefaultConfiguration();

            configuration.Should().NotBeNull();

            // We export two rules, FunctionlessTestRule and TestRule
            configuration.Keys.Count.Should().Be(NumberOfExportedRules());
        }

        [Fact]
        public void ExportConfigurationCommandBase_VerifyAllRulesDisabledConfiguration()
        {
            PropertiesDictionary configuration = CreateAllRulesDisabledConfiguration();

            configuration.Should().NotBeNull();

            // We export two rules, FunctionlessTestRule and TestRule
            configuration.Keys.Count.Should().Be(NumberOfExportedRules());

            foreach (PropertiesDictionary ruleProperties in configuration.Values)
            {
                ((RuleEnabledState)ruleProperties[DefaultDriverOptions.RuleEnabled.Name]).Should().Be(RuleEnabledState.Disabled);
            }
        }

        private static int NumberOfExportedRules()
        {
            return typeof(ExportConfigurationCommandBaseTests).Assembly.GetTypes()
                .Count(t => t.GetCustomAttributes(typeof(ExportAttribute)).Count() > 0);
        }
    }
}
