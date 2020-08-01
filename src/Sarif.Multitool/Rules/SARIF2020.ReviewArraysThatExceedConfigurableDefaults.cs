// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

// TODO: root location pointer is "" in the log file. Didn't I already change that to "root" (=> "<root>"?)?

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ReviewArraysThatExceedConfigurableDefaults : SarifValidationSkimmerBase
    {
        /// <summary>
        /// SARIF2020
        /// </summary>
        public override string Id => RuleId.ReviewArraysThatExceedConfigurableDefaults;

        // The GitHub Security Portal limits the number and size of results it displays. There are
        // limits on the number of runs per log file, rules per run, results per run, locations per
        // result, code flows per result, and steps per code flow. You can provide a configuration
        // file at the root of your repository to specify higher limits.
        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = RuleResources.SARIF2020_ReviewArraysThatExceedConfigurableDefaults_FullDescription_Text };

        protected override IEnumerable<string> MessageResourceNames => new string[] {
            nameof(RuleResources.SARIF2020_ReviewArraysThatExceedConfigurableDefaults_Error_Default_Text)
        };

        public override FailureLevel DefaultLevel => FailureLevel.Error;

        public override bool EnabledByDefault => false;

        // Some of the actual limits are impractically large for testing purposes, so make them
        // settable.
        //
        // We create a dictionary whose keys are pseudo-JSON pointers to arrays of limited size
        // and whose values are the corresponding limits. The dictionary keys cannot be simply
        // the array's property names, because those can collide (see below where two different
        // dictionaries have the same property name 'locations'). They can't be the "real" JSON
        // pointers, either, because those include varying array indices along the paths. So when
        // given a real JSON pointer, we'll replace all the indicies with 0s, and use the result
        // to look up the array's size limit.
        internal static readonly string s_runsPerLogKey = $"/{SarifPropertyName.Runs}";
        internal static readonly string s_rulesPerRunKey = $"{s_runsPerLogKey}/0/{SarifPropertyName.Tool}/{SarifPropertyName.Driver}/{SarifPropertyName.Rules}";
        internal static readonly string s_resultsPerRunKey = $"{s_runsPerLogKey}/0/{SarifPropertyName.Results}";
        internal static readonly string s_locationsPerResultKey = $"{s_resultsPerRunKey}/0/{SarifPropertyName.Locations}";
        internal static readonly string s_codeFlowsPerResultKey = $"{s_resultsPerRunKey}/0/{SarifPropertyName.CodeFlows}";
        internal static readonly string s_locationsPerThreadFlowKey = $"{s_codeFlowsPerResultKey}/0/{SarifPropertyName.ThreadFlows}/0/{SarifPropertyName.Locations}";

        internal static Dictionary<string, int> s_arraySizeLimitDictionary = new Dictionary<string, int>
        {
            [s_runsPerLogKey] = 5,
            [s_rulesPerRunKey] = 1000,
            [s_resultsPerRunKey] = 1000,
            [s_locationsPerResultKey] = 10,
            [s_codeFlowsPerResultKey] = 100,
            [s_locationsPerThreadFlowKey] = 100
        };

        protected override void Analyze(SarifLog sarifLog, string sarifLogPointer)
        {
            CheckArraySize(
                sarifLog.Runs?.Count,
                sarifLogPointer.AtProperty(SarifPropertyName.Runs));
        }

        protected override void Analyze(Run run, string runPointer)
        {
            CheckArraySize(
                run.Tool.Driver.Rules?.Count,
                runPointer
                    .AtProperty(SarifPropertyName.Tool)
                    .AtProperty(SarifPropertyName.Driver)
                    .AtProperty(SarifPropertyName.Rules));

            CheckArraySize(
                run.Results?.Count,
                runPointer.AtProperty(SarifPropertyName.Results));
        }

        protected override void Analyze(Result result, string resultPointer)
        {
            CheckArraySize(
                result.Locations?.Count,
                resultPointer.AtProperty(SarifPropertyName.Locations));

            CheckArraySize(
                result.CodeFlows?.Count,
                resultPointer.AtProperty(SarifPropertyName.CodeFlows));
        }

        protected override void Analyze(ThreadFlow threadFlow, string threadFlowPointer)
        {
            CheckArraySize(
                threadFlow.Locations?.Count,
                threadFlowPointer.AtProperty(SarifPropertyName.Locations));
        }

        private void CheckArraySize(int? actualSize, string arrayPointer)
        {
            string dictionaryKey = DictionaryKeyFromJsonPointer(arrayPointer);
            if (actualSize.HasValue && actualSize.Value > s_arraySizeLimitDictionary[dictionaryKey])
            {
                // {0}: This array contains {2} elements, which exceeds the default limit of {3}
                // imposed by the GitHub Developer Security Portal. The portal will only display
                // information up to that limit. You can provide a configuration file at the root
                // of your repository to specify a higher limit.
                LogResult(
                    arrayPointer,
                    nameof(RuleResources.SARIF2020_ReviewArraysThatExceedConfigurableDefaults_Error_Default_Text),
                    actualSize.Value.ToString(CultureInfo.InvariantCulture),
                    s_arraySizeLimitDictionary[dictionaryKey].ToString(CultureInfo.InvariantCulture));
            }
        }

        private const string ArrayIndexPattern = @"/\d+";
        private static readonly Regex s_arrayIndexRegex = new Regex(ArrayIndexPattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private string DictionaryKeyFromJsonPointer(string jsonPointer)
            => s_arrayIndexRegex.Replace(jsonPointer, "/0");
    }
}
