// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public abstract class SkimmerBase<TContext>  : ISkimmer<TContext>
    {
        public SkimmerBase()
        {
            this.Options = new Dictionary<string, string>();
        }

        abstract public Uri HelpUri { get;  }

   
        private Dictionary<string, string> formatSpecifiers;

        abstract protected ResourceManager ResourceManager { get; }

        abstract protected IEnumerable<string> FormatSpecifierIds { get; }

        virtual public Dictionary<string, string> FormatSpecifiers
        {
            get
            {
                if (this.formatSpecifiers == null)
                {
                    this.formatSpecifiers = InitializeFormatSpecifiers();
                }
                return this.formatSpecifiers;
            }
        }

        private Dictionary<string, string> InitializeFormatSpecifiers()
        {
            return RuleUtilities.BuildDictionary(ResourceManager, FormatSpecifierIds, Id);
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

        public Dictionary<string, string> Options { get; }

        public Dictionary<string, string> Properties { get; }

        public IList<string> Tag { get; }

        public virtual void Initialize(TContext context) { }

        public virtual AnalysisApplicability CanAnalyze(TContext context, out string reasonIfNotApplicable)
        {
            reasonIfNotApplicable = null;
            return AnalysisApplicability.ApplicableToSpecifiedTarget;
        }

        public abstract void Analyze(TContext context);
    }
}
