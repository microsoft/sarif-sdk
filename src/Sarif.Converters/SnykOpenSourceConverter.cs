// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Converters.SnykOpenSourceObjectModel;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class SnykOpenSourceConverter : ToolFileConverterBase
    {
        private readonly LogReader<List<Test>> logReader;
        private const string _CWE_IDENTIFIER_KEY = "CWE";
        private const string _CVE_IDENTIFIER_KEY = "CVE";

        public SnykOpenSourceConverter()
        {
            logReader = new SnykOpenSourceReader();
        }

        public override string ToolName => ToolFormat.SnykOpenSource;

        public override void Convert(Stream input, IResultLogWriter output, OptionallyEmittedData dataToInsert)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));
            output = output ?? throw new ArgumentNullException(nameof(output));

            //Read Snyk data
            List<Test> snykTests = logReader.ReadLog(input);

            //Init objects
            var log = new SarifLog();
            log.Runs = new List<Run>();
            var run = new Run();
            run.Tool = new Tool();
            run.Results = new List<Result>();

            //Set driver details
            run.Tool.Driver = CreateDriver();

            //Set the list of tool rules & results
            run.Tool.Driver.Rules = new List<ReportingDescriptor>();
            foreach (Test test in snykTests.Where(i => !i.Ok))
            {
                foreach (Vulnerability vulnerability in test.Vulnerabilities)
                {
                    //Add rule id if not exits in collection
                    if (!run.Tool.Driver.Rules.Any(i => i.Id == vulnerability.Id))
                    {
                        run.Tool.Driver.Rules.Add(CreateReportDescriptor(vulnerability));
                    }

                    //Add result for the rule if does not previously exist (there are duplicates???)
                    if (!run.Results.Any(i => i.RuleId == vulnerability.Id && i.Locations.Any(l => l.PhysicalLocation.ArtifactLocation.Uri.ToString().Equals(test.DisplayTargetFile))))
                    {
                        run.Results.Add(CreateResult(vulnerability, test));
                    }
                }
            }

            log.Runs.Add(run);
            PersistResults(output, log);
        }

        private ToolComponent CreateDriver()
        {

            var driver = new ToolComponent();

            driver.Name = this.ToolName;
            driver.FullName = this.ToolName;
            //JSON schema has no version information. Pin to 1.0 for now.
            driver.Version = "1.0.0";
            driver.SemanticVersion = "1.0.0";
            driver.InformationUri = new Uri("https://docs.snyk.io/products/snyk-open-source");

            return driver;
        }

        private ReportingDescriptor CreateReportDescriptor(Vulnerability item)
        {
            ReportingDescriptor descriptor = new ReportingDescriptor();

            descriptor.Id = item.Id;
            descriptor.Name = $"{item.Name}@{item.Version}";
            descriptor.ShortDescription = new MultiformatMessageString()
            {
                Text = $"{item.Title} in {item.Name}@{item.Version}",
                Markdown = $"{item.Title} in {item.Name}@{item.Version}",
            };
            descriptor.FullDescription = new MultiformatMessageString()
            {
                Text = item.Description,
                Markdown = item.Description,
            };

            //Help text includes refs + triage advice
            StringBuilder sbHelp = new StringBuilder();
            if (item.References.Count() > 0)
            {
                sbHelp.AppendLine("References:");
                foreach (Reference reference in item.References)
                {
                    sbHelp.AppendFormat("{0}: {1}{2}", reference.Title, reference.Url, Environment.NewLine);
                }
            }

            if (!string.IsNullOrEmpty(item.Insights?.TriageAdvice))
            {
                sbHelp.AppendLine("");
                sbHelp.AppendLine("Triage Advice:");
                sbHelp.Append(item.Insights.TriageAdvice);
            }

            if (sbHelp.Length > 0)
            {
                descriptor.Help = new MultiformatMessageString()
                {
                    Text = sbHelp.ToString(),
                    Markdown = sbHelp.ToString(),
                };
            }

            //Use for GH Security Advisories
            descriptor.SetProperty("security-severity", item.Cvss3BaseScore);

            //Tags for GH filtering
            var tags = new List<string>()
            {
                "security",
                item.PackageManager,
            };

            if (item.Identifiers.ContainsKey(_CWE_IDENTIFIER_KEY))
            {
                tags.AddRange(item.Identifiers[_CWE_IDENTIFIER_KEY]);
            }

            descriptor.SetProperty("tags", tags);

            return descriptor;
        }

        private Result CreateResult(Vulnerability item, Test test)
        {
            //set the result metadata
            Result result = new Result
            {
                RuleId = item.Id,
                Message = new Message
                {
                    Text = $"This file introduces a vulnerable {item.PackageName} package with a {item.SeverityWithCritical} severity vulnerability."
                },
            };

            //Set the kind, level, and rank based on cvss3 score
            FailureLevel level = FailureLevel.None;
            double rank = RankConstants.None;
            getResultSeverity(item.Cvss3BaseScore, out level, out rank);

            //Set the properties
            result.Kind = ResultKind.Fail;
            result.Level = level;
            result.Rank = rank;

            //Set the location properties
            PhysicalLocation location = new PhysicalLocation()
            {
                ArtifactLocation = new ArtifactLocation()
                {
                    Uri = new Uri(test.DisplayTargetFile, UriKind.Relative),
                    UriBaseId = "%SRCROOT%",
                },
                Region = new Region()
                {
                    StartLine = 1,
                }
            };
            result.Locations = new List<Location>();
            result.Locations.Add(new Location()
            {
                PhysicalLocation = location,
            });

            //Set the unique fingerprint
            var fingerprints = new List<string>() {
                item.Id,
                item.PackageManager,
                item.PackageName,
                item.Version,
                test.DisplayTargetFile,
            };

            result.Fingerprints = new Dictionary<string, string>();
            result.Fingerprints.Add("0", HashUtilities.ComputeSha256HashValue(string.Join(".", fingerprints)).ToLower());

            result.SetProperty("packageManager", item.PackageManager);
            result.SetProperty("packageName", item.PackageName);
            result.SetProperty("packageVersion", item.Version);
            result.SetProperty("cvss3BaseScore", item.Cvss3BaseScore.ToString());
            result.SetProperty("cvss3Vector", item.Cvss3Vector);
            result.SetProperty("vulnPublicationDate", item.PublicationTime);

            if (item.Semver.Vulnerable.Any())
            {
                result.SetProperty("semanticVersion", item.Semver.Vulnerable);
            }

            if (item.FixedIn.Any())
            {
                result.SetProperty("patchedVersion", item.FixedIn);
            }

            var cves = new List<string>();
            if (item.Identifiers.ContainsKey(_CVE_IDENTIFIER_KEY))
            {
                cves.AddRange(item.Identifiers[_CVE_IDENTIFIER_KEY]);
            }
            result.SetProperty("cve", cves);

            var cwes = new List<string>();
            if (item.Identifiers.ContainsKey(_CWE_IDENTIFIER_KEY))
            {
                cwes.AddRange(item.Identifiers[_CWE_IDENTIFIER_KEY]);
            }
            result.SetProperty("cwe", cwes);

            //Add other identifiers to the xref
            var xrefs = new List<string>();
            foreach (string key in item.Identifiers.Keys)
            {
                //Skip cve / cwe with dedicated elements
                if (key.Equals(_CVE_IDENTIFIER_KEY) || key.Equals(_CWE_IDENTIFIER_KEY))
                {
                    continue;
                }

                xrefs.AddRange(item.Identifiers[key]);
            }
            result.SetProperty("xref", xrefs);

            return result;
        }

        private void getResultSeverity(double cvss3score, out FailureLevel level, out double rank)
        {
            // Default values
            level = FailureLevel.None;
            rank = RankConstants.None;

            //Failure level by cvss score
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
    }
}
