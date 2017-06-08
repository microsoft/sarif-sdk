// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    internal class ConverterFactory
    {
        internal ToolFileConverterBase CreateConverter(string toolFormat, string pluginAssemblyPath)
        {
            return string.IsNullOrWhiteSpace(pluginAssemblyPath)
                ? new BuiltInConverterFactory().CreateConverter(toolFormat)
                : new PluginConverterFactory(pluginAssemblyPath).CreateConverter(toolFormat);
        }
    }
}
