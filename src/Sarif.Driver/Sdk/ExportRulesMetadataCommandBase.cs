// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class ExportRulesMetadataCommandBase : PlugInDriverCommand<ExportRulesMetadataOptions>
    {

        public override int Run(ExportRulesMetadataOptions exportOptions)
        {
            int result = FAILURE;

            try
            {
                ImmutableArray<ReportingDescriptor> skimmers = CompositionUtilities.GetExports<ReportingDescriptor>(DefaultPlugInAssemblies);

                string format = "";
                string outputFilePath = exportOptions.OutputFilePath;
                string extension = Path.GetExtension(outputFilePath);

                switch (extension)
                {
                    case (".json"):
                    case (SarifConstants.SarifFileExtension):
                    {
                        format = "SARIF";
                        OutputSarifRulesMetada(outputFilePath, skimmers);
                        break;
                    }

                    case (".xml"):
                    {
                        format = "SonarQube";
                        OutputSonarQubeRulesMetada(outputFilePath, skimmers);
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

        private void OutputSonarQubeRulesMetada(string outputFilePath, ImmutableArray<ReportingDescriptor> skimmers)
        {
            const string TAB = "   ";
            var sb = new StringBuilder();

            SortedDictionary<int, ReportingDescriptor> sortedRuleContexts = new SortedDictionary<int, ReportingDescriptor>();

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

        private void OutputSarifRulesMetada(string outputFilePath, ImmutableArray<ReportingDescriptor> skimmers)
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

            SortedDictionary<int, ReportingDescriptor> sortedRules = new SortedDictionary<int, ReportingDescriptor>();

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
    }
}
