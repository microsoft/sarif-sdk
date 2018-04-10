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

        abstract public Message Help { get; }

        private IDictionary<string, string> messageStrings;
        private IDictionary<string, string> richMessageStrings;

        abstract protected ResourceManager ResourceManager { get; }

        abstract protected IEnumerable<string> MessageResourceNames { get; }

        virtual protected IEnumerable<string> RichMessageResourceNames => new List<string>();

        virtual public RuleConfiguration Configuration {  get; }

        virtual public ResultLevel DefaultLevel { get { return ResultLevel.Warning; } }

        virtual public IDictionary<string, string> MessageStrings
        {
            get
            {
                if (this.messageStrings == null)
                {
                    this.messageStrings = InitializeMessageStrings();
                }
                return this.messageStrings;
            }
        }

        virtual public IDictionary<string, string> RichMessageStrings
        {
            get
            {
                if (this.richMessageStrings == null)
                {
                    this.richMessageStrings = InitializeRichMessageStrings();
                }
                return this.richMessageStrings;
            }
        }

        private Dictionary<string, string> InitializeMessageStrings()
        {
            return RuleUtilities.BuildDictionary(ResourceManager, MessageResourceNames, ruleId: Id);
        }

        private Dictionary<string, string> InitializeRichMessageStrings()
        {
            return RuleUtilities.BuildDictionary(ResourceManager, RichMessageResourceNames,ruleId: Id, prefix: "Rich");
        }

        abstract public string Id { get; }

        abstract public Message FullDescription { get; }

        public virtual Message ShortDescription
        {
            get { return new Message { Text = FirstSentence(FullDescription.Text) }; }
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

        public virtual Message Name {  get { return new Message { Text = this.GetType().Name }; } }

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
