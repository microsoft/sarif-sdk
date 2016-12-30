// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    internal class PropertiesDictionaryContractResolver : DefaultContractResolver
    {
        private static PropertiesDictionaryConverter s_converter = new PropertiesDictionaryConverter();

        public override JsonContract ResolveContract(Type type)
        {
            JsonContract contract = base.CreateContract(type);

            // this will only be called once and then cached
            if (type == typeof(IntegerSet))
                contract.Converter = s_converter;

            if (type == typeof(StringSet))
                contract.Converter = s_converter;

            return contract;
        }
    }
}