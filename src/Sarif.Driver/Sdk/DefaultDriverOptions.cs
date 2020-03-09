// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    /// <summary>
    /// Options that cut across all rules that are provided by a driver. Currently restricted to a 
    /// single setting that allows for rules to be explicitly disabled. In the future, this 
    /// same mechanism will be improved to allow configuration to alter the warning level
    /// for a result.
    /// </summary>
    public class DefaultDriverOptions : IOptionsProvider
    {
        public static DefaultDriverOptions Instance = new DefaultDriverOptions();

        private DefaultDriverOptions()
        {
        }

        /// <summary>
        /// Enable namespace import optimization.
        /// </summary>
        public static PerLanguageOption<RuleEnabledState> RuleEnabled { get; } =
            new PerLanguageOption<RuleEnabledState>(
                feature: "DefaultDriverOptions",
                name: nameof(RuleEnabled),
                defaultValue: () => { return RuleEnabledState.Default; },
                description:
                @"Enabled state of rule. Valid values: Default, Disabled, Warning, Error. A rule in the 'Default' " +
                @"state will raise all issues as errors or warnings according to how the issue is logged.");

        public static PerLanguageOption<T> CreateRuleSpecificOption<T>(ReportingDescriptor rule, PerLanguageOption<T> option)
        {
            // This helper returns a copy of a rule option that is qualified by a new feature name constructed
            // from an arbitrary rule instance. This allows users to create a generic property descriptor
            // that is further qualified (by feature name) to be associated with a different check.
            return new PerLanguageOption<T>(
                feature: rule.Id + "." + rule.Name,
                name: option.Name,
                defaultValue: option.DefaultValue,
                description: option.Description);
        }

        public IEnumerable<IOption> GetOptions()
        {
            return new IOption[]
            {
                RuleEnabled
            };
        }
    }
}
