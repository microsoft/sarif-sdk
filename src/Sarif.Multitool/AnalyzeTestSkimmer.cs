#if DEBUG
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class AnalyzeTestSkimmer : Skimmer<AnalyzeTestContext>
    {
        public override string Id => "AT1001";

        public override string Name => "FiresOnEveryTarget";

        protected override IEnumerable<string> MessageResourceNames => new string[] {
                    nameof(MultitoolResources.AT1001_Error_FiredAnError)
                };

        public override MultiformatMessageString FullDescription => new MultiformatMessageString { Text = MultitoolResources.AT1001_FiresOnEveryTarget_Description };

        protected override ResourceManager ResourceManager => MultitoolResources.ResourceManager;

        public override AnalysisApplicability CanAnalyze(AnalyzeTestContext context, out string reasonIfNotApplicable)
        {
            reasonIfNotApplicable = null;
            return AnalysisApplicability.ApplicableToSpecifiedTarget;
        }

        public override void Analyze(AnalyzeTestContext context)
        {
            context.Logger.Log(context.Rule, RuleUtilities.BuildResult(FailureLevel.Error, context, null,
                nameof(MultitoolResources.AT1001_Error_FiredAnError),
                context.CurrentTarget.Uri.GetFileName()));
        }
    }
}
#endif
