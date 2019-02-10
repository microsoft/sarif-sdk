// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndTimeMustNotBeBeforeStartTime : SarifValidationSkimmerBase
    {
        private Message _fullDescription = new Message
        {
            Text = RuleResources.SARIF1007_EndTimeMustNotBeBeforeStartTime
        };

        public override Message FullDescription => _fullDescription;

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        /// <summary>
        /// SARIF1007
        /// </summary>
        public override string Id => RuleId.EndTimeMustNotBeBeforeStartTime;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            nameof(RuleResources.SARIF1007_Default)
        };

        protected override void Analyze(Invocation invocation, string invocationPointer)
        {
            if (invocation.StartTimeUtc > invocation.EndTimeUtc)
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
