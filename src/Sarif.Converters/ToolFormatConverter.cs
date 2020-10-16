// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// A class that provides helpers for converting a log file produced by 
    /// one of a well-known set of tools to the SARIF format.
    /// </summary>
    public class ToolFormatConverter
    {
        private readonly AssemblyLoadFileDelegate assemblyLoadFileDelegate;

        public ToolFormatConverter(AssemblyLoadFileDelegate assemblyLoadFileDelegate = null)
        {
            this.assemblyLoadFileDelegate = assemblyLoadFileDelegate;
        }

        /// <summary>Converts a tool log file into the SARIF format.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
        /// illegal values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        /// <param name="toolFormat">The tool format of the input file.</param>
        /// <param name="inputFileName">The input log file name.</param>
        /// <param name="outputFileName">The name of the file to which the resulting SARIF log shall be
        /// written. This cannot be a directory.</param>
        /// <param name="conversionOptions">Options for controlling the conversion.</param>
        /// <param name="pluginAssemblyPath">Path to plugin assembly containing converter types.</param>
        public void ConvertToStandardFormat(
            string toolFormat,
            string inputFileName,
            string outputFileName,
            LoggingOptions loggingOptions = LoggingOptions.None,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            string pluginAssemblyPath = null)
        {
            if (inputFileName == null) { throw new ArgumentNullException(nameof(inputFileName)); }
            if (outputFileName == null) { throw new ArgumentNullException(nameof(outputFileName)); }

            // FileMode settings here will results in an exception being raised if the input 
            // file does not exist, and that an existing output file will be overwritten
            using (FileStream input = File.OpenRead(inputFileName))
            using (FileStream outputTextStream = File.Create(outputFileName))
            using (var outputTextWriter = new StreamWriter(outputTextStream))
            using (var outputJson = new JsonTextWriter(outputTextWriter))
            {
                if (loggingOptions.HasFlag(LoggingOptions.PrettyPrint))
                {
                    outputJson.Formatting = Formatting.Indented;
                }

                using (var output = new ResultLogJsonWriter(outputJson))
                {
                    ConvertToStandardFormat(toolFormat, input, output, dataToInsert, pluginAssemblyPath);
                }
            }
        }


        /// <summary>Converts a tool log file represented as a stream into the SARIF format.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
        /// illegal values.</exception>
        /// <param name="toolFormat">The tool format of the input file.</param>
        /// <param name="inputStream">A stream that contains tool log contents.</param>
        /// <param name="outputStream">A stream to which the converted output should be written.</param>
        /// <param name="pluginAssemblyPath">Path to plugin assembly containing converter types.</param>
        public void ConvertToStandardFormat(
            string toolFormat,
            Stream inputStream,
            IResultLogWriter outputStream,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            string pluginAssemblyPath = null)
        {
            if (inputStream == null) { throw new ArgumentNullException(nameof(inputStream)); }
            if (outputStream == null) { throw new ArgumentNullException(nameof(outputStream)); }

            ConverterFactory factory = CreateConverterFactory(pluginAssemblyPath);

            ToolFileConverterBase converter = factory.CreateConverter(toolFormat);
            if (converter != null)
            {
                converter.Convert(inputStream, outputStream, dataToInsert);
            }
            else
            {
                throw new ArgumentException("Unrecognized tool specified: " + toolFormat, nameof(toolFormat));
            }
        }

        // Set up a Chain of Responsibility that will get the converter from the first
        // factory capable of creating it.
        // This method is internal, rather than private, for test purposes.
        internal ConverterFactory CreateConverterFactory(string pluginAssemblyPath)
        {
            ConverterFactory factory = new BuiltInConverterFactory();
            if (!string.IsNullOrWhiteSpace(pluginAssemblyPath))
            {
                factory = new PluginConverterFactory(pluginAssemblyPath, this.assemblyLoadFileDelegate)
                {
                    Next = factory,
                };
            }

            return factory;
        }
    }
}
