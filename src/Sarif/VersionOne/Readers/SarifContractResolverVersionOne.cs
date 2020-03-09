// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class SarifContractResolverVersionOne : DefaultContractResolver
    {
        public static readonly SarifContractResolverVersionOne Instance = new SarifContractResolverVersionOne();

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

            else if (objectType == typeof(SarifVersionVersionOne))
                contract.Converter = SarifVersionConverter.Instance;

            else if (objectType == typeof(AnnotatedCodeLocationKindVersionOne))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(AnnotatedCodeLocationImportanceVersionOne))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(ResultLevelVersionOne))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(RuleConfigurationVersionOne))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(NotificationLevelVersionOne))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(AlgorithmKindVersionOne))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(BaselineStateVersionOne))
                contract.Converter = EnumConverter.Instance;

            else if (objectType == typeof(SuppressionStatesVersionOne))
                contract.Converter = FlagsEnumConverter.Instance;

            else if (objectType == typeof(IDictionary<string, SerializedPropertyInfo>))
                contract.Converter = PropertyBagConverter.Instance;

            return contract;
        }
    }
}
