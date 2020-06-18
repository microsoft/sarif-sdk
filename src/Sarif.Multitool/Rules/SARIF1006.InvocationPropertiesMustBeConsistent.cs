// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class InvocationPropertiesMustBeConsistent : SarifValidationSkimmerBase
    {
        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.SARIF1006_InvocationPropertiesMustBeConsistent_FullDescription_Text
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override string Id => RuleId.InvocationPropertiesMustBeConsistent;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1006_InvocationPropertiesMustBeConsistent_Error_EndTimeMustNotPrecedeStartTime_Text)
        };

        protected override void Analyze(Invocation invocation, string invocationPointer)
        {
            // Compare the start and end times only if both are present.
            if (invocation.StartTimeUtc != default &&
                invocation.EndTimeUtc != default &&
                invocation.StartTimeUtc > invocation.EndTimeUtc)
            {
                string endTimePointer = invocationPointer.AtProperty(SarifPropertyName.EndTimeUtc);

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
