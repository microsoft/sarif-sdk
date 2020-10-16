// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Composition;
using System.Resources;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Driver.Sdk;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// This test class exists to provide an additional check that can be disabled via context, etc. It has no configurable behavior.
    /// </summary>
    [Export(typeof(ReportingDescriptor)), Export(typeof(IOptionsProvider)), Export(typeof(Skimmer<TestAnalysisContext>))]
    internal class FunctionlessTestRule : TestRuleBase, IOptionsProvider
    {
        private const string FunctionlessTestRuleId = "TEST1002";

        public override string Id => FunctionlessTestRuleId;

        protected override ResourceManager ResourceManager => SkimmerBaseTestResources.ResourceManager;

        protected override IEnumerable<string> MessageResourceNames => new List<string>
        {
        };

        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = "This is the full description for TEST1002" };

        public override void Analyze(TestAnalysisContext context)
        {
        }

        public IEnumerable<IOption> GetOptions()
        {
            return new IOption[] { Behaviors, UnusedOption };
        }

        private const string AnalyzerName = FunctionlessTestRuleId + "." + nameof(FunctionlessTestRule);

        public static PerLanguageOption<TestRuleBehaviors> Behaviors { get; } =
            new PerLanguageOption<TestRuleBehaviors>(
                AnalyzerName, nameof(TestRuleBehaviors), defaultValue: () => TestRuleBehaviors.None);

        public static PerLanguageOption<bool> UnusedOption { get; } =
            new PerLanguageOption<bool>(
                AnalyzerName, nameof(TestRuleBehaviors), () => true);
    }
}
