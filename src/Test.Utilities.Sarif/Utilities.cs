// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class Utilities
    {
        public static bool RunningInAppVeyor { get { return Environment.CommandLine.IndexOf("/logger:appveyor", StringComparison.OrdinalIgnoreCase) != -1; } }

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

        public static string GetConverterJson(ToolFileConverterBase converter, byte[] inputData)
        {
            using (var input = new MemoryStream(inputData))
            {
                using (var output = new StringWriter())
                {
                    var json = new JsonTextWriter(output)
                    {
                        Formatting = Newtonsoft.Json.Formatting.Indented,
                        CloseOutput = false
                    };

                    using (var outputWriter = new ResultLogJsonWriter(json))
                    {
                        converter.Convert(input, outputWriter, OptionallyEmittedData.None);
                    }

                    return output.ToString();
                }
            }
        }

        public static string GetConverterJson(ToolFileConverterBase converter, string inputData)
        {
            return GetConverterJson(converter, Encoding.UTF8.GetBytes(inputData));
        }

        public static ResultLogObjectWriter GetConverterObjects(ToolFileConverterBase converter, byte[] inputData)
        {
            var result = new ResultLogObjectWriter();
            using (var input = new MemoryStream(inputData))
            {
                converter.Convert(input, result, OptionallyEmittedData.None);
            }

            return result;
        }

        public static ResultLogObjectWriter GetConverterObjects(ToolFileConverterBase converter, string inputData)
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

        public static List<FieldInfo> GetToolFormatFields()
        {
            return typeof(ToolFormat)
                .GetMembers(BindingFlags.Public | BindingFlags.Static)
                .OfType<FieldInfo>()
                .ToList();
        }

        public static List<string> GetToolFormats()
        {
            return GetToolFormatFields()
                .Select(f => f.GetRawConstantValue() as string)
                .Where(fmt => string.CompareOrdinal(fmt, ToolFormat.None) != 0)
                .ToList();
        }

        public static SarifLog GetSarifLogFromResource(ResourceExtractor extractor, string resourceName)
        {
            string logFileContents = extractor.GetResourceText(resourceName);
            return JsonConvert.DeserializeObject<SarifLog>(logFileContents);
        }

        public static string SafeFormat(string s)
            => s != null ? $"'{s}'" : "<null>";
    }
}
