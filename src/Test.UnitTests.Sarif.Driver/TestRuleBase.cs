// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    internal abstract class TestRuleBase : Skimmer<TestAnalysisContext>
    {
        protected ReportingConfiguration _reportingConfiguration = null;

        public override SupportedPlatform SupportedPlatforms
        {
            get
            {
                return SupportedPlatform.All;
            }
        }

        public override FailureLevel DefaultLevel { get { return FailureLevel.Warning; } }

        public override string Name => this.GetType().Name;

        public override MultiformatMessageString FullDescription { get { return new MultiformatMessageString { Text = this.GetType().Name + " full description." }; } }

        public override MultiformatMessageString ShortDescription { get { return new MultiformatMessageString { Text = this.GetType().Name + " short description." }; } }

        public override ReportingConfiguration DefaultConfiguration
        {
            get
            {
                if (_reportingConfiguration == null)
                {
                    _reportingConfiguration = new ReportingConfiguration();
                }

                return _reportingConfiguration;
            }
        }

        public override MultiformatMessageString Help { get { return new MultiformatMessageString() { Text = "[Empty]" }; } }

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
