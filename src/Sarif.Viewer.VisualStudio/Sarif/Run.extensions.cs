// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
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
