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
        static object s_syncRoot = new object();
        static JObject s_ruleMetadata;

        static JObject RuleMetadata
        {
            get
            {
                if (s_ruleMetadata == null)
                {
                    lock (s_syncRoot)
                    {
                        if (s_ruleMetadata == null)
                        {
                            Byte[] ruleLookupBytes = Resources.RuleLookup;
                            string ruleLookupText = Encoding.UTF8.GetString(ruleLookupBytes);
                            s_ruleMetadata = JObject.Parse(ruleLookupText);
                        }
                    }
                }

                return s_ruleMetadata;
            }
        }

        public static bool TryGetRule(this Run run, string ruleId, string ruleKey, out IRule rule)
        {
            rule = null;

            if (run != null && run.Rules != null && (ruleId != null || ruleKey != null))
            {
                Rule concreteRule = null;
                if (ruleKey != null)
                {
                    run.Rules.TryGetValue(ruleKey, out concreteRule);
                }
                else
                {
                    run.Rules.TryGetValue(ruleId, out concreteRule);
                }

                rule = concreteRule;
            }
            else if (ruleId != null)
            {
                // No rule in log file. 
                // If the rule is a PREfast rule. create a "fake" rule using the external rule metadata file.
                if (RuleMetadata[ruleId] != null)
                {
                    string ruleName = null;
                    if (RuleMetadata[ruleId]["heading"] != null)
                    {
                        ruleName = RuleMetadata[ruleId]["heading"].Value<string>();
                    }

                    Uri helpUri = null;
                    if (RuleMetadata[ruleId]["url"] != null)
                    {
                        helpUri = new Uri(RuleMetadata[ruleId]["url"].Value<string>());
                    }

                    if (ruleName != null || helpUri != null)
                    {
                        rule = new Rule(ruleId, ruleName, null, null, null, RuleConfiguration.Unknown, ResultLevel.Warning, helpUri, null);
                    }
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
