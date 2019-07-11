// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    // Factory class for creating one of the built-in converters.
    internal class BuiltInConverterFactory : ConverterFactory
    {
        // Internal rather than private for testing purposes.
        internal static readonly IDictionary<string, Lazy<ToolFileConverterBase>> BuiltInConverters = CreateBuiltInConverters();

        public override ToolFileConverterBase CreateConverterCore(string toolFormat)
        {
            Lazy<ToolFileConverterBase> converter;
            return BuiltInConverters.TryGetValue(toolFormat, out converter)
                ? converter.Value
                : null;
        }

        private static Dictionary<string, Lazy<ToolFileConverterBase>> CreateBuiltInConverters()
        {
            var result = new Dictionary<string, Lazy<ToolFileConverterBase>>();
            CreateConverterRecord<AndroidStudioConverter>(result, ToolFormat.AndroidStudio);
            CreateConverterRecord<CppCheckConverter>(result, ToolFormat.CppCheck);
            CreateConverterRecord<ClangAnalyzerConverter>(result, ToolFormat.ClangAnalyzer);
            CreateConverterRecord<ContrastSecurityConverter>(result, ToolFormat.ContrastSecurity);
            CreateConverterRecord<FortifyConverter>(result, ToolFormat.Fortify);
            CreateConverterRecord<FortifyFprConverter>(result, ToolFormat.FortifyFpr);
            CreateConverterRecord<FxCopConverter>(result, ToolFormat.FxCop);
            CreateConverterRecord<PREfastConverter>(result, ToolFormat.PREfast);
            CreateConverterRecord<PylintConverter>(result, ToolFormat.Pylint);
            CreateConverterRecord<SemmleQLConverter>(result, ToolFormat.SemmleQL);
            CreateConverterRecord<StaticDriverVerifierConverter>(result, ToolFormat.StaticDriverVerifier);
            CreateConverterRecord<TSLintConverter>(result, ToolFormat.TSLint);
            CreateConverterRecord<MSBuildConverter>(result, ToolFormat.MSBuild);
            return result;
        }

        private static void CreateConverterRecord<T>(IDictionary<string, Lazy<ToolFileConverterBase>> dict, string format)
            where T : ToolFileConverterBase, new()
        {
            dict.Add(format, new Lazy<ToolFileConverterBase>(() => new T(), LazyThreadSafetyMode.ExecutionAndPublication));
        }
    }
}
