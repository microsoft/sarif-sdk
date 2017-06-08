// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class ConverterFactory
    {
        internal ToolFileConverterBase CreateConverter(string toolFormat, string pluginAssemblyPath)
        {
            return string.IsNullOrWhiteSpace(pluginAssemblyPath)
                ? new BuiltInConverterFactory().CreateConverter(toolFormat)
                : CreateConverterFromPlugin(toolFormat, pluginAssemblyPath);
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
    }
}
