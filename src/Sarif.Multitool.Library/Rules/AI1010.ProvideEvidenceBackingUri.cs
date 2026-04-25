// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.Json.Pointer;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideEvidenceBackingUri : SarifValidationSkimmerBase
    {
        public ProvideEvidenceBackingUri()
        {
            this.DefaultConfiguration.Level = FailureLevel.Warning;
        }

        /// <summary>
        /// AI1010
        /// </summary>
        public override string Id => RuleId.AIProvideEvidenceBackingUri;

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.AI });

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.AI1010_ProvideEvidenceBackingUri_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.AI1010_ProvideEvidenceBackingUri_Warning_Default_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!result.TryGetSerializedPropertyValue("ai/evidence", out string evidenceJson))
            {
                return;
            }

            JArray evidenceArray;
            try
            {
                evidenceArray = JArray.Parse(evidenceJson);
            }
            catch
            {
                return;
            }

            for (int i = 0; i < evidenceArray.Count; i++)
            {
                if (!(evidenceArray[i] is JObject entry))
                {
                    continue;
                }

                string backing = entry.Value<string>("backing");
                if (string.IsNullOrEmpty(backing) || !backing.StartsWith("sarif:", StringComparison.Ordinal))
                {
                    continue;
                }

                string jsonPointerString = backing.Substring("sarif:".Length);

                try
                {
                    var jsonPointer = new JsonPointer(jsonPointerString);
                    JToken resolved = jsonPointer.Evaluate(Context.InputLogToken);
                    if (resolved == null)
                    {
                        LogResult(
                            resultPointer,
                            nameof(RuleResources.AI1010_ProvideEvidenceBackingUri_Warning_Default_Text),
                            i.ToString(),
                            backing);
                    }
                }
                catch
                {
                    LogResult(
                        resultPointer,
                        nameof(RuleResources.AI1010_ProvideEvidenceBackingUri_Warning_Default_Text),
                        i.ToString(),
                        backing);
                }
            }
        }
    }
}
