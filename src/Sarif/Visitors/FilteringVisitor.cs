// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// A visitor that filters a specified SARIF log to create a new log containing only those
    /// results for which a specified predicate returns true, and only those elements of run-level
    /// collections such as Run.Artifacts that are relevant to the filtered results.
    /// </summary>
    public class FilteringVisitor : SarifRewritingVisitor
    {
        /// <summary>
        /// A delgate for a function that returns true if the specified result should be included
        /// in the filtered log, otherwise false.
        /// </summary>
        /// <param name="result">
        /// The result to be tested.
        /// </param>
        public delegate bool IncludeResultPredicate(Result result);

        private readonly IncludeResultPredicate predicate;

        private Run currentRun;
        bool includeReferencedObjects = true;

        private IList<Result> filteredResults;
        private IList<Artifact> filteredArtifacts;
        private IDictionary<int, int> remappedArtifactIndexDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilteringVisitor"/> class.
        /// </summary>
        /// <param name="predicate">
        /// A predicate that selects the results in the filtered log file.
        /// </param>
        public FilteringVisitor(IncludeResultPredicate predicate)
        {
            this.predicate = predicate;
        }

        public override Run VisitRun(Run node)
        {
            currentRun = node;

            filteredResults = new List<Result>();
            filteredArtifacts = new List<Artifact>();
            remappedArtifactIndexDictionary = new Dictionary<int, int>();

            Run visitedRun = base.VisitRun(node);

            visitedRun.Results = filteredResults;
            visitedRun.Artifacts = filteredArtifacts;

            return visitedRun;
        }

        public override Result VisitResult(Result node)
        {
            includeReferencedObjects = false;
            if (predicate(node))
            {
                includeReferencedObjects = true;
                filteredResults.Add(node);
            }

            return base.VisitResult(node);
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (includeReferencedObjects)
            {
                if (node.Index >= 0)
                {
                    if (!remappedArtifactIndexDictionary.ContainsKey(node.Index))
                    {
                        int newIndex = filteredArtifacts.Count;
                        remappedArtifactIndexDictionary.Add(node.Index, newIndex);
                        filteredArtifacts.Add(currentRun.Artifacts[node.Index]);
                    }

                    node.Index = remappedArtifactIndexDictionary[node.Index];
                }
            }

            return base.VisitArtifactLocation(node);
        }
    }
}

