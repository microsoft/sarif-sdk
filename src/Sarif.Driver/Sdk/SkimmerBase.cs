// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class SkimmerBase<TContext>  : ISkimmer<TContext>
    {
        public SkimmerBase()
        {
            this.Options = new Dictionary<string, string>();
        }

        abstract public Uri HelpUri { get;  }

        private IDictionary<string, string> messageFormats;

        abstract protected ResourceManager ResourceManager { get; }

        abstract protected IEnumerable<string> FormatIds { get; }

        virtual public ResultLevel DefaultLevel { get { return ResultLevel.Warning; } }

        virtual public IDictionary<string, string> MessageFormats
        {
            get
            {
                if (this.messageFormats == null)
                {
                    this.messageFormats = InitializeMessageFormats();
                }
                return this.messageFormats;
            }
        }

        private Dictionary<string, string> InitializeMessageFormats()
        {
            return RuleUtilities.BuildDictionary(ResourceManager, FormatIds, Id);
        }

        abstract public string Id { get; }

        abstract public string FullDescription { get; }

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

        public IDictionary<string, string> Properties { get; }

        public IList<string> Tags { get; }

        public virtual void Initialize(TContext context) { }

        public virtual AnalysisApplicability CanAnalyze(TContext context, out string reasonIfNotApplicable)
        {
            reasonIfNotApplicable = null;
            return AnalysisApplicability.ApplicableToSpecifiedTarget;
        }

        public abstract void Analyze(TContext context);
    }
}
