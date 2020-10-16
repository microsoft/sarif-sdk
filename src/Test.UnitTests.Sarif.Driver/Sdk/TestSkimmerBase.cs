// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Driver.Sdk
{
    // This class overrides all the abstract members of SkimmerBase. As a result,
    // when we want to write a set of tests that implement only a limited number
    // of members from SkimmerBase, we don't have to reimplement all the other
    // members that we don't care about.
    public abstract class TestSkimmerBase : Skimmer<TestAnalysisContext>
    {
        public override Uri HelpUri => throw new NotImplementedException();

        public override MultiformatMessageString Help => throw new NotImplementedException();

        public override string Id => throw new NotImplementedException();

        public override IList<string> DeprecatedIds => throw new NotImplementedException();

        public override MultiformatMessageString FullDescription => throw new NotImplementedException();

        protected override ResourceManager ResourceManager => throw new NotImplementedException();

        protected override IEnumerable<string> MessageResourceNames => throw new NotImplementedException();

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
