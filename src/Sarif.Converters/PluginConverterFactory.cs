// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    // Factory class for creating a converter from a specified plug-in assembly.
    internal class PluginConverterFactory : IConverterFactory
    {
        private readonly string pluginAssemblyPath;

        internal PluginConverterFactory(string pluginAssemblyPath)
        {
            this.pluginAssemblyPath = pluginAssemblyPath;
        }

        public ToolFileConverterBase CreateConverter(string toolFormat)
        {
            if (!File.Exists(this.pluginAssemblyPath))
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorMissingPluginAssembly,
                    this.pluginAssemblyPath);

                throw new ArgumentException(message, nameof(this.pluginAssemblyPath));
            }

            // Convention: The converter type name is derived from the tool name. It can
            // reside in any namespace.
            string converterTypeName = toolFormat + "Converter";

            Assembly pluginAssembly = Assembly.LoadFile(this.pluginAssemblyPath);
            Type[] pluginTypes = pluginAssembly
                .GetTypes()
                .Where(t => t.IsPublic && t.Name.Equals(converterTypeName, StringComparison.Ordinal))
                .ToArray();

            if (pluginTypes.Length == 0)
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorMissingConverterType,
                    this.pluginAssemblyPath,
                    converterTypeName,
                    toolFormat);

                throw new ArgumentException(message, nameof(this.pluginAssemblyPath));
            }

            // This can happen if types with the same name exist in more than one namespace.
            if (pluginTypes.Length > 1)
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorAmbiguousConverterType,
                    this.pluginAssemblyPath,
                    converterTypeName,
                    toolFormat);

                throw new ArgumentException(message, nameof(this.pluginAssemblyPath));
            }

            Type converterType = pluginTypes[0];
            if (!converterType.HasDefaultConstructor())
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorConverterTypeHasNoDefaultConstructor,
                    converterType.FullName,
                    this.pluginAssemblyPath,
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
                    this.pluginAssemblyPath,
                    toolFormat,
                    typeof(ToolFileConverterBase).FullName);

                throw new ArgumentException(message, nameof(this.pluginAssemblyPath));
            }

            return converter;
        }
    }
}
