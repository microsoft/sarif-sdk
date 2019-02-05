// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Json.Schema.Validation.Private
{
    public partial class ResultFactory
    {
        public static Result CreateResult(JToken jToken, ErrorNumber errorNumber, object[] args)
        {
            IJsonLineInfo lineInfo = jToken;

            return CreateResult(
                lineInfo.LineNumber,
                lineInfo.LinePosition,
                jToken.Path,
                errorNumber,
                args);
        }

        public static Result CreateResult(
            int startLine,
            int startColumn,
            string jsonPath,
            ErrorNumber errorNumber,
            params object[] args)
        {
            MessageDescriptor rule = RuleFactory.GetRuleFromErrorNumber(errorNumber);

            var messageArguments = new List<string> { jsonPath };
            messageArguments.AddRange(args.Select(a => a.ToString()));

            var result = new Result
            {
                RuleId = rule.Id,
                Level = rule.DefaultConfiguration.Level,
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            Region = new Region
                            {
                                StartLine = startLine,
                                StartColumn = startColumn
                            }
                        }
                    }
                },

                Message = new Message
                {
                    MessageId = RuleFactory.DefaultRuleMessageId,
                    Arguments = messageArguments
                },
            };

            result.SetProperty("jsonPath", jsonPath);

            return result;
        }

        internal static string RuleIdFromErrorNumber(ErrorNumber errorNumber)
        {
            return string.Format(CultureInfo.InvariantCulture, RuleFactory.ErrorCodeFormat, (int)errorNumber);
        }
    }
}
