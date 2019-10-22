// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{

    /// <summary>
    /// A visitor that filters a specified SARIF log to create a new log containing only those
    /// results for which a specified predicate returns true, and only those elements of run-level
    /// collections such as Run.Artifacts that are relevant to the filtered results.
    /// </summary>
    public class FilteringVisitor : SarifRewritingVisitor
    {
        private readonly SarifPartitioner.FilteringPredicate predicate;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilteringVisitor"/> class.
        /// </summary>
        /// <param name="predicate">
        /// A predicate that selects the results in the filtered log file.
        /// </param>
        public FilteringVisitor(SarifPartitioner.FilteringPredicate predicate)
        {
            this.predicate = predicate;
        }

        public override Run VisitRun(Run node)
        {
            return base.VisitRun(node);
        }
    }
}

