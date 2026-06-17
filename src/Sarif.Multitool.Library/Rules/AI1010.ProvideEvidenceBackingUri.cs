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
            this.DefaultConfiguration.Level = FailureLevel.Error;
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
            nameof(RuleResources.AI1010_ProvideEvidenceBackingUri_Error_Default_Text)
        };

        protected override void Analyze(Result result, string resultPointer)
        {
            if (!result.TryGetSerializedPropertyValue("ai/evidence", out string evidenceJson))
            {
                return;
            }

            // A malformed or wrong-shaped 'ai/evidence' value is reported by AI2016, which
            // owns the well-formedness check; skip cleanly here rather than re-report it.
            if (!EvidenceJsonReader.TryParseEvidenceArray(evidenceJson, out JArray evidenceArray))
            {
                return;
            }

            for (int i = 0; i < evidenceArray.Count; i++)
            {
                if (!(evidenceArray[i] is JObject entry))
                {
                    continue;
                }

                // SARIF-AI producers in the wild emit 'backing' as either a single
                // string or a JSON array of strings (multi-source evidence). Read
                // defensively rather than calling JObject.Value<string>(), which
                // throws InvalidCastException on a JArray and disables the rule
                // mid-run (issue #2908).
                IReadOnlyList<string> backings = EvidenceJsonReader.ReadStrings(entry, "backing");

                foreach (string backing in backings)
                {
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
                                nameof(RuleResources.AI1010_ProvideEvidenceBackingUri_Error_Default_Text),
                                i.ToString(),
                                backing);
                        }
                    }
                    catch (ArgumentException)
                    {
                        // A malformed or unresolvable JSON pointer is the violation this
                        // rule reports; JsonPointer surfaces both as ArgumentException.
                        // Anything else is an unexpected fault and propagates to the
                        // analysis engine's logging handler rather than being masked here.
                        LogResult(
                            resultPointer,
                            nameof(RuleResources.AI1010_ProvideEvidenceBackingUri_Error_Default_Text),
                            i.ToString(),
                            backing);
                    }
                }
            }
        }
    }
}
