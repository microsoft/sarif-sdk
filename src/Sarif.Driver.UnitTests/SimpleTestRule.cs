﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Composition;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Export(typeof(ReportingDescriptor)), Export(typeof(IOptionsProvider)), Export(typeof(Skimmer<TestAnalysisContext>))]
    internal class SimpleTestRule : TestRuleBase, IOptionsProvider
    {
        public override string Id => "TEST002";

        public override MultiformatMessageString FullDescription { get { return new MultiformatMessageString { Text = String.Empty }; } }

        public override void Analyze(TestAnalysisContext context)
        {
            if (context.Policy.GetProperty(Behaviors) == TestRuleBehaviors.LogError)
            {
                context.Logger.Log(this,
                    new Result()
                    {
                        RuleId = this.Id,
                        Level = FailureLevel.Error,
                        Message = new Message { Text = "Simple test rule message." }
                    });
            }
        }

        public IEnumerable<IOption> GetOptions()
        {
            return new IOption[] { Behaviors };
        }

        private const string AnalyzerName = "TEST002." + nameof(SimpleTestRule);

        public static PerLanguageOption<TestRuleBehaviors> Behaviors { get; } =
            new PerLanguageOption<TestRuleBehaviors>(
                AnalyzerName, nameof(TestRuleBehaviors), defaultValue: () => { return TestRuleBehaviors.None; });

    }
}
