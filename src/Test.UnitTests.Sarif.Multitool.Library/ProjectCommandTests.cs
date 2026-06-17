// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;

using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// Exercises the <c>project</c> verb end to end: it reads a SARIF file and emits flat rows over a
    /// caller-chosen column list. Fixtures are authored as JSON text (not typed objects) so an omitted
    /// <c>result.level</c> survives the read path as a genuinely absent value — the typed model would
    /// otherwise populate the schema default — which is what the inherited-level gap test depends on.
    /// </summary>
    public class ProjectCommandTests
    {
        private const string LogWithInheritance = @"{
  ""$schema"": ""https://json.schemastore.org/sarif-2.1.0.json"",
  ""version"": ""2.1.0"",
  ""runs"": [
    {
      ""tool"": {
        ""driver"": {
          ""name"": ""demo"",
          ""rules"": [
            {
              ""id"": ""TEST001"",
              ""defaultConfiguration"": { ""level"": ""error"" },
              ""properties"": { ""security-severity"": ""9.8"" }
            }
          ]
        }
      },
      ""results"": [
        {
          ""ruleIndex"": 0,
          ""message"": { ""text"": ""finding"" },
          ""locations"": [
            { ""physicalLocation"": { ""artifactLocation"": { ""uri"": ""src/a.cs"" }, ""region"": { ""startLine"": 5 } } }
          ]
        }
      ]
    }
  ]
}";

        [Fact]
        public void Project_ResolvesInheritedLevelAndRuleProperty()
        {
            string[] rows = Project(
                LogWithInheritance,
                ProjectionFormat.Csv,
                new[] { "RuleId", "Level", "Location.Uri", "Location.Region.StartLine", "Properties.security-severity" });

            rows[0].Should().Be("\"RuleId\",\"Level\",\"Location.Uri\",\"Location.Region.StartLine\",\"Properties.security-severity\"");

            // ruleIndex dereferenced to TEST001; absent result level inherits the rule's Error; absent
            // result security-severity inherits the rule's 9.8.
            rows[1].Should().Be("\"TEST001\",\"Error\",\"src/a.cs\",\"5\",\"9.8\"");
        }

        [Fact]
        public void Project_ExplicitResultLevelWinsOverRuleDefault()
        {
            string log = LogWithInheritance.Replace(
                @"""ruleIndex"": 0,",
                @"""ruleIndex"": 0, ""level"": ""note"",");

            string[] rows = Project(log, ProjectionFormat.Csv, new[] { "Level" }, noHeader: true);

            rows[0].Should().Be("\"Note\"");
        }

        [Fact]
        public void Project_ExplicitResultPropertyWinsOverRuleProperty()
        {
            string log = LogWithInheritance.Replace(
                @"""message"": { ""text"": ""finding"" },",
                @"""message"": { ""text"": ""finding"" }, ""properties"": { ""security-severity"": ""1.0"" },");

            string[] rows = Project(log, ProjectionFormat.Csv, new[] { "Properties.security-severity" }, noHeader: true);

            rows[0].Should().Be("\"1.0\"");
        }

        [Fact]
        public void Project_OneRowPerLocationByDefault()
        {
            string log = LogWithInheritance.Replace(
                @"{ ""physicalLocation"": { ""artifactLocation"": { ""uri"": ""src/a.cs"" }, ""region"": { ""startLine"": 5 } } }",
                @"{ ""physicalLocation"": { ""artifactLocation"": { ""uri"": ""src/a.cs"" } } }, { ""physicalLocation"": { ""artifactLocation"": { ""uri"": ""src/b.cs"" } } }");

            string[] rows = Project(log, ProjectionFormat.Csv, new[] { "Location.Uri" }, noHeader: true);

            rows.Should().Equal("\"src/a.cs\"", "\"src/b.cs\"");
        }

        [Fact]
        public void Project_FirstLocationOnly_EmitsSingleRow()
        {
            string log = LogWithInheritance.Replace(
                @"{ ""physicalLocation"": { ""artifactLocation"": { ""uri"": ""src/a.cs"" }, ""region"": { ""startLine"": 5 } } }",
                @"{ ""physicalLocation"": { ""artifactLocation"": { ""uri"": ""src/a.cs"" } } }, { ""physicalLocation"": { ""artifactLocation"": { ""uri"": ""src/b.cs"" } } }");

            string[] rows = Project(log, ProjectionFormat.Csv, new[] { "Location.Uri" }, noHeader: true, firstLocationOnly: true);

            rows.Should().Equal("\"src/a.cs\"");
        }

        [Fact]
        public void Project_ResultWithNoLocation_EmitsOneRowWithEmptyLocationCells()
        {
            string log = LogWithInheritance.Replace(
                @"""locations"": [
            { ""physicalLocation"": { ""artifactLocation"": { ""uri"": ""src/a.cs"" }, ""region"": { ""startLine"": 5 } } }
          ]",
                @"""locations"": []");

            string[] rows = Project(log, ProjectionFormat.Csv, new[] { "RuleId", "Location.Uri" }, noHeader: true);

            rows.Should().Equal("\"TEST001\",\"\"");
        }

        [Fact]
        public void Project_TsvFormat_TabDelimited()
        {
            string[] rows = Project(
                LogWithInheritance,
                ProjectionFormat.Tsv,
                new[] { "RuleId", "Level", "Location.Uri" },
                noHeader: true);

            rows[0].Should().Be("TEST001\tError\tsrc/a.cs");
        }

        [Fact]
        public void Project_NdjsonFormat_OneObjectPerRowNoHeader()
        {
            string[] rows = Project(
                LogWithInheritance,
                ProjectionFormat.Ndjson,
                new[] { "RuleId", "Level", "Properties.security-severity" });

            rows.Should().ContainSingle();
            var row = JObject.Parse(rows[0]);
            row["RuleId"].Value<string>().Should().Be("TEST001");
            row["Level"].Value<string>().Should().Be("Error");
            row["Properties.security-severity"].Value<string>().Should().Be("9.8");
        }

        [Fact]
        public void Project_DynamicFingerprintColumn_ResolvesByKey()
        {
            string log = LogWithInheritance.Replace(
                @"""message"": { ""text"": ""finding"" },",
                @"""message"": { ""text"": ""finding"" }, ""fingerprints"": { ""stableId"": ""abc123"" },");

            string[] rows = Project(log, ProjectionFormat.Csv, new[] { "Fingerprints.stableId" }, noHeader: true);

            rows[0].Should().Be("\"abc123\"");
        }

        [Fact]
        public void Project_UnknownColumn_ReturnsFailure()
        {
            string inputPath = WriteTempLog(LogWithInheritance);
            string outputPath = inputPath + ".out";
            try
            {
                int exitCode = new ProjectCommand().RunWithoutCatch(new ProjectOptions
                {
                    InputFilePath = inputPath,
                    Columns = new[] { "NoSuchColumn" },
                    OutputFilePath = outputPath,
                    Force = true,
                });

                exitCode.Should().Be(CommandBase.FAILURE);
            }
            finally
            {
                Cleanup(inputPath, outputPath);
            }
        }

        private static string[] Project(
            string log,
            ProjectionFormat format,
            string[] columns,
            bool noHeader = false,
            bool firstLocationOnly = false)
        {
            string inputPath = WriteTempLog(log);
            string outputPath = inputPath + ".out";
            try
            {
                int exitCode = new ProjectCommand().RunWithoutCatch(new ProjectOptions
                {
                    InputFilePath = inputPath,
                    Columns = columns,
                    Format = format,
                    OutputFilePath = outputPath,
                    Force = true,
                    NoHeader = noHeader,
                    FirstLocationOnly = firstLocationOnly,
                });

                exitCode.Should().Be(CommandBase.SUCCESS);

                return File.ReadAllText(outputPath)
                    .Split('\n')
                    .Where(line => line.Length > 0)
                    .ToArray();
            }
            finally
            {
                Cleanup(inputPath, outputPath);
            }
        }

        private static string WriteTempLog(string log)
        {
            string path = Path.GetTempFileName();
            File.WriteAllText(path, log);
            return path;
        }

        private static void Cleanup(params string[] paths)
        {
            foreach (string path in paths)
            {
                if (File.Exists(path)) { File.Delete(path); }
            }
        }
    }
}
