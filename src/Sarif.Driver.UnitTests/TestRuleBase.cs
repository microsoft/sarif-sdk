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

        public virtual Message Name { get { return new Message { Text = this.GetType().Name }; } }

        public virtual Message FullDescription { get { return new Message { Text = this.GetType().Name + " full description." }; } }

        public virtual Message ShortDescription { get { return new Message { Text = this.GetType().Name + " short description." }; } }

        public IDictionary<string, string> MessageFormats
        {
            get
            {
                return new Dictionary<string, string> { { nameof(SdkResources.NotApplicable_InvalidMetadata), SdkResources.NotApplicable_InvalidMetadata } };
            }
        }

        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        public RuleConfiguration Configuration { get { return new RuleConfiguration(); } }

        public string RichDescription => throw new NotImplementedException();

        public IDictionary<string, string> MessageStrings { get { return new Dictionary<string, string>(); } }

        public IDictionary<string, string> RichMessageStrings { get { return new Dictionary<string, string>(); } }

        public Message Help => throw new NotImplementedException();

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
