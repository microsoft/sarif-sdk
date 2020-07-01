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
        /// <summary>
        /// SARIF1006
        /// </summary>
        public override string Id => RuleId.InvocationPropertiesMustBeConsistent;

        /// <summary>
        /// The properties of an 'invocation' object must be consistent.
        ///
        /// If the 'invocation' object specifies both 'startTimeUtc' and 'endTimeUtc', then 'endTimeUtc'
        /// must not precede 'startTimeUtc'. To allow for the possibility that the duration of the run
        /// is less than the resolution of the string representation of the time, the start time and the
        /// end time may be equal.
        /// </summary>
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF1006_InvocationPropertiesMustBeConsistent_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF1006_InvocationPropertiesMustBeConsistent_Error_EndTimeMustNotPrecedeStartTime_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

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
