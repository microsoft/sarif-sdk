// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    internal abstract class TestRuleBase : PropertyBagHolder, IRule, ISkimmer<TestAnalysisContext>
    {
        public virtual SupportedPlatform SupportedPlatforms
        {
            get
            {
                return SupportedPlatform.All;
            }
        }

        public Uri HelpUri { get; set; }

        public abstract string Id { get; }        

        public virtual ResultLevel DefaultLevel { get { return ResultLevel.Warning; } }

        public virtual string Name { get { return this.GetType().Name; } }

        public virtual string FullDescription { get { return this.GetType().Name + " full description."; } }

        public virtual string ShortDescription { get { return this.GetType().Name + " short description."; } }

        public IDictionary<string, string> MessageFormats
        {
            get
            {
                return new Dictionary<string, string> { { nameof(SdkResources.NotApplicable_InvalidMetadata), SdkResources.NotApplicable_InvalidMetadata } };
            }
        }

        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        public RuleConfiguration Configuration
        {
            get
            {
                return RuleConfiguration.Enabled;
            }
        }

        public string RichDescription => throw new NotImplementedException();

        public IDictionary<string, string> MessageTemplates => throw new NotImplementedException();

        public IDictionary<string, string> RichMessageTemplates => throw new NotImplementedException();

        public string Help => throw new NotImplementedException();

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
