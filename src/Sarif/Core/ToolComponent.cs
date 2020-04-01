// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A component of the analysis tool that was run, such as the primary driver or a plug-in.
    /// </summary>
    public partial class ToolComponent
    {
        private Dictionary<string, ReportingDescriptor> _cachedRulesById;
        private Dictionary<string, ReportingDescriptor> _cachedRulesByGuid;

        private void BuildRuleCaches()
        {
            _cachedRulesById = new Dictionary<string, ReportingDescriptor>();
            _cachedRulesByGuid = new Dictionary<string, ReportingDescriptor>();

            foreach (ReportingDescriptor r in this.Rules ?? Enumerable.Empty<ReportingDescriptor>())
            {
                if (r.Id != null) { _cachedRulesById[r.Id] = r; }
                if (r.Guid != null) { _cachedRulesByGuid[r.Guid] = r; }
            }
        }

        public ReportingDescriptor GetRuleById(string ruleId)
        {
            ReportingDescriptor rule = null;

            // Build lookup if not built or possibly out-of-date
            if (_cachedRulesById == null || !_cachedRulesById.TryGetValue(ruleId, out rule))
            {
                BuildRuleCaches();
                _cachedRulesById.TryGetValue(ruleId, out rule);
            }

            return rule;
        }

        public ReportingDescriptor GetRuleByGuid(string ruleGuid)
        {
            ReportingDescriptor rule = null;

            // Build lookup if not built or possibly out-of-date
            if (_cachedRulesByGuid?.TryGetValue(ruleGuid, out rule) != true)
            {
                BuildRuleCaches();
                _cachedRulesByGuid.TryGetValue(ruleGuid, out rule);
            }

            return rule;
        }

        public bool ShouldSerializeRules()
        {
            return this.Rules.HasAtLeastOneNonNullValue();
        }

        public bool ShouldSerializeNotifications()
        {
            return this.Notifications.HasAtLeastOneNonNullValue();
        }
    }
}
