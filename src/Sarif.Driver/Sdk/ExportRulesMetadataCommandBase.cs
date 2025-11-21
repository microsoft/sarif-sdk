// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class ExportRulesMetadataCommandBase : PluginDriverCommand<ExportRulesMetadataOptions>
    {
        private readonly string[] _levels = new string[] { "Error", "Warning", "Note", "None", "Pass", "NotApplicable" };
        private static readonly Regex s_friendlyNameRegex = new Regex("(?<level>Error|Warning|Note|None|Pass|NotApplicable)_(?<friendlyName>[^_]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override int Run(ExportRulesMetadataOptions exportOptions)
        {
            int result = FAILURE;

            try
            {
                ImmutableArray<ReportingDescriptor> skimmers = CompositionUtilities.GetExports<ReportingDescriptor>(RetrievePluginAssemblies(DefaultPluginAssemblies, exportOptions.PluginFilePaths));

                string format = "";
                string outputFilePath = exportOptions.OutputFilePath;
                string extension = FileSystem.PathGetExtension(outputFilePath);

                switch (extension)
                {
                    case (".json"):
                    case (SarifConstants.SarifFileExtension):
                    {
                        format = "SARIF";
                        OutputSarifRulesMetadata(outputFilePath, skimmers);
                        break;
                    }

                    case (".xml"):
                    {
                        format = "SonarQube";
                        OutputSonarQubeRulesMetadata(outputFilePath, skimmers);
                        break;
                    }

                    case (".md"):
                    {
                        format = "Markdown";
                        OutputMarkdownRulesMetadata(outputFilePath, skimmers);
                        break;
                    }

                    default:
                    {
                        throw new InvalidOperationException("Unrecognized output file extension: " + extension);
                    }
                }

                result = SUCCESS;
                Console.WriteLine(format + " rules metadata exported to: " + Path.GetFullPath(outputFilePath));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }

            return result;
        }

        private void OutputSonarQubeRulesMetadata(string outputFilePath, ImmutableArray<ReportingDescriptor> skimmers)
        {
            const string TAB = "   ";
            var sb = new StringBuilder();

            var sortedRuleContexts = new SortedDictionary<int, ReportingDescriptor>();

            foreach (ReportingDescriptor rule in skimmers)
            {
                int numericId = GetIdIntegerSuffix(rule.Id);
                sortedRuleContexts[numericId] = rule;
            }

            sb.AppendLine("<?xml version='1.0' encoding='UTF-8'?>" + Environment.NewLine +
                         "<rules>");

            foreach (ReportingDescriptor ruleContext in sortedRuleContexts.Values)
            {
                sb.AppendLine(TAB + "<rule>");
                sb.AppendLine(TAB + TAB + "<key>" + ruleContext.Id + "</key>");
                sb.AppendLine(TAB + TAB + "<name>" + ruleContext.Name + "</name>");
                sb.AppendLine(TAB + TAB + "<severity>MAJOR</severity>");

                sb.AppendLine(TAB + TAB + "<description>" + Environment.NewLine +
                              TAB + TAB + TAB + "<![CDATA[" + Environment.NewLine +
                              TAB + TAB + TAB + TAB + ruleContext.FullDescription + Environment.NewLine +
                              TAB + TAB + TAB + "]]>" + Environment.NewLine +
                              TAB + TAB + "</description>");

                sb.AppendLine(TAB + TAB + "<tag>binary</tag>");
                sb.AppendLine(TAB + "</rule>");
            }

            sb.AppendLine("</rules>" + Environment.NewLine + "</profile>");

            File.WriteAllText(outputFilePath, sb.ToString());
        }

        private void OutputSarifRulesMetadata(string outputFilePath, ImmutableArray<ReportingDescriptor> skimmers)
        {
            var log = new SarifLog();

            SarifVersion sarifVersion = SarifVersion.Current;
            log.SchemaUri = sarifVersion.ConvertToSchemaUri();
            log.Version = sarifVersion;

            // The SARIF spec currently requires an array
            // of run logs with at least one member
            log.Runs = new List<Run>();

            var run = new Run();
            run.Tool = new Tool();

            run.Tool = Tool.CreateFromAssemblyData(this.GetType().Assembly);
            run.Results = new List<Result>();

            log.Runs.Add(run);

            var sortedRules = new SortedDictionary<int, ReportingDescriptor>();

            foreach (ReportingDescriptor rule in skimmers)
            {
                int numericId = GetIdIntegerSuffix(rule.Id);

                sortedRules[numericId] = rule;
            }

            run.Tool.Driver.Rules = new List<ReportingDescriptor>(sortedRules.Values);

            var settings = new JsonSerializerSettings()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
            };

            File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(log, settings));
        }

        private void OutputMarkdownRulesMetadata(string outputFilePath, ImmutableArray<ReportingDescriptor> skimmers)
        {
            var sb = new StringBuilder();
            sb.Append("# Rules").AppendLine(Environment.NewLine);

            foreach (ReportingDescriptor rule in skimmers)
            {
                BuildRule(rule, sb);
            }

            File.WriteAllText(outputFilePath, sb.ToString());
        }

        private int GetIdIntegerSuffix(string id)
        {
            int alphaCount = 0;

            foreach (char ch in id)
            {
                if (char.IsLetter(ch))
                {
                    alphaCount++;
                    continue;
                }
                break;
            }
            return int.Parse(id.Substring(alphaCount));
        }

        internal void BuildRule(ReportingDescriptor rule, StringBuilder sb)
        {
            sb.Append("## Rule `").Append(rule.Moniker).Append('`').AppendLine(Environment.NewLine);
            sb.Append("### Description").AppendLine(Environment.NewLine);
            sb.Append(rule.FullDescription?.Markdown
                ?? rule.FullDescription?.Text
                ?? rule.ShortDescription?.Markdown
                ?? rule.ShortDescription?.Text
                ?? DriverResources.NoRuleDescription).AppendLine(Environment.NewLine);
            sb.Append("### Messages").AppendLine(Environment.NewLine);

            if (rule.MessageStrings != null)
            {
                foreach (KeyValuePair<string, MultiformatMessageString> message in rule.MessageStrings)
                {
                    string ruleName = message.Key;
                    string ruleLevel;
                    Match match = s_friendlyNameRegex.Match(message.Key);
                    if (match.Success)
                    {
                        ruleName = match.Groups["friendlyName"].Value;
                        ruleLevel = match.Groups["level"].Value;
                    }
                    else
                    {
                        ruleLevel = GetLevelFromRuleName(ruleName);
                    }

                    sb.Append("#### `").Append(ruleName).Append("`: ").Append(ruleLevel).AppendLine(Environment.NewLine);
                    sb.Append(message.Value.Markdown ?? message.Value.Text).AppendLine(Environment.NewLine);
                }
            }

            sb.Append("---").AppendLine(Environment.NewLine);
        }

        private string GetLevelFromRuleName(string ruleName)
        {
            return Array.Find(_levels, level => ruleName.IndexOf(level, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
