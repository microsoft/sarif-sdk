// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Json.Pointer;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class StepValuesMustFormOneBasedSequence : SarifValidationSkimmerBase
    {
        private readonly Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF1009_StepValuesMustFormOneBasedSequence
        };

        public override Message FullDescription => _fullDescription;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SARIF1009
        /// </summary>
        public override string Id => RuleId.StepValuesMustFormOneBasedSequence;

        protected override IEnumerable<string> MessageResourceNames
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF1009_StepNotPresentOnAllLocations),
                    nameof(RuleResources.SARIF1009_InvalidStepValue)
                };
            }
        }

        protected override void Analyze(ThreadFlow threadFlow, string threadFlowPointer)
        {
            var pointer = new JsonPointer(threadFlowPointer);
            JToken threadFlowToken = pointer.Evaluate(Context.InputLogToken);

            JProperty locationsProperty = threadFlowToken.Children<JProperty>()
                .FirstOrDefault(prop => prop.Name.Equals(SarifPropertyName.Locations, StringComparison.Ordinal));
            if (locationsProperty != null)
            {
                JArray threadFlowLocationsArray = locationsProperty.Value as JArray;
                string threadFlowLocationsPointer = threadFlowPointer.AtProperty(SarifPropertyName.Locations);

                ReportMissingStepProperty(
                    threadFlowLocationsArray,
                    threadFlowLocationsPointer);

                ReportInvalidStepValues(
                    threadFlow.Locations.ToArray(),
                    threadFlowLocationsArray,
                    threadFlowLocationsPointer);
            }
        }

        private void ReportInvalidStepValues(
            ThreadFlowLocation[] locations,
            JArray threadFlowLocationsArray,
            string threadFlowLocationsPointer)
        {
            JObject[] threadFlowLocationObjects = threadFlowLocationsArray.Children<JObject>().ToArray();

            for (int i = 0; i < locations.Length; ++i)
            {
                // Only report "invalid step value" for locations that actually specify
                // the "step" property (the value of the Step property in the object
                // model will be 0 for such steps, which is never valid), because we
                // already reported the missing "step" properties.
                if (LocationHasStep(threadFlowLocationObjects[i]) &&
                    locations[i].Step != i + 1)
                {
                    string invalidStepPointer = threadFlowLocationsPointer
                        .AtIndex(i).AtProperty(SarifPropertyName.Step);

                    LogResult(
                        invalidStepPointer,
                        nameof(RuleResources.SARIF1009_InvalidStepValue),
                        (i + 1).ToInvariantString(),
                        (locations[i].Step).ToInvariantString());
                }
            }
        }

        private void ReportMissingStepProperty(
            JArray threadFlowLocationArray,
            string threadFlowLocationsPointer)
        {
            JObject[] threadFlowLocationObjects = threadFlowLocationArray.Children<JObject>().ToArray();
            if (threadFlowLocationObjects.Length > 0)
            {
                JObject[] locationsWithStep = GetLocationsWithStep(threadFlowLocationObjects);

                // It's ok if there are no steps, but if any location has a step property,
                // all locations must have it.
                if (locationsWithStep.Length > 0 &&
                    locationsWithStep.Length < threadFlowLocationObjects.Length)
                {
                    int missingStepIndex = FindFirstLocationWithMissingStep(threadFlowLocationObjects);
                    Debug.Assert(missingStepIndex != -1, "Couldn't find location with missing step.");

                    string missingStepPointer = threadFlowLocationsPointer.AtIndex(missingStepIndex);

                    LogResult(missingStepPointer, nameof(RuleResources.SARIF1009_StepNotPresentOnAllLocations));
                }
            }
        }

        private int FindFirstLocationWithMissingStep(JObject[] threadFlowLocationObjects)
        {
            int index = -1;

            for (int i = 0; i < threadFlowLocationObjects.Length; ++i)
            {
                if (!threadFlowLocationObjects[i].HasProperty(SarifPropertyName.Step))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        private static JObject[] GetLocationsWithStep(JObject[] threadFlowLocationObjects)
        {
            return threadFlowLocationObjects
                .Where(LocationHasStep)
                .ToArray();
        }

        private static bool LocationHasStep(JObject loc)
        {
            return loc.HasProperty(SarifPropertyName.Step);
        }
    }
}