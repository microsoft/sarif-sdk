// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Resources;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.Json.Pointer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public abstract class SarifValidationSkimmerBase : SkimmerBase<SarifValidationContext>
    {
        private const string SarifSpecUri =
            "https://rawgit.com/sarif-standard/sarif-spec/master/Static%20Analysis%20Results%20Interchange%20Format%20(SARIF).html";

        private readonly Uri _defaultHelpUri = new Uri(SarifSpecUri);
        private JToken _rootToken;

        public override Uri HelpUri => _defaultHelpUri;

        protected SarifValidationContext Context { get; private set; }

        protected override sealed ResourceManager ResourceManager => RuleResources.ResourceManager;

        public override sealed void Analyze(SarifValidationContext context)
        {
            Context = context;

            string logContents = File.ReadAllText(context.TargetUri.LocalPath);
            _rootToken = JToken.Parse(logContents);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };

            SarifLog log = JsonConvert.DeserializeObject<SarifLog>(logContents, settings);
            string logPointer = string.Empty;

            Visit(log, logPointer);
        }

        private void Visit(SarifLog log, string logPointer)
        {
            if (log.Runs != null)
            {
                Run[] runs = log.Runs.ToArray();
                string runsPointer = logPointer.AtProperty(SarifPropertyName.Runs);

                for (int iRun = 0; iRun < runs.Length; ++iRun)
                {
                    Run run = runs[iRun];
                    string runPointer = runsPointer.AtIndex(iRun);

                    Visit(run, runPointer);
                }
            }
        }

        private void Visit(Run run, string runPointer)
        {
            if (run.Results != null)
            {
                Result[] results = run.Results.ToArray();
                string resultsPointer = runPointer.AtProperty(SarifPropertyName.Results);

                for (int iResult = 0; iResult < results.Length; ++iResult)
                {
                    Result result = results[iResult];
                    string resultPointer = resultsPointer.AtIndex(iResult);

                    Visit(result, resultPointer);
                }
            }

            if (run.Rules != null)
            {
                Rule[] rules = run.Rules.Values.ToArray();
                string rulesPointer = runPointer.AtProperty(SarifPropertyName.Rules);

                for (int iRule = 0; iRule < rules.Length; ++iRule)
                {
                    Rule rule = rules[iRule];
                    if (rule.Id != null)
                    {
                        string rulePointer = rulesPointer.AtProperty(rule.Id);
                        Analyze(rule, rulePointer);
                    }
                }
            }
        }

        private void Visit(Result result, string resultPointer)
        {
            if (result.Locations != null)
            {
                Location[] locations = result.Locations.ToArray();
                string locationsPointer = resultPointer.AtProperty(SarifPropertyName.Locations);

                for (int iLocation = 0; iLocation < locations.Length; ++iLocation)
                {
                    Location location = locations[iLocation];
                    string locationPointer = locationsPointer.AtIndex(iLocation);

                    Visit(location, locationPointer);
                }
            }
        }

        private void Visit(Location location, string locationPointer)
        {
            if (location.AnalysisTarget != null)
            {
                string analysisTargetPointer = locationPointer.AtProperty(SarifPropertyName.AnalysisTarget);
                Visit(location.AnalysisTarget, analysisTargetPointer);
            }

            if (location.ResultFile != null)
            {
                string resultFilePointer = locationPointer.AtProperty(SarifPropertyName.ResultFile);
                Visit(location.ResultFile, resultFilePointer);
            }
        }

        private void Visit(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
            Analyze(physicalLocation, physicalLocationPointer);
        }

        protected virtual void Analyze(Rule rule, string rulePointer)
        {
        }

        protected virtual void Analyze(PhysicalLocation physicalLocation, string physicalLocationPointer)
        {
        }

        protected void LogResult(ResultLevel level, string jPointer, string formatId, params string[] args)
        {
            Region region = GetRegionFromJPointer(jPointer);

            Context.Logger.Log(this,
                RuleUtilities.BuildResult(ResultLevel.Warning, Context, region, formatId, args));
        }

        private Region GetRegionFromJPointer(string jPointer)
        {
            JsonPointer jsonPointer = new JsonPointer(jPointer);
            JToken jToken = jsonPointer.Evaluate(_rootToken);
            IJsonLineInfo lineInfo = jToken;

            Region region = null;
            if (lineInfo.HasLineInfo())
            {
                region = new Region
                {
                    StartLine = lineInfo.LineNumber,
                    StartColumn = lineInfo.LinePosition
                };
            }

            return region;
        }
    }
}
