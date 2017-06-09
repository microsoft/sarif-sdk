// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class BuiltInConverterFactoryTests
    {
        [TestMethod]
        public void BuiltInConverterFactory_HasConverterForEveryBuiltInToolFormatExceptPREfast()
        {
            List<string> toolFormats = Utilities.GetToolFormats()
                .Except(new[] { ToolFormat.PREfast })
                .ToList();

            string factoryName = nameof(BuiltInConverterFactory);

            foreach (string toolFormat in toolFormats)
            {
                Assert.IsTrue(
                    BuiltInConverterFactory.BuiltInConverters.ContainsKey(toolFormat),
                    $"There is no built-in converter for the {toolFormat} tool format, or the converter exists but is not registered in {factoryName}.");
            }
        }

        [TestMethod]
        public void BuiltInConverterFactory_HasMatchingConverterTypeNamesForAllRegisteredToolFormats()
        {
            foreach (string toolFormat in Utilities.GetToolFormats().Except(new[] { ToolFormat.None }))
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

                    Assert.AreEqual(
                        0,
                        string.CompareOrdinal(expectedConverterTypeName, actualConverterTypeName),
                        $"Converter type name {actualConverterTypeName} does not follow convention based on tool format. It should be named {expectedConverterTypeName}.");
                }
            }
        }
    }
}
