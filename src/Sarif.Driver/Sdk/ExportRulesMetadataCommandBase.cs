// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Readers;

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
                ImmutableArray<IRule> skimmers = DriverUtilities.GetExports<IRule>(DefaultPlugInAssemblies);

                string format = "";
                string outputFilePath = exportOptions.OutputFilePath;
                string extension = Path.GetExtension(outputFilePath);
                
                switch (extension)
                {
                    case (".json"):
                    case (".sarif"):
                    {
                        format = "SARIF";
                        ImmutableArray<IOptionsProvider> options = DriverUtilities.GetExports<IOptionsProvider>(DefaultPlugInAssemblies);
                        OutputSarifRulesMetada(outputFilePath, skimmers, options);
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

        private void OutputSonarQubeRulesMetada(string outputFilePath, ImmutableArray<IRule> skimmers)
        {
            const string TAB = "   ";
            var sb = new StringBuilder();

            SortedDictionary<int, IRule> sortedRuleContexts = new SortedDictionary<int, IRule>();

            foreach (IRule rule in skimmers)
            {
                int numericId = GetIdIntegerSuffix(rule.Id);
                sortedRuleContexts[numericId] = rule;
            }

            sb.AppendLine("<?xml version='1.0' encoding='UTF-8'?>" + Environment.NewLine +
                         "<rules>");

            foreach (IRule ruleContext in sortedRuleContexts.Values)
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

        private void OutputSarifRulesMetada(string outputFilePath, ImmutableArray<IRule> skimmers, ImmutableArray<IOptionsProvider> options)
        {
            var log = new SarifLog();

            SarifVersion sarifVersion = SarifVersion.OneZeroZeroBetaFour;
            log.SchemaUri = sarifVersion.ConvertToSchemaUri();
            log.Version = sarifVersion;

            // The SARIF spec currently requires an array
            // of run logs with at least one member
            log.Runs = new List<Run>();

            var run = new Run();
            run.Tool = new Tool();

            run.Tool.InitializeFromAssembly(this.GetType().Assembly, Prerelease);
            run.Results = new List<Result>();

            log.Runs.Add(run);
            run.Rules = new Dictionary<string, Rule>();

            SortedDictionary<int, Rule> sortedRules = new SortedDictionary<int, Rule>();

            foreach (IRule rule in skimmers)
            {
                var newRule = new Rule();

                newRule.Id = rule.Id;
                newRule.Name = rule.Name;
                newRule.HelpUri = rule.HelpUri;
                newRule.Properties = rule.Properties;
                newRule.FullDescription = rule.FullDescription;
                newRule.MessageFormats = rule.MessageFormats;

                newRule.ShortDescription = rule.ShortDescription;

                int numericId = GetIdIntegerSuffix(newRule.Id);

                sortedRules[numericId] = newRule;
            }

            foreach (Rule rule in sortedRules.Values)
            {
                run.Rules[rule.Id] = rule;
            }

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = Newtonsoft.Json.Formatting.Indented,
            };
            File.WriteAllText(outputFilePath, JsonConvert.SerializeObject(log, settings));
        }

        private int GetIdIntegerSuffix(string id)
        {
            int alphaCount = 0;

            foreach (char ch in id)
            {
                if (Char.IsLetter(ch))
                {
                    alphaCount++;
                    continue;
                }
                break;
            }
            return Int32.Parse(id.Substring(alphaCount));
        }
    }
}
