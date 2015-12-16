// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    internal static class MemberHoister
    {
        public static DataModel HoistTypes(DataModel source)
        {
            ImmutableSortedDictionary<string, HoistAction> hoistMap = GetViableHoistEdgeList(source);
            if (hoistMap.Count == 0)
            {
                return source;
            }

            hoistMap = RemoveCycles(hoistMap);
            hoistMap = ApplyTransitiveProperty(hoistMap);
            var newTypes = new List<DataModelType>();
            foreach (DataModelType sourceType in source.Types)
            {
                if (hoistMap.ContainsKey(sourceType.G4DeclaredName))
                {
                    // This type is being eliminated
                    continue;
                }

                bool anythingDifferent = false;
                ImmutableArray<DataModelMember>.Builder newMembers = ImmutableArray.CreateBuilder<DataModelMember>();
                foreach (DataModelMember sourceMember in sourceType.Members)
                {
                    HoistAction todo;
                    if (hoistMap.TryGetValue(sourceMember.DeclaredName, out todo))
                    {
                        anythingDifferent = true;
                        newMembers.Add(new DataModelMember(
                            todo.Becomes,
                            sourceMember.CSharpName,
                            sourceMember.SerializedName,
                            sourceMember.SummaryText,
                            sourceMember.ArgumentName,
                            sourceMember.Pattern,
                            sourceMember.Minimum,
                            sourceMember.MinItems,
                            sourceMember.UniqueItems,
                            sourceMember.Default,
                            sourceMember.Rank + todo.AddedRanks,
                            sourceMember.Required
                            ));
                    }
                    else
                    {
                        newMembers.Add(sourceMember);
                    }
                }

                if (anythingDifferent)
                {
                    newTypes.Add(new DataModelType(
                        sourceType.RootObject,
                        sourceType.SummaryText,
                        sourceType.RemarksText,
                        sourceType.G4DeclaredName,
                        sourceType.CSharpName,
                        sourceType.InterfaceName,
                        newMembers.ToImmutable(),
                        ImmutableArray<string>.Empty,
                        ImmutableArray<string>.Empty,
                        ImmutableArray<ToStringEntry>.Empty,
                        sourceType.Base,
                        sourceType.Kind
                        ));
                }
                else
                {
                    newTypes.Add(sourceType);
                }
            }

            return new DataModel(source.SourceFilePath, source.MetaData, newTypes);
        }

        public static ImmutableSortedDictionary<string, HoistAction> ApplyTransitiveProperty(ImmutableSortedDictionary<string, HoistAction> edges)
        {
            ImmutableSortedDictionary<string, HoistAction>.Builder builder = edges.ToBuilder();

            var keys = new List<KeyValuePair<string, HoistAction>>();
            foreach (string startingKey in edges.Keys)
            {
                keys.Clear();
                string currentKey = startingKey;
                HoistAction currentValue;
                while (builder.TryGetValue(currentKey, out currentValue))
                {
                    keys.Add(Pair.Make(currentKey, currentValue));
                    currentKey = currentValue.Becomes;
                }

                Debug.Assert(keys.Count > 0);
                if (keys.Count <= 1)
                {
                    // No transitive behavior to apply
                    continue;
                }

                string becomes = keys[keys.Count - 1].Value.Becomes;

                int addedRanks = keys[keys.Count - 1].Value.AddedRanks;
                for (int idx = keys.Count - 2; idx >= 0; --idx)
                {
                    KeyValuePair<string, HoistAction> key = keys[idx];
                    addedRanks += key.Value.AddedRanks;
                    builder[key.Key] = new HoistAction(becomes, addedRanks);
                }
            }

            return builder.ToImmutable();
        }

        public static ImmutableSortedDictionary<string, HoistAction> RemoveCycles(ImmutableSortedDictionary<string, HoistAction> edges)
        {
            ImmutableSortedDictionary<string, HoistAction>.Builder builder = edges.ToBuilder();
            foreach (KeyValuePair<string, HoistAction> currentEdge in edges)
            {
                string targetKey = FindCycle(builder, currentEdge);
                if (targetKey != null)
                {
                    HoistAction nextKey;
                    while (builder.TryGetValue(targetKey, out nextKey))
                    {
                        builder.Remove(targetKey);
                        targetKey = nextKey.Becomes;
                    }
                }
            }

            return builder.ToImmutable();
        }

        private static string FindCycle(IReadOnlyDictionary<string, HoistAction> edges, KeyValuePair<string, HoistAction> startEdge)
        {
            var cycleDetector = new HashSet<string>();
            cycleDetector.Add(startEdge.Key);

            string currentEdgeSource = startEdge.Key;
            HoistAction currentEdgeTarget = startEdge.Value;
            do
            {
                if (!cycleDetector.Add(currentEdgeTarget.Becomes))
                {
                    return currentEdgeTarget.Becomes;
                }

                currentEdgeSource = currentEdgeTarget.Becomes;
            }
            while (edges.TryGetValue(currentEdgeSource, out currentEdgeTarget));

            // no cycle detected
            return null;
        }

        private static ImmutableSortedDictionary<string, HoistAction> GetViableHoistEdgeList(DataModel source)
        {
            ImmutableSortedDictionary<string, HoistAction>.Builder hoistMap = ImmutableSortedDictionary.CreateBuilder<string, HoistAction>();
            foreach (DataModelType candidate in source.Types)
            {
                if (candidate.HasBase)
                {
                    // Can't be hoist because this would break inheritance chains
                    continue;
                }

                if (candidate.Members.Length != 1)
                {
                    // Can't be hoist because the member can't replace the type
                    continue;
                }

                DataModelMember toBe = candidate.Members[0];
                hoistMap.Add(candidate.G4DeclaredName, new HoistAction(toBe.DeclaredName, toBe.Rank));
            }

            return hoistMap.ToImmutable();
        }
    }
}
