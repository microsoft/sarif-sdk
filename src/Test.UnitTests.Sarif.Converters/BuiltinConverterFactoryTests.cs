// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class BuiltInConverterFactoryTests
    {
        [Fact]
        public void BuiltInConverterFactory_HasConverterForEveryBuiltInToolFormat()
        {
            List<string> toolFormats = Utilities.GetToolFormats()
                .ToList();

            string factoryName = nameof(BuiltInConverterFactory);

            foreach (string toolFormat in toolFormats)
            {
                Assert.True(
                    BuiltInConverterFactory.BuiltInConverters.ContainsKey(toolFormat),
                    $"There is no built-in converter for the {toolFormat} tool format, or the converter exists but is not registered in {factoryName}.");
            }
        }

        [Fact]
        public void BuiltInConverterFactory_HasMatchingConverterTypeNamesForAllRegisteredToolFormats()
        {
            foreach (string toolFormat in Utilities.GetToolFormats())
            {
                Lazy<ToolFileConverterBase> lazyConverter;

                // There's another test that ensures that all the required keys do in fact exist.
                if (BuiltInConverterFactory.BuiltInConverters.TryGetValue(toolFormat, out lazyConverter))
                {
                    // The only way to tell what subtype of ToolFileConverterBase is stored in each dictionary
                    // entry is to instantiate it. Accessing the "Value" property of a Lazy<T> instantiates the object.
                    ToolFileConverterBase converter = lazyConverter.Value;

                    string expectedConverterTypeName = toolFormat.ConverterTypeName();
                    string actualConverterTypeName = converter.GetType().Name;

                    Assert.Equal(expectedConverterTypeName, actualConverterTypeName);
                }
            }
        }
    }
}
