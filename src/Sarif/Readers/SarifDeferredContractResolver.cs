// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    /// <summary>
    ///  SarifDeferredContractResolver is a JSON.NET contract resolver for the SARIF file format
    ///  which uses deferred collection classes for the large collections in the file so that the
    ///  full object graph doesn't have to be kept in memory.
    ///  
    ///  JSON.NET still must parse the whole file, and must parse the deferred collections again
    ///  when they are enumerated, so load times are slower.
    /// </summary>
    public class SarifDeferredContractResolver : SarifContractResolver
    {
        public static new readonly SarifDeferredContractResolver Instance = new SarifDeferredContractResolver();

        private static readonly DeferredListConverter<Result> ResultConverterInstance = new DeferredListConverter<Result>();
        private static readonly DeferredDictionaryConverter<Artifact> FilesConverterInstance = new DeferredDictionaryConverter<Artifact>();

        protected override JsonContract CreateContract(Type type)
        {
            JsonContract contract = base.CreateContract(type);

            if (type == typeof(IList<Result>))
            {
                contract.Converter = ResultConverterInstance;
                return contract;
            }
            else if (type == typeof(IDictionary<string, Artifact>))
            {
                contract.Converter = FilesConverterInstance;
                return contract;
            }

            return contract;
        }
    }
}
