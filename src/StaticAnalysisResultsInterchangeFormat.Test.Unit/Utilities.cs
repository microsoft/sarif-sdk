// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Sarif
{
    internal static class Utilities
    {
        public static MemoryStream CreateStreamFromString(string data)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(data));
        }

        public static XmlReader CreateXmlReaderFromString(string data)
        {
            var xmlSettings = new XmlReaderSettings
            {
                CloseInput = true,
                DtdProcessing = DtdProcessing.Ignore,
                ConformanceLevel = ConformanceLevel.Fragment
            };

            return CreateXmlReader(data, xmlSettings);
        }

        public static XmlReader CreateWhitespaceSkippingXmlReaderFromString(string data)
        {
            var xmlSettings = new XmlReaderSettings
            {
                CloseInput = true,
                DtdProcessing = DtdProcessing.Ignore,
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreWhitespace = true
            };

            return CreateXmlReader(data, xmlSettings);
        }

        private static XmlReader CreateXmlReader(string data, XmlReaderSettings xmlSettings)
        {
            return XmlReader.Create(new StringReader(data), xmlSettings);
        }

        public static string GetConverterJson(IToolFileConverter converter, byte[] inputData)
        {
            using (var input = new MemoryStream(inputData))
            {
                using (var output = new StringWriter())
                {
                    var json = new JsonTextWriter(output);
                    json.Formatting = Newtonsoft.Json.Formatting.Indented;
                    json.CloseOutput = false;
                    using (var outputWriter = new ResultLogJsonWriter(json))
                    {
                        converter.Convert(input, outputWriter);
                    }

                    return output.ToString();
                }
            }
        }

        public static string GetConverterJson(IToolFileConverter converter, string inputData)
        {
            return GetConverterJson(converter, Encoding.UTF8.GetBytes(inputData));
        }

        public static ResultLogObjectWriter GetConverterObjects(IToolFileConverter converter, byte[] inputData)
        {
            var result = new ResultLogObjectWriter();
            using (var input = new MemoryStream(inputData))
            {
                converter.Convert(input, result);
            }

            return result;
        }

        public static ResultLogObjectWriter GetConverterObjects(IToolFileConverter converter, string inputData)
        {
            return GetConverterObjects(converter, Encoding.UTF8.GetBytes(inputData));
        }

        public static XmlReader CreateNameTableReader(this XNode node)
        {
            // Make an XmlTextReader instead of XmlReader so that the NameTable
            // member is filled out.
            string stringXml = node.ToString(SaveOptions.DisableFormatting);
            return CreateXmlReaderFromString(stringXml);
        }
    }
}
