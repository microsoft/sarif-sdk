// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    internal abstract class TestRuleBase : Rule, ISkimmer<TestAnalysisContext>
    {
        protected RuleConfiguration _ruleConfiguration = null;

        public virtual SupportedPlatform SupportedPlatforms
        {
            get
            {
                return SupportedPlatform.All;
            }
        }

        public virtual ResultLevel DefaultLevel { get { return ResultLevel.Warning; } }

        public override Message Name { get { return new Message { Text = this.GetType().Name }; } }

        public override Message FullDescription { get { return new Message { Text = this.GetType().Name + " full description." }; } }

        public override Message ShortDescription { get { return new Message { Text = this.GetType().Name + " short description." }; } }

        public IDictionary<string, string> MessageFormats
        {
            get
            {
                return new Dictionary<string, string> { { nameof(SdkResources.NotApplicable_InvalidMetadata), SdkResources.NotApplicable_InvalidMetadata } };
            }
        }

        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        public override RuleConfiguration Configuration
        {
            get
            {
                if (_ruleConfiguration == null)
                {
                    _ruleConfiguration = new RuleConfiguration();
                }

                return _ruleConfiguration;
            }
        }

        public override IDictionary<string, string> MessageStrings { get { return new Dictionary<string, string>(); } }

        public override IDictionary<string, string> RichMessageStrings { get { return new Dictionary<string, string>(); } }

        public override Message Help { get { return new Message() { Text = "[Empty]" }; } }

        public abstract void Analyze(TestAnalysisContext context);

        public virtual AnalysisApplicability CanAnalyze(TestAnalysisContext context, out string reasonIfNotApplicable)
        {
            reasonIfNotApplicable = null;
            return AnalysisApplicability.ApplicableToSpecifiedTarget;
        }

        public virtual void Initialize(TestAnalysisContext context)
        {

        }
    }
}
