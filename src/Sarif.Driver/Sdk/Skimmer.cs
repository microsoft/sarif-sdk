// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class Skimmer<TContext> : ReportingDescriptor
    {
        public Skimmer()
        {
            this.Options = new Dictionary<string, string>();
        }

        private IDictionary<string, MultiformatMessageString> multiformatMessageStrings;

        protected virtual ResourceManager ResourceManager => null;

        protected virtual IEnumerable<string> MessageResourceNames => throw new NotImplementedException();

        virtual public FailureLevel DefaultLevel { get { return FailureLevel.Warning; } }

        public override IDictionary<string, MultiformatMessageString> MessageStrings
        {
            get
            {
                if (this.multiformatMessageStrings == null)
                {
                    this.multiformatMessageStrings = InitializeMultiformatMessageStrings();
                }
                return this.multiformatMessageStrings;
            }
        }

        private Dictionary<string, MultiformatMessageString> InitializeMultiformatMessageStrings()
        {
            return (ResourceManager == null) ? null
                : RuleUtilities.BuildDictionary(ResourceManager, MessageResourceNames, ruleId: Id);
        }

        public override string Id => throw new InvalidOperationException($"The {nameof(Id)} property must be overridden in the SkimmerBase-derived class.");

        public override MultiformatMessageString FullDescription => throw new InvalidOperationException($"The {nameof(FullDescription)} property must be overridden in the SkimmerBase-derived class.");

        public override MultiformatMessageString ShortDescription => new MultiformatMessageString { Text = ExtensionMethods.GetFirstSentence(FullDescription.Text) };

        public override string Name => this.GetType().Name;

        public IDictionary<string, string> Options { get; }

        public virtual void Initialize(TContext context) { }

        public virtual SupportedPlatform SupportedPlatforms => SupportedPlatform.All;

        public virtual AnalysisApplicability CanAnalyze(TContext context, out string reasonIfNotApplicable)
        {
            reasonIfNotApplicable = null;
            return AnalysisApplicability.ApplicableToSpecifiedTarget;
        }

        public abstract void Analyze(TContext context);
    }
}
