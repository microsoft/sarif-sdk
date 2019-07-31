// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public abstract class SplittingVisitor : SarifRewritingVisitor
    {
        public SplittingVisitor(Func<Result, bool> filteringStrategy = null)
        {
            FilteringStrategy = filteringStrategy ?? FilteringStrategies.NewOrUnbaselined;
        }

        protected Run CurrentRun { get; set; }

        protected Func<Result, bool> FilteringStrategy { get; set; }

        public IList<SarifLog> SplitSarifLogs { get; private set; }

        // Each run will drive creation of a single SarifLog instance.
        public override Run VisitRun(Run node)
        {
            CurrentRun = node;
            SplitSarifLogs = new List<SarifLog>();

            return base.VisitRun(node);
        }
    }
}
