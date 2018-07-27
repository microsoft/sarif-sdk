// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.ResultMatching.HeuristicMatchers
{
    /// <summary>
    /// TODO--This is incomplete at this point in time and needs unit tests.
    /// </summary>
    internal class StructuralDataHeuristicMatcher : GenericHeuristicMatcher
    {
        public StructuralDataHeuristicMatcher() : base (StructuralDataEqualityComparator.Instance) {  }
        
        public class StructuralDataEqualityComparator : IResultMatchingComparer
        {
            public static readonly StructuralDataEqualityComparator Instance = new StructuralDataEqualityComparator();

            private IEqualityComparer<Stack> _stackEqualityComparer { get; }
            private IEqualityComparer<Graph> _graphEqualityComparer { get; }
            private IEqualityComparer<GraphTraversal> _graphTraversalComparer { get; }
            private IEqualityComparer<CodeFlow> _codeflowEqualityComparer { get; }

            public StructuralDataEqualityComparator() :
                this(StackEqualityComparer.Instance,
                    GraphEqualityComparer.Instance,
                    GraphTraversalEqualityComparer.Instance,
                    CodeFlowEqualityComparer.Instance
                    ) { }

            public StructuralDataEqualityComparator(IEqualityComparer<Stack> stackEqualityComparer, IEqualityComparer<Graph> graphEqualityComparer, IEqualityComparer<GraphTraversal> graphTraversalComparer, IEqualityComparer<CodeFlow> codeflowEqualityComparer)
            {
                _stackEqualityComparer = stackEqualityComparer;
                _graphEqualityComparer = graphEqualityComparer;
                _graphTraversalComparer = graphTraversalComparer;
                _codeflowEqualityComparer = codeflowEqualityComparer;
            }


            public bool Equals(MatchingResult x, MatchingResult y)
            {
                if (!ListStructureSame(x.Result.Stacks, y.Result.Stacks) || !ListContentsSame(x.Result.Stacks, y.Result.Stacks, _stackEqualityComparer))
                {
                    return false;
                }
                if (!ListStructureSame(x.Result.Graphs, y.Result.Graphs) 
                    || !ListStructureSame(x.Result.GraphTraversals, y.Result.GraphTraversals)
                    || !ListContentsSame(x.Result.Graphs, y.Result.Graphs, _graphEqualityComparer)
                    || !ListContentsSame(x.Result.GraphTraversals, y.Result.GraphTraversals, _graphTraversalComparer))
                {
                    return false;
                }
                if (!ListStructureSame(x.Result.CodeFlows, y.Result.CodeFlows) || !ListContentsSame(x.Result.CodeFlows, y.Result.CodeFlows, _codeflowEqualityComparer))
                {
                    return false;
                }

                return true;
            }

            private bool ListContentsSame<T>(IList<T> first, IList<T> second, IEqualityComparer<T> comparer)
            {
                HashSet<T> contents = new HashSet<T>(comparer);
                foreach (var element in first)
                {
                    contents.Add(element);
                }
                foreach (var element in second)
                {
                    if (!contents.Contains(element))
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool ListStructureSame<T>(IList<T> first, IList<T> second)
            {
                if(first != null && second == null || second != null && first == null)
                {
                    return false;
                }
                if (first != null && second != null && second.Count != first.Count)
                {
                    return false;
                }
                return true;
            }

            public int GetHashCode(MatchingResult obj)
            {
                int hash = -1348952307;

                foreach (var stack in obj.Result.Stacks)
                {
                    hash ^= _stackEqualityComparer.GetHashCode(stack);
                }
                foreach (var graph in obj.Result.Graphs)
                {
                    hash ^= _graphEqualityComparer.GetHashCode(graph);
                }

                foreach (var graphTraversal in obj.Result.GraphTraversals)
                {
                    hash ^= _graphTraversalComparer.GetHashCode(graphTraversal);
                }

                foreach (var codeflow in obj.Result.CodeFlows)
                {
                    hash ^= _codeflowEqualityComparer.GetHashCode(codeflow);
                }

                return hash;
            }

            public bool ResultMatcherApplies(MatchingResult result)
            {
                return (result.Result.Stacks != null && result.Result.Stacks.Any())
                    || (result.Result.Graphs != null && result.Result.Graphs.Any())
                    || (result.Result.GraphTraversals != null && result.Result.GraphTraversals.Any())
                    || (result.Result.CodeFlows != null && result.Result.CodeFlows.Any());
            }
        }
    }
}
