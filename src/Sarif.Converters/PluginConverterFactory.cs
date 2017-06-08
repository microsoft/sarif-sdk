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
    internal class PluginConverterFactory : ConverterFactory
    {
        // This field is internal, rather than private, for test purposes.
        internal readonly string pluginAssemblyPath;

        internal PluginConverterFactory(string pluginAssemblyPath)
        {
            this.pluginAssemblyPath = pluginAssemblyPath;
        }

        public override ToolFileConverterBase CreateConverterCore(string toolFormat)
        {
            if (!File.Exists(this.pluginAssemblyPath))
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorMissingPluginAssembly,
                    this.pluginAssemblyPath);

                throw new ArgumentException(message, nameof(this.pluginAssemblyPath));
            }

            Assembly pluginAssembly = Assembly.LoadFile(this.pluginAssemblyPath);
            Type[] pluginTypes = pluginAssembly
                .GetTypes()
                .Where(t => IsConverterClassForToolFormat(t, toolFormat))
                .ToArray();

            // This can happen if types with the same name exist in more than one namespace.
            if (pluginTypes.Length > 1)
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    ConverterResources.ErrorAmbiguousConverterType,
                    this.pluginAssemblyPath,
                    ConverterTypeNameForToolFormat(toolFormat),
                    toolFormat);

                throw new ArgumentException(message, nameof(this.pluginAssemblyPath));
            }

            return pluginTypes.Length == 0
                ? null
                : (ToolFileConverterBase)Activator.CreateInstance(pluginTypes[0]);
        }

        private static string ConverterTypeNameForToolFormat(string toolFormat)
        {
            // Convention: The converter type name is derived from the tool name. It can
            // reside in any namespace.
            return toolFormat + "Converter";
        }

        private static bool IsConverterClassForToolFormat(Type type, string toolFormat)
        {
            return type.IsPublic
                && type.Name.Equals(ConverterTypeNameForToolFormat(toolFormat), StringComparison.Ordinal)
                && type.IsSubclassOf(typeof(ToolFileConverterBase))
                && type.HasDefaultConstructor()
                && !type.IsAbstract;
        }
    }
}
