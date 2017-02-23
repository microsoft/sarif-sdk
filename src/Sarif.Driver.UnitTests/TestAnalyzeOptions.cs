// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class TestAnalyzeOptions : AnalyzeOptionsBase
    {
        public TestAnalyzeOptions()
        {
            RegardAnalysisTargetAsValid = true;
        }

        public bool RegardAnalysisTargetAsNotApplicable { get; set; }

        public bool RegardAnalysisTargetAsCorrupted { get; set; }

        public bool RegardAnalysisTargetAsValid { get; set; }

        public bool RegardOptionsAsInvalid { get; set; }

        public string[] DefaultPlugInFilePaths { get; set; }
    }
}
