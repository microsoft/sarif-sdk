// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public class StepMustIncreaseByOneFromOne : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SV0009_StepMustIncreaseByOneFromOne;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SV0009
        /// </summary>
        public override string Id => RuleId.StepMustIncreaseByOneFromOne;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SV0009_StepNotPresentOnAllLocations)
                };
            }
        }

        protected override void Analyze(CodeFlow codeFlow, string codeFlowPointer)
        {
            var pointer = new JsonPointer(codeFlowPointer);
            JToken codeFlowToken = pointer.Evaluate(Context.InputLogToken);

            JProperty locationsProperty = codeFlowToken.Children<JProperty>()
                .FirstOrDefault(prop => prop.Name.Equals(SarifPropertyName.Locations, StringComparison.Ordinal));
            if (locationsProperty != null)
            {
                JArray annotatedCodeLocationArray = locationsProperty.Value as JArray;
                string annotatedCodeLocationsPointer = codeFlowPointer.AtProperty(SarifPropertyName.Locations);

                ReportMissingStepProperty(annotatedCodeLocationArray, annotatedCodeLocationsPointer);
            }
        }

        private void ReportMissingStepProperty(
            JArray annotatedCodeLocationArray,
            string annotatedCodeLocationsPointer)
        {
            JObject[] annotatedCodeLocationObjects = annotatedCodeLocationArray.Children<JObject>().ToArray();
            if (annotatedCodeLocationObjects.Length > 0)
            {
                JObject[] locationsWithStep = GetLocationsWithStep(annotatedCodeLocationObjects);
                if (locationsWithStep.Length < annotatedCodeLocationObjects.Length)
                {
                    int missingStepIndex = FindFirstLocationWithMissingStep(annotatedCodeLocationObjects);
                    Debug.Assert(missingStepIndex != -1, "Couldn't find location with missing step.");

                    string missingStepPointer = annotatedCodeLocationsPointer.AtIndex(missingStepIndex);

                    LogResult(missingStepPointer, nameof(RuleResources.SV0009_StepNotPresentOnAllLocations));
                }
            }
        }

        private int FindFirstLocationWithMissingStep(JObject[] annotatedCodeLocationObjects)
        {
            int index = -1;

            for (int i = 0; i < annotatedCodeLocationObjects.Length; ++i)
            {
                if (!annotatedCodeLocationObjects[i].Children<JProperty>().Any(prop => prop.Name.Equals(SarifPropertyName.Step, StringComparison.Ordinal)))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private static JObject[] GetLocationsWithStep(JObject[] annotatedCodeLocationObjects)
        {
            return annotatedCodeLocationObjects
                .Where(loc => loc.Children<JProperty>().Any(
                    prop => prop.Name.Equals(SarifPropertyName.Step, StringComparison.Ordinal)))
                .ToArray();
        }
    }
}
