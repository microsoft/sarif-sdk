// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class ConverterFactory
    {
        private static readonly IDictionary<string, Lazy<ToolFileConverterBase>> BuiltInConverters = CreateBuiltInConverters();

        internal ToolFileConverterBase CreateConverter(string toolFormat, string pluginAssemblyPath)
        {
            return string.IsNullOrWhiteSpace(pluginAssemblyPath)
                ? CreateBuiltInConverter(toolFormat)
                : CreateConverterFromPlugin(toolFormat, pluginAssemblyPath);
        }

        private static ToolFileConverterBase CreateBuiltInConverter(string toolFormat)
        {
            Lazy<ToolFileConverterBase> converter;
            return BuiltInConverters.TryGetValue(toolFormat, out converter)
                ? converter.Value
                : null;
        }

        private static ToolFileConverterBase CreateConverterFromPlugin(string toolFormat, string pluginAssemblyPath)
        {
            if (!File.Exists(pluginAssemblyPath))
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorMissingPluginAssembly,
                    pluginAssemblyPath);

                throw new ArgumentException(message, nameof(pluginAssemblyPath));
            }

            // Convention: The converter type name is derived from the tool name. It can
            // reside in any namespace.
            string converterTypeName = toolFormat + "Converter";

            Assembly pluginAssembly = Assembly.LoadFile(pluginAssemblyPath);
            Type[] pluginTypes = pluginAssembly
                .GetTypes()
                .Where(t => t.IsPublic && t.Name.Equals(converterTypeName, StringComparison.Ordinal))
                .ToArray();

            if (pluginTypes.Length == 0)
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorMissingConverterType,
                    pluginAssemblyPath,
                    converterTypeName,
                    toolFormat);

                throw new ArgumentException(message, nameof(pluginAssemblyPath));
            }

            // This can happen if types with the same name exist in more than one namespace.
            if (pluginTypes.Length > 1)
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorAmbiguousConverterType,
                    pluginAssemblyPath,
                    converterTypeName,
                    toolFormat);

                throw new ArgumentException(message, nameof(pluginAssemblyPath));
            }

            Type converterType = pluginTypes[0];
            if (!converterType.HasDefaultConstructor())
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorConverterTypeHasNoDefaultConstructor,
                    converterType.FullName,
                    pluginAssemblyPath,
                    toolFormat);

                throw new ArgumentException(message, nameof(pluginAssemblyPath));
            }

            var converter = Activator.CreateInstance(converterType) as ToolFileConverterBase;

            if (converter == null)
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorIncorrectConverterTypeDerivation,
                    converterType.FullName,
                    pluginAssemblyPath,
                    toolFormat,
                    typeof(ToolFileConverterBase).FullName);

                throw new ArgumentException(message, nameof(pluginAssemblyPath));
            }

            return converter;
        }

        private static Dictionary<string, Lazy<ToolFileConverterBase>> CreateBuiltInConverters()
        {
            var result = new Dictionary<string, Lazy<ToolFileConverterBase>>();
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
