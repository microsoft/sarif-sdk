// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    // Factory class for creating one of the built-in converters.
    internal class BuiltInConverterFactory : IConverterFactory
    {
        private static readonly IDictionary<string, Lazy<ToolFileConverterBase>> BuiltInConverters = CreateBuiltInConverters();

        public ToolFileConverterBase CreateConverter(string toolFormat)
        {
            Lazy<ToolFileConverterBase> converter;
            return BuiltInConverters.TryGetValue(toolFormat, out converter)
                ? converter.Value
                : null;
        }

        private static Dictionary<string, Lazy<ToolFileConverterBase>> CreateBuiltInConverters()
        {
            var result = new Dictionary<string, Lazy<ToolFileConverterBase>>();
            // TODO: Reflect to get them all.
            CreateConverterRecord<AndroidStudioConverter>(result, ToolFormat.AndroidStudio);
            CreateConverterRecord<CppCheckConverter>(result, ToolFormat.CppCheck);
            CreateConverterRecord<ClangAnalyzerConverter>(result, ToolFormat.ClangAnalyzer);
            CreateConverterRecord<FortifyConverter>(result, ToolFormat.Fortify);
            CreateConverterRecord<FortifyFprConverter>(result, ToolFormat.FortifyFpr);
            CreateConverterRecord<FxCopConverter>(result, ToolFormat.FxCop);
            CreateConverterRecord<SemmleConverter>(result, ToolFormat.SemmleQL);
            CreateConverterRecord<StaticDriverVerifierConverter>(result, ToolFormat.StaticDriverVerifier);
            return result;
        }

        private static void CreateConverterRecord<T>(IDictionary<string, Lazy<ToolFileConverterBase>> dict, string format)
            where T : ToolFileConverterBase, new()
        {
            dict.Add(format, new Lazy<ToolFileConverterBase>(() => new T(), LazyThreadSafetyMode.ExecutionAndPublication));
        }
    }
}
