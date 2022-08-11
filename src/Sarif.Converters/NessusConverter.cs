// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.CodeAnalysis.Sarif.Converters.NessusObjectModel;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class NessusConverter : ToolFileConverterBase
    {
        public override string ToolName => ToolFormat.Nessus;

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            //LogicalLocations.Clear();

            XmlReaderSettings settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null
            };

            //Parse nessus data
            var serializer = new XmlSerializer(typeof(NessusClientData));

            //List of runs (one for each nessus host scanned)
            var log = new SarifLog();
            log.Runs = new List<Run>();

            using (var reader = XmlReader.Create(input, settings))
            {
                var nessusClientData = (NessusClientData)serializer.Deserialize(reader);

                foreach (ReportHost host in nessusClientData.Report.ReportHosts)
                {
                    var run = new Run();

                    //Init objects
                    run.Tool = new Tool();
                    run.Results = new List<Result>();

                    //Set driver details
                    run.Tool.Driver = CreateDriver(nessusClientData, host.Name);

                    //Set the list of tool rules
                    foreach (ReportItem item in host.ReportItems)
                    {
                        //Add rule (plugin id) if not exits
                        if (!run.Tool.Driver.Rules.Any(i => i.Id == item.PluginId))
                        {
                            run.Tool.Driver.Rules.Add(CreateReportDescriptor(item));
                        }

                        run.Results.Add(CreateResult(item, host.Name));
                    }

                    log.Runs.Add(run);
                }

                PersistResults(output, log);
            }
        }

        private ToolComponent CreateDriver(NessusClientData data, string targetId)
        {
            var driver = new ToolComponent();
            driver.Rules = new List<ReportingDescriptor>();

            driver.Name = this.ToolName;
            driver.FullName = data.Report.Name;
            driver.Version = data.Policy.Preferences.ServerPreference.Preferences.FirstOrDefault(i => i.Name.Equals("sc_version"))?.Value;
            driver.InformationUri = new Uri("https://static.tenable.com/documentation/nessus_v2_file_format.pdf");
            driver.SetProperty("targetId", targetId);
            return driver;
        }

        private ReportingDescriptor CreateReportDescriptor(ReportItem item)
        {
            ReportingDescriptor descriptor = new ReportingDescriptor();

            descriptor.Id = item.PluginId;
            descriptor.Name = item.PluginName;
            descriptor.ShortDescription = new MultiformatMessageString()
            {
                Text = item.Synopsis,
                Markdown = item.Synopsis,
            };
            descriptor.FullDescription = new MultiformatMessageString()
            {
                Text = item.Description,
                Markdown = item.Description,
            };

            if (!string.IsNullOrWhiteSpace(item.SeeAlso))
            {
                descriptor.Help = new MultiformatMessageString()
                {
                    Text = item.SeeAlso,
                    Markdown = item.SeeAlso,
                };
            }

            descriptor.SetProperty("pluginFamily", item.PluginFamily);
            descriptor.SetProperty("pluginModificationDate", item.PluginModificationDate);
            descriptor.SetProperty("pluginPublicationDate", item.PluginPublicationDate);
            descriptor.SetProperty("pluginType", item.PluginType);

            return descriptor;
        }

        internal Result CreateResult(ReportItem item, string hostName)
        {
            //set the result metadata
            Result result = new Result
            {
                RuleId = item.PluginId,
                Message = new Message { Text = item.PluginOutput },
            };

            //set misc properties
            result.SetProperty("port", item.Port);
            result.SetProperty("protocol", item.Protocol);
            result.SetProperty("service", item.ServiceName);

            //set solution if present
            if (!string.IsNullOrWhiteSpace(item.Solution) && !item.Solution.Equals("n/a"))
                result.SetProperty("solution", item.Solution);

            //set severity (rank)
            //ignore risk factor (H/M/L) as it conflicts with rank 
            result.Kind = ResultKind.Fail;
            result.Rank = double.Parse(item.Severity);

            //vulnerable packages contain cve / cvss data
            if (!string.IsNullOrEmpty(item.Cvss3BaseScore))
            {
                result.SetProperty("cvss3BaseScore", item.Cvss3BaseScore);
            }

            if (!string.IsNullOrEmpty(item.Cvss3TemporalScore))
            {
                result.SetProperty("cvss3TemporalScore", item.Cvss3TemporalScore);
            }

            if (!string.IsNullOrEmpty(item.Cvss3Vector))
            {
                result.SetProperty("cvss3Vector", item.Cvss3Vector);
            }

            if (!string.IsNullOrEmpty(item.Cvss3TemporalVector))
            {
                result.SetProperty("cvss3TemporalVector", item.Cvss3TemporalVector);
            }

            if (!string.IsNullOrEmpty(item.VulnPublicationDate))
            {
                result.SetProperty("vulnPublicationDate", item.VulnPublicationDate);
            }

            if (!string.IsNullOrEmpty(item.PatchPublicationDate))
            {
                result.SetProperty("patchPublicationDate", item.PatchPublicationDate);
            }

            if (item.Cves.Any())
            {
                result.SetProperty("cve", item.Cves);
            }

            if (item.Xrefs.Any())
            {
                result.SetProperty("xref", item.Xrefs);
            }

            if (!string.IsNullOrEmpty(item.ExploitAvailable))
            {
                result.SetProperty("exploitAvailable", bool.Parse(item.ExploitAvailable));
            }

            //Set the unique fingerprint per item
            var fingerprints = new List<string>() {
                item.PluginId,
                item.Port,
                item.Protocol,
                item.ServiceName,
            };

            result.Fingerprints = new Dictionary<string, string>();
            result.Fingerprints.Add("0", HashUtilities.ComputeSha256HashValue(string.Join(".", fingerprints)).ToLower());

            return result;
        }
    }
}
