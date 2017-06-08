// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    // Factory class for creating a converter from one of a specified
    // sequence of converters. The converter from the first factory
    // capable of creating the desired converter is returned.
    public class ChainedConverterFactory : ConverterFactory
    {
        private readonly List<ConverterFactory> factories;

        public ChainedConverterFactory(string pluginAssemblyPath)
        {
            this.factories = new List<ConverterFactory>();
            if (!string.IsNullOrWhiteSpace(pluginAssemblyPath))
            {
                this.factories.Add(new PluginConverterFactory(pluginAssemblyPath));
            }

            this.factories.Add(new BuiltInConverterFactory());
        }

        public override ToolFileConverterBase CreateConverter(string toolFormat)
        {
            ToolFileConverterBase converter = null;

            foreach (ConverterFactory factory in this.factories)
            {
                converter = factory.CreateConverter(toolFormat);
                if (converter != null)
                {
                    break;
                }
            }

            return converter;
        }
    }
}
