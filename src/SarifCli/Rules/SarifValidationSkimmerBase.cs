// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
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

        protected override ResourceManager ResourceManager => RuleResources.ResourceManager;

        public override void Analyze(SarifValidationContext context)
        {
            Context = context;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };

            string inputLogContents = File.ReadAllText(context.TargetUri.LocalPath);
            InputLogToken = JToken.Parse(inputLogContents);
            InputLog = JsonConvert.DeserializeObject<SarifLog>(inputLogContents, settings);

            AnalyzeCore();
        }

        protected abstract void AnalyzeCore();

        protected Region GetRegionFromJPointer(string jPointerValue)
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
