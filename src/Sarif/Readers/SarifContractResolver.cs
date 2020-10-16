// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class SarifContractResolver : DefaultContractResolver
    {

        [Obsolete("The default SARIF serialization has been updated so that specifying a contract resolver is no longer required.", error: false)]
        public static readonly SarifContractResolver Instance = new SarifContractResolver();

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            // this will only be called once and then cached
            if (objectType == typeof(Uri))
                contract.Converter = UriConverter.Instance;

            else if (objectType == typeof(DateTime))
                contract.Converter = DateTimeConverter.Instance;

            else if (objectType == typeof(Version))
                contract.Converter = VersionConverter.Instance;

            else if (objectType == typeof(SarifVersion))
                contract.Converter = SarifVersionConverter.Instance;

            else if (objectType == typeof(ThreadFlowLocationImportance))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(FailureLevel))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(ResultKind))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(BaselineState))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(SuppressionKind))
                contract.Converter = FlagsEnumConverter.Instance;

            else if (objectType == typeof(ArtifactRoles))
                contract.Converter = FlagsEnumConverter.Instance;

            else if (objectType == typeof(IDictionary<string, SerializedPropertyInfo>))
                contract.Converter = PropertyBagConverter.Instance;

            else if (objectType == typeof(Dictionary<string, SerializedPropertyInfo>))
                contract.Converter = PropertyBagConverter.Instance;

            return contract;
        }
    }
}
