// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class EndTimeMustBeAfterStartTime : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SARIF1007_EndTimeMustBeAfterStartTime;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SARIF1007
        /// </summary>
        public override string Id => RuleId.EndTimeMustBeAfterStartTime;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SARIF1007_Default)
                };
            }
        }

        protected override void Analyze(Invocation invocation, string invocationPointer)
        {
            if (invocation.StartTime > invocation.EndTime)
            {
                string endTimePointer = invocationPointer.AtProperty(SarifPropertyName.EndTime);

                LogResult(
                    endTimePointer,
                    nameof(RuleResources.SARIF1007_Default),
                    FormatDateTime(invocation.EndTime),
                    FormatDateTime(invocation.StartTime));
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
