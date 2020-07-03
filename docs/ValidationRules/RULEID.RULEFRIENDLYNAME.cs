// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

/*
 * INSTRUCTIONS:
 * 
 * - Replace `RULEID` with a valid value.
 *      It should start with a prefix "SARIF", followed by 
 *      a 4 digit number which is the next sequential number available
 *      to use in ~\src\Sarif.Multitool\Rules\RuleId.cs file.
 *      Example:
 *          SARIF1023
 *
 * - Replace `RULEFRIENDLYNAME` with a valid value.
 *      RULEFRIENDLYNAME should concisely define the purpose of this rule.
 *      Use imperative language, like `UseAbsoluteUri`
 *      instead of indicative language, like `UriIsNotAbsolute`.
 *      Examples:
 *          DoNotUseFriendlyNameAsRuleId
 *          ReferToFinalSchema
 *
 * - Rename this file as <RULEID>.<RULEFRIENDLYNAME>.cs.
 * 
 * - Remove All INSTRUCTIONS after the changes have been made.
 */
namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class RULEFRIENDLYNAME : SarifValidationSkimmerBase
    {
        private readonly MultiformatMessageString FullDescription = new MultiformatMessageString
        {
            /*
             * INSTRUCTIONS:
             * Add a new key-value pair in ~\src\Sarif.Multitool\Rules\RuleResources.resx.
             * 
             * Key: 
             *      RULEID_RULEFRIENDLYNAME
             *      
             * Value: 
             *      Provide a brief description with atleast two sentences, both ending in a period.
             *      The first sentence should be a short description of the rule.
             *      The second sentence must describe the utility/usage of the rule.
             * 
             * Example:
             *      The $schema property should be present, and must refer to the final version 
             *      of the SARIF 2.1.0 schema. This enables IDEs to provide Intellisense for SARIF log files.
             * 
             * Notes:
             *      The first sentence will be used as a `ShortDescription` for this rule.
             *      All sentences together will be used as a `LongDescription` for this rule.
             */
            Text = RuleResources.RULEID_RULEFRIENDLYNAME
        };

        /*
         * INSTRUCTIONS:
         * Decide the appropriate FailureLevel for this rule as appropriate.
         * The following heuristics can be used to arrive at a decision:
         * 
         * Error:
         *      In general, an `Error` should be reserved for rules which address
         *      a SARIF spec violation that cannot be expressed by the Schema.
         *      Example:
         *      SARIF1019.RuleIdMustBePresentAndConsistent
         *      Per spec, at least one of result.ruleId and result.rule.id must be present, 
         *      and if both are present, they must be the same.
         * 
         * Warning:
         *      In general, a `Warning` is a good practice which should be followed, but it
         *      does not violate the SARIF spec.
         *      Example:
         *      SARIF1021.DoNotUseFriendlyNameAsRuleId
         */
        public override FailureLevel DefaultLevel => FailureLevel.Warning;

        /*
         * INSTRUCTIONS:
         * Add a new property in ~\src\Sarif.Multitool\Rules\RuleId.cs with 
         * the name as RULEFRIENDLYNAME and 
         * the value as RULEID.
         * 
         * Example:
         *      public const string ReferToFinalSchema = "SARIF1020";
         */
        public override string Id => RuleId.RULEFRIENDLYNAME;

        protected override IEnumerable<string> MessageResourceNames => new string[]
        {
            /*
            * INSTRUCTIONS:
            * Each rule has one or more result message strings, each with symbolic name 
            * in PascalCase. Add at least one new key-value pair for user messages
            * in ~\src\Sarif.Multitool\Rules\RuleResources.resx.
            *
            * Key: 
            *       RULEID_USERMESSAGESYMBOLICNAME
            * 
            * Value: 
            *      Provide the default user message for this rule. It should be a dynamic string
            *      and always start with `{0}:`.
            * 
            * Conditional user messages:
            *      If the rule requires more than one user messages to be defined, define each as a
            *      key-value pair in resx file. The keys should be named as: RULEID_SHORTDESCRIPTION
            * 
            * Example:
            * 
            *      Key      : SARIF1018_LacksTrailingSlash
            *      Value    : {0}: The URI '{1}' belonging to the '{2}' element of 
            *                 run.originalUriBaseIds does not end with a slash.
            * 
            * Notes:
            *       Provide a meaningful symbolic name for each message, even if there is only one 
            *       in this rule. Do not use a generic name like "default" for a single message.
            */
            nameof(RuleResources.RULEID_Default)
        };

        /*
         * INSTRUCTIONS:
         * Override the "Analyze" method for the SARIF property which needs analysis.
         * Here is an example:
         */
        protected override void Analyze(ReportingDescriptor reportingDescriptor, string reportingDescriptorPointer)
        {
            if (reportingDescriptor.Id != null &&
                reportingDescriptor.Name != null &&
                reportingDescriptor.Id.Equals(reportingDescriptor.Name, StringComparison.OrdinalIgnoreCase))
            {
                LogResult(
                    reportingDescriptorPointer,
                    nameof(RuleResources.SARIF1001_Default),
                    reportingDescriptor.Id);
            }
        }
    }
}
