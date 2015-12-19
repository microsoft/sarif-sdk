// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class TestAnalyzeOptions : IAnalyzeOptions
    {
        public TestAnalyzeOptions()
        {
            RegardAnalysisTargetAsValid = true;
        }

        public bool ComputeTargetsHash { get; set; }

        public string OutputFilePath { get; set; }

        public IList<string> PlugInFilePaths { get; set;  }


        public string PolicyFilePath { get; set; }

        public bool Recurse { get; set; }

        public bool Statistics { get; set; }

        public IEnumerable<string> TargetFileSpecifiers { get; set; }

        public bool Verbose { get; set; }

        public bool RegardAnalysisTargetAsValid { get; set; }
    }
}
