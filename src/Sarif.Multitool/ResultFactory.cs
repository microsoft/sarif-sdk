// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Json.Schema;
using Microsoft.Json.Schema.JsonSchemaValidator.Sarif;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    internal class ResultFactory
    {
        private const string ErrorCodeFormat = "JS{0:D4}";

        internal static Result CreateResult(JToken jToken, ErrorNumber errorNumber, object[] args)
        {
            IJsonLineInfo lineInfo = jToken;

            return CreateResult(
                lineInfo.LineNumber,
                lineInfo.LinePosition,
                jToken.Path,
                errorNumber,
                args);
        }

        internal static Result CreateResult(
            int startLine,
            int startColumn,
            string jsonPath,
            ErrorNumber errorNumber,
            params object[] args)
        {
            Rule rule = RuleFactory.GetRuleFromErrorNumber(errorNumber);

            var messageArguments = new List<string> { jsonPath };
            messageArguments.AddRange(args.Select(a => a.ToString()));

            var result = new Result
            {
                RuleId = rule.Id,
                Level = rule.DefaultLevel,
                Locations = new List<Location>
                {
                    new Location
                    {
                        AnalysisTarget = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = startLine,
                                StartColumn = startColumn
                            }
                        }
                    }
                },

                FormattedRuleMessage = new FormattedRuleMessage
                {
                    FormatId = RuleFactory.DefaultMessageFormatId,
                    Arguments = messageArguments
                }
            };

            result.SetProperty("jsonPath", jsonPath);

            return result;
        }

        internal static string RuleIdFromErrorNumber(ErrorNumber errorNumber)
        {
            return string.Format(CultureInfo.InvariantCulture, ErrorCodeFormat, (int)errorNumber);
        }
    }
}
