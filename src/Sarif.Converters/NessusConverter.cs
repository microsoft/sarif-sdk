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

                        if (item.Severity != "0")
                        {
                            run.Results.Add(CreateResult(item, host.Name));
                        }
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

            //Use for GH Security Advisories
            //set result level and rank (Critical - Low risk rating)
            //ignoring risk factor (H/M/L) as it conflicts with severity
            //cvss3 base score overrides severity 
            FailureLevel level = FailureLevel.None;
            double rank = RankConstants.None;
            getResultSeverity(item.Cvss3BaseScore, item.Severity, out level, out rank);
            descriptor.SetProperty("security-severity", rank.ToString("F1"));

            //Tags for GH filtering
            var tags = new List<string>()
            {
                "security",
            };

            if (item.Cves.Any())
            {
                tags.AddRange(item.Cves);
            }

            descriptor.SetProperty("tags", tags);

            return descriptor;
        }

        internal Result CreateResult(ReportItem item, string hostName)
        {
            //set the result metadata
            Result result = new Result
            {
                RuleId = item.PluginId,
                Message = new Message
                {
                    Text = string.IsNullOrWhiteSpace(item.PluginOutput.Trim()) ? item.Synopsis.Trim() : item.PluginOutput.Trim(),
                },
            };

            //set misc properties
            result.SetProperty("port", item.Port);
            result.SetProperty("protocol", item.Protocol);
            result.SetProperty("service", item.ServiceName);
            result.SetProperty("targetId", hostName);

            //set solution if present
            if (!string.IsNullOrWhiteSpace(item.Solution) && !item.Solution.Equals("n/a"))
                result.SetProperty("solution", item.Solution);

            //set result level and rank (Critical - Low risk rating)
            //ignoring risk factor (H/M/L) as it conflicts with severity
            //cvss3 base score overrides severity 
            FailureLevel level = FailureLevel.None;
            double rank = RankConstants.None;
            getResultSeverity(item.Cvss3BaseScore, item.Severity, out level, out rank);

            result.Kind = ResultKind.Fail;
            result.Level = level;
            result.Rank = rank;

            //set severity value
            result.SetProperty("severity", item.Severity);

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
                hostName,
                item.PluginId,
                item.Port,
                item.Protocol,
                item.ServiceName,
            };

            result.Fingerprints = new Dictionary<string, string>();
            result.Fingerprints.Add("0", HashUtilities.ComputeSha256HashValue(string.Join(".", fingerprints)).ToLower());

            return result;
        }

        private void getResultSeverity(string cvss3BaseScore, string severity, out FailureLevel level, out double rank)
        {
            // Default values
            level = FailureLevel.None;
            rank = RankConstants.None;

            //Failure level by cvss score
            if (!string.IsNullOrWhiteSpace(cvss3BaseScore))
            {
                double cvss3score = double.Parse(cvss3BaseScore);

                if (cvss3score >= 9.0)
                {
                    level = FailureLevel.Error;
                    rank = cvss3score;
                }
                if (cvss3score >= 7.0 && cvss3score < 9.0)
                {
                    level = FailureLevel.Error;
                    rank = cvss3score;
                }
                else if (cvss3score >= 4.0 && cvss3score < 7.0)
                {
                    level = FailureLevel.Warning;
                    rank = cvss3score;

                }
                else if (cvss3score > 0 && cvss3score < 4.0)
                {
                    level = FailureLevel.Note;
                    rank = cvss3score;
                }
            }
            else
            {
                if (severity == "4")
                {
                    level = FailureLevel.Error;
                    rank = RankConstants.Critical;
                }
                else if (severity == "3")
                {
                    level = FailureLevel.Error;
                    rank = RankConstants.High;
                }
                else if (severity == "2")
                {
                    level = FailureLevel.Warning;
                    rank = RankConstants.Medium;
                }
                else if (severity == "1")
                {
                    level = FailureLevel.Note;
                    rank = RankConstants.Low;
                }
            }
        }
    }
}
