// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Driver.Sdk;

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    internal abstract class TestRuleBase : Skimmer<TestAnalysisContext>
    {
        protected TestRuleBase(string ruleId, string fullDescriptionText, IEnumerable<string> messageResourceNames = null)
            : base(BuildRule(ruleId, fullDescriptionText, messageResourceNames))
        {
            Name = this.GetType().Name;
            FullDescription = new MultiformatMessageString { Text = GetType().Name + " full description." };
            ShortDescription = new MultiformatMessageString { Text = GetType().Name + " short description." };
        }

        private static ReportingDescriptor BuildRule(string ruleId, string fullDescriptionText, IEnumerable<string> messageResourceNames)
        {
            return new ReportingDescriptor()
            {
                Id = ruleId,
                FullDescription = new MultiformatMessageString() { Text = fullDescriptionText },
                Help = new MultiformatMessageString() { Text = "[Empty]" },
                MessageStrings = RuleUtilities.BuildDictionary(SkimmerBaseTestResources.ResourceManager, messageResourceNames, ruleId: ruleId),
                DefaultConfiguration = new ReportingConfiguration()
                {
                    Level = FailureLevel.Error,
                    Enabled = true
                }
            };
        }

        public override SupportedPlatform SupportedPlatforms
        {
            get
            {
                return SupportedPlatform.All;
            }
        }

        public override AnalysisApplicability CanAnalyze(TestAnalysisContext context, out string reasonIfNotApplicable)
        {
            reasonIfNotApplicable = null;
            return AnalysisApplicability.ApplicableToSpecifiedTarget;
        }

        public override void Initialize(TestAnalysisContext context)
        {
        }
    }
}
