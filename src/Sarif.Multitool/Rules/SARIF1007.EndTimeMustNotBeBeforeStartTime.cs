// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndTimeMustNotBeBeforeStartTime : SarifValidationSkimmerBase
    {
        public EndTimeMustNotBeBeforeStartTime() : base(
            RuleId.EndTimeMustNotBeBeforeStartTime,
            RuleResources.SARIF1007_EndTimeMustNotBeBeforeStartTime,
            FailureLevel.Error,
            new string[] { nameof(RuleResources.SARIF1007_Default) })
        { }

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
                    nameof(RuleResources.SARIF1007_Default),
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
