// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class InvocationPropertiesMustBeConsistent : SarifValidationSkimmerBase
    {
        public InvocationPropertiesMustBeConsistent() : base(
            RuleId.InvocationPropertiesMustBeConsistent,
            RuleResources.SARIF1006_InvocationPropertiesMustBeConsistent_FullDescription_Text,
            FailureLevel.Error,
            new string[] { nameof(RuleResources.SARIF1006_InvocationPropertiesMustBeConsistent_Error_EndTimeMustNotPrecedeStartTime_Text) })
        { }

        protected override void Analyze(Invocation invocation, string invocationPointer)
        {
            // Compare the start and end times only if both are present.
            if (invocation.StartTimeUtc != default &&
                invocation.EndTimeUtc != default &&
                invocation.StartTimeUtc > invocation.EndTimeUtc)
            {
                string endTimePointer = invocationPointer.AtProperty(SarifPropertyName.EndTimeUtc);

                // {0}: The 'endTimeUtc' value '{1}' precedes the 'startTimeUtc' value '{2}'.
                // The properties of an 'invocation' object must be internally consistent.
                LogResult(
                    endTimePointer,
                    nameof(RuleResources.SARIF1006_InvocationPropertiesMustBeConsistent_Error_EndTimeMustNotPrecedeStartTime_Text),
                    FormatDateTime(invocation.EndTimeUtc),
                    FormatDateTime(invocation.StartTimeUtc));
            }
        }

        private static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString(
                SarifUtilities.SarifDateTimeFormatMillisecondsPrecision,
                CultureInfo.InvariantCulture);
        }
    }
}
