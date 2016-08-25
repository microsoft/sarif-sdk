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

        public override Uri HelpUri => _defaultHelpUri;

        protected SarifValidationContext Context { get; private set; }
        protected SarifLog InputLog { get; private set; }
        protected JToken InputLogToken { get; private set; }

        protected override sealed ResourceManager ResourceManager => RuleResources.ResourceManager;

        public override sealed void Analyze(SarifValidationContext context)
        {
            Context = context;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };

            string inputLogContents = File.ReadAllText(context.TargetUri.LocalPath);
            InputLogToken = JToken.Parse(inputLogContents);
            InputLog = JsonConvert.DeserializeObject<SarifLog>(inputLogContents, settings);

            if (InputLog.Runs != null)
            {
                Run[] runs = InputLog.Runs.ToArray();
                for (int iRun = 0; iRun < runs.Length; ++iRun)
                {
                    Run run = runs[iRun];
                    if (run.Rules != null)
                    {
                        Rule[] rules = run.Rules.Values.ToArray();
                        for (int iRule = 0; iRule < rules.Length; ++iRule)
                        {
                            Rule rule = rules[iRule];
                            string jPointer = $"/runs/{iRun}/rules/{rule.Id}";
                            Visit(rule, jPointer);
                        }
                    }
                }
            }
        }

        protected abstract void Visit(Rule rule, string jPointer);

        protected void LogResult(ResultLevel level, string jPointer, string formatId, params string[] args)
        {
            Region region = GetRegionFromJPointer(jPointer);

            Context.Logger.Log(this,
                RuleUtilities.BuildResult(ResultLevel.Warning, Context, region, formatId, args));
        }

        private Region GetRegionFromJPointer(string jPointerValue)
        {
            JsonPointer jPointer = new JsonPointer(jPointerValue);
            JToken jToken = jPointer.Evaluate(InputLogToken);
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
