// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class TestAnalyzeOptions : AnalyzeOptionsBase
    {
        public TestAnalyzeOptions()
        {
            Kind = new List<ResultKind> { ResultKind.Fail };
            Level = new List<FailureLevel?> { FailureLevel.Warning, FailureLevel.Error };
        }

        public TestRuleBehaviors TestRuleBehaviors { get; set; }

        public bool DisableCheck { get; set; }

        public string[] DefaultPlugInFilePaths { get; set; }
    }
}
