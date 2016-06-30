// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class RunExtensions
    {
        public static bool TryGetRule(this Run run, string ruleId, string ruleKey, out IRule rule)
        {
            rule = null;

            if (run != null && run.Rules != null)
            {
                if (ruleKey != null)
                {
                    rule = run.Rules[ruleKey];
                }
                else
                {
                    rule = run.Rules[ruleId];
                }
            }
            // No rule in log file, created "fake" rule
            else
            {
                Byte[] ruleLookupBytes = Resource1.ruleLookup;
                string ruleLookupText = Encoding.UTF8.GetString(ruleLookupBytes);
                JObject metadata = JObject.Parse(ruleLookupText);

                if (metadata[ruleId] != null)
                {
                    string ruleName = metadata[ruleId]["heading"].Value<string>();
                    Uri helpUri = new Uri(metadata[ruleId]["url"].Value<string>());
                    rule = new Rule(ruleId, ruleName, null, null, null, ResultLevel.Unknown, helpUri, null);
                }
            }

            return rule != null;
        }

        public static string GetToolName(this Run run)
        {
            if (run == null || run.Tool == null)
            {
                return null;
            }

            return run.Tool.FullName ?? run.Tool.Name;
        }
    }
}
