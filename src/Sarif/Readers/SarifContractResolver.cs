// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json.Serialization;
using System;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class SarifContractResolver : DefaultContractResolver
    {
        public static readonly SarifContractResolver Instance = new SarifContractResolver();

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            // this will only be called once and then cached
            if (objectType == typeof(SarifVersion))
                contract.Converter = SarifVersionConverter.Instance;
            // 
            if (objectType == typeof(Version))
                contract.Converter = VersionConverter.Instance;

            return contract;
        }
    }
}
