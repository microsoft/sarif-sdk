// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Test.UnitTests.Sarif.Driver.Net48
{
    public class AnalyzeTestSkimmer : Skimmer<AnalyzeTestContext>
    {
        public override string Id => "AT1002";

        public override string Name => "DoesNothing";

        public override AnalysisApplicability CanAnalyze(AnalyzeTestContext context, out string reasonIfNotApplicable)
        {
            reasonIfNotApplicable = string.Empty;
            return AnalysisApplicability.ApplicableToSpecifiedTarget;
        }

        public override void Analyze(AnalyzeTestContext context)
        {
            // do nothing
        }
    }
}
