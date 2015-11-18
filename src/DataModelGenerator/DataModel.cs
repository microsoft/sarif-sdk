// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Driver;

namespace Microsoft.CodeAnalysis.DataModelGenerator
{
    internal class DataModel : IEquatable<DataModel>
    {
        public readonly string SourceFilePath;
        public readonly DataModelMetadata MetaData;
        private readonly ImmutableSortedDictionary<string, DataModelType> _g4Lookup;

        public DataModel(string sourceFilePath, DataModelMetadata metaData, IEnumerable<DataModelType> types)
        {
            this.SourceFilePath = sourceFilePath;
            this.MetaData = metaData;

            _g4Lookup = ImmutableSortedDictionary.CreateRange(
                StringComparer.Ordinal, types.Select(type => Pair.Make(type.G4DeclaredName, type)));
            ImmutableSortedDictionary<string, DataModelType>.Builder csTypes = ImmutableSortedDictionary.CreateBuilder<string, DataModelType>(StringComparer.Ordinal);
            foreach (DataModelType type in _g4Lookup.Values)
            {
                string key = type.CSharpName;
                if (csTypes.ContainsKey(key))
                {
                    csTypes[key] = null;
                }
                else
                {
                    csTypes.Add(key, type);
                }
            }
        }

        public IEnumerable<DataModelType> Types
        {
            get
            {
                return _g4Lookup.Values;
            }
        }

        public string Name
        {
            get
            {
                return this.MetaData.Name;
            }
        }

        public DataModelType GetTypeByG4Name(string g4Name)
        {
            DataModelType answer;
            _g4Lookup.TryGetValue(g4Name, out answer);
            return answer;
        }

        public ImmutableArray<string> GetKnownTypesFor(DataModelType type)
        {
            string cSharpName = type.CSharpName;
            ImmutableArray<string>.Builder result = ImmutableArray.CreateBuilder<string>();
            foreach (DataModelType thisType in _g4Lookup.Values)
            {
                if (thisType.Base == cSharpName)
                {
                    result.Add(thisType.CSharpName);
                }
            }

            return result.ToImmutable().Sort(StringComparer.Ordinal);
        }

        public DataModelType GetTypeForMember(DataModelMember member)
        {
            return this.GetTypeByG4Name(member.DeclaredName);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as DataModel);
        }

        public bool Equals(DataModel obj)
        {
            return obj != null
                && this.SourceFilePath.Equals(obj.SourceFilePath, StringComparison.OrdinalIgnoreCase)
                && this.MetaData.Equals(obj.MetaData)
                && this.Types.SequenceEqual(obj.Types);
        }

        public override int GetHashCode()
        {
            var hash = new MultiplyByPrimesHash();
            hash.Add(this.SourceFilePath);
            hash.Add(this.MetaData);
            hash.AddRange(this.Types);
            return hash.GetHashCode();
        }

        public override string ToString()
        {
            return String.Join(Environment.NewLine, this.Types);
        }
    }
}
