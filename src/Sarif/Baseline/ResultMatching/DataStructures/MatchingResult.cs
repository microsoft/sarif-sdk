// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching
{
    public class MatchingResult
    {
        public string RuleId { get; set; }

        public Result Result { get; set; }

        public Rule Rule { get; set; }

        public Tool Tool { get; set; }

        public Run OriginalRun { get; set; }
    }
}
