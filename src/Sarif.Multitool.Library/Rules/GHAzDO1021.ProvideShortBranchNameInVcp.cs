// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class ProvideShortBranchNameInVcp
        : SarifValidationSkimmerBase
    {
        private const string BranchPropertyName = "branch";

        private static readonly Regex s_refsPrefixRegex = new Regex(
            @"^refs/[^/]+/",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public override string Id => RuleId.GHAzDOProvideShortBranchNameInVcp;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString
        {
            Text = RuleResources.GHAzDO1021_ProvideShortBranchNameInVcp_FullDescription_Text
        };

        protected override ICollection<string> MessageResourceNames => new List<string>
        {
            nameof(RuleResources.GHAzDO1021_ProvideShortBranchNameInVcp_Error_Default_Text)
        };

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.GHAzDO });

        public ProvideShortBranchNameInVcp()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(VersionControlDetails versionControlDetails, string versionControlDetailsPointer)
        {
            string branch = versionControlDetails?.Branch;
            if (branch == null || !s_refsPrefixRegex.IsMatch(branch))
            {
                return;
            }

            string shortBranchName = s_refsPrefixRegex.Replace(branch, string.Empty, 1);

            LogResult(
                versionControlDetailsPointer.AtProperty(BranchPropertyName),
                nameof(RuleResources.GHAzDO1021_ProvideShortBranchNameInVcp_Error_Default_Text),
                branch,
                shortBranchName);
        }
    }
}
