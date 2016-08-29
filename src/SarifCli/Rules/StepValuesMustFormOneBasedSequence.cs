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
    public class StepValuesMustFormOneBasedSequence : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SV0009_StepValuesMustFormOneBasedSequence;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SV0009
        /// </summary>
        public override string Id => RuleId.StepValuesMustFormOneBasedSequence;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SV0009_StepNotPresentOnAllLocations),
                    nameof(RuleResources.SV0009_InvalidStepValue)
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

                ReportMissingStepProperty(
                    annotatedCodeLocationArray,
                    annotatedCodeLocationsPointer);

                ReportInvalidStepValues(
                    codeFlow.Locations.ToArray(),
                    annotatedCodeLocationArray,
                    annotatedCodeLocationsPointer);
            }
        }

        private void ReportInvalidStepValues(
            AnnotatedCodeLocation[] locations,
            JArray annotatedCodeLocationArray,
            string annotatedCodeLocationsPointer)
        {
            JObject[] annotatedCodeLocationObjects = annotatedCodeLocationArray.Children<JObject>().ToArray();

            for (int i = 0; i < locations.Length; ++i)
            {
                // Only report "invalid step value" for locations that actually specify
                // the "step" property (the value of the Step property in the object
                // model will be 0 for such steps, which is never valid), because we
                // already reported the missing "step" properties.
                if (LocationHasStep(annotatedCodeLocationObjects[i]) &&
                    locations[i].Step != i + 1)
                {
                    string invalidStepPointer = annotatedCodeLocationsPointer
                        .AtIndex(i).AtProperty(SarifPropertyName.Step);

                    LogResult(
                        invalidStepPointer,
                        nameof(RuleResources.SV0009_InvalidStepValue),
                        (i + 1).ToInvariantString(),
                        (locations[i].Step).ToInvariantString());
                }
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

                // It's ok if there are no steps, but if any location has a step property,
                // all locations must have it.
                if (locationsWithStep.Length > 0 &&
                    locationsWithStep.Length < annotatedCodeLocationObjects.Length)
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
                if (!annotatedCodeLocationObjects[i].HasProperty(SarifPropertyName.Step))
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
                .Where(LocationHasStep)
                .ToArray();
        }

        private static bool LocationHasStep(JObject loc)
        {
            return loc.HasProperty(SarifPropertyName.Step);
        }
    }
}