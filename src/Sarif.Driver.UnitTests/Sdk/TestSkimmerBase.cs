using System;
using System.Collections.Generic;
using System.Resources;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    // This class overrides all the abstract members of SkimmerBase. As a result,
    // when we want to write a set of tests that implement only a limited number
    // of members from SkimmerBase, we don't have to reimplement all the other
    // members that we don't care about.
    public abstract class TestSkimmerBase : SkimmerBase<TestAnalysisContext>
    {
        public override Uri HelpUri => throw new NotImplementedException();

        public override string Help => throw new NotImplementedException();

        public override string Id => throw new NotImplementedException();

        public override string FullDescription => throw new NotImplementedException();

        public override string RichDescription => throw new NotImplementedException();

        protected override ResourceManager ResourceManager => throw new NotImplementedException();

        protected override IEnumerable<string> TemplateResourceNames => throw new NotImplementedException();

        protected override IEnumerable<string> RichTemplateResourceNames => throw new NotImplementedException();

        // Most of the members of this class throw NotImplementedException so that,
        // if you write a test that requires that member, you will be reminded to
        // implement it. But Analyze must not throw an exception. That's because the
        // tests in class AnalyzeCommandBaseTests use reflection to locate all the
        // ISkimmer-derived classes in this assembly, and ultimately call Analyze
        // on all of them. Those tests will fail if the Analyze method throws an
        // exception.
        public override void Analyze(TestAnalysisContext context) { }
    }
}
