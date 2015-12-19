// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Sdk;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public abstract class ExportRulesMetadataCommandBase : PlugInDriverCommand<ExportRulesMetadataOptions>
    {

        public override int Run(ExportRulesMetadataOptions exportOptions)
        {
            int result = FAILURE;

            try
            {
                ImmutableArray<IRuleDescriptor> skimmers = DriverUtilities.GetExports<IRuleDescriptor>(DefaultPlugInAssemblies);

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

        private void OutputSonarQubeRulesMetada(string outputFilePath, ImmutableArray<IRuleDescriptor> skimmers)
        {
            const string TAB = "   ";
            var sb = new StringBuilder();

            SortedDictionary<int, IRuleDescriptor> sortedRuleContexts = new SortedDictionary<int, IRuleDescriptor>();

            foreach (IRuleDescriptor ruleDescriptor in skimmers)
            {
                int numericId = GetIdIntegerSuffix(ruleDescriptor.Id);
                sortedRuleContexts[numericId] = ruleDescriptor;
            }

            sb.AppendLine("<?xml version='1.0' encoding='UTF-8'?>" + Environment.NewLine +
                         "<rules>");

            foreach (IRuleDescriptor ruleContext in sortedRuleContexts.Values)
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

        private void OutputSarifRulesMetada(string outputFilePath, ImmutableArray<IRuleDescriptor> skimmers, ImmutableArray<IOptionsProvider> options)
        {
            var log = new ResultLog();

            log.Version = SarifVersion.ZeroDotFour;

            // The SARIF spec currently requires an array
            // of run logs with at least one member
            log.RunLogs = new List<RunLog>();

            var runLog = new RunLog();
            runLog.ToolInfo = new ToolInfo();

            runLog.ToolInfo.InitializeFromAssembly(this.GetType().Assembly, Prerelease);
            runLog.Results = new List<Result>();

            log.RunLogs.Add(runLog);
            runLog.ToolInfo.RuleInfo = new List<RuleDescriptor>();

            SortedDictionary<int, RuleDescriptor> sortedRuleDescriptors = new SortedDictionary<int, RuleDescriptor>();

            foreach (IRuleDescriptor descriptor in skimmers)
            {
                var ruleDescriptor = new RuleDescriptor();

                ruleDescriptor.Id = descriptor.Id;
                ruleDescriptor.Name = descriptor.Name;
                ruleDescriptor.FullDescription = descriptor.FullDescription;

                int numericId = GetIdIntegerSuffix(ruleDescriptor.Id);

                sortedRuleDescriptors[numericId] = ruleDescriptor;
            }

            foreach (RuleDescriptor ruleDescriptor in sortedRuleDescriptors.Values)
            {
                runLog.ToolInfo.RuleInfo.Add(ruleDescriptor);
            }

            var settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = Formatting.Indented,
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
