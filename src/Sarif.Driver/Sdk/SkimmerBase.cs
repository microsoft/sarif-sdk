// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class SkimmerBase<TContext>  : PropertyBagHolder, ISkimmer<TContext>
    {
        public SkimmerBase()
        {
            this.Options = new Dictionary<string, string>();
        }

        abstract public Uri HelpUri { get;  }

        abstract public string Help { get; }

        private IDictionary<string, string> messageTemplates;
        private IDictionary<string, string> richMessageTemplates;

        abstract protected ResourceManager ResourceManager { get; }

        abstract protected IEnumerable<string> TemplateResourceNames { get; }

        virtual protected IEnumerable<string> RichTemplateResourceNames => new List<string>();

        virtual public RuleConfiguration Configuration {  get; }

        virtual public ResultLevel DefaultLevel { get { return ResultLevel.Warning; } }

        virtual public IDictionary<string, string> MessageTemplates
        {
            get
            {
                if (this.messageTemplates == null)
                {
                    this.messageTemplates = InitializeMessageTemplates();
                }
                return this.messageTemplates;
            }
        }

        virtual public IDictionary<string, string> RichMessageTemplates
        {
            get
            {
                if (this.richMessageTemplates == null)
                {
                    this.richMessageTemplates = InitializeRichMessageTemplates();
                }
                return this.richMessageTemplates;
            }
        }

        private Dictionary<string, string> InitializeMessageTemplates()
        {
            return RuleUtilities.BuildDictionary(ResourceManager, TemplateResourceNames, ruleId: Id);
        }

        private Dictionary<string, string> InitializeRichMessageTemplates()
        {
            return RuleUtilities.BuildDictionary(ResourceManager, RichTemplateResourceNames,ruleId: Id, prefix: "Rich");
        }

        abstract public string Id { get; }

        abstract public string FullDescription { get; }

        abstract public string RichDescription { get; }

        public virtual string ShortDescription
        {
            get { return FirstSentence(FullDescription); }
        }

        internal static string FirstSentence(string fullDescription)
        {
            int charCount = 0;
            bool withinApostrophe = false;

            foreach (char ch in fullDescription)
            {
                charCount++;
                switch (ch)
                {
                    case '\'':
                    {
                        withinApostrophe = !withinApostrophe;
                        continue;
                    }

                    case '.':
                    {
                        if (withinApostrophe) { continue; }
                        return fullDescription.Substring(0, charCount);
                    }
                }
            }
            int length = Math.Min(fullDescription.Length, 80);
            bool truncated = length < fullDescription.Length;
            return fullDescription.Substring(0, length) + (truncated ? "..." : "");
        }

        public virtual string Name {  get { return this.GetType().Name; } }

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
