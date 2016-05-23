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
        public static bool TryGetRule(this Run run, string ruleId, out IRule rule)
        {
            rule = null;

            if (run != null && run.Rules != null)
            {
                rule = run.Rules.Values.FirstOrDefault(r => r.Id == ruleId);
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
