﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis.Sarif.Writers;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// A class that provides helpers for converting a log file produced by 
    /// one of a well-known set of tools to the SARIF format.
    /// </summary>
    public class ToolFormatConverter
    {
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
        public void ConvertToStandardFormat(
            string toolFormat,
            string inputFileName,
            string outputFileName,
            ToolFormatConversionOptions conversionOptions)
        {
            if (inputFileName == null) { throw new ArgumentNullException(nameof(inputFileName)); };
            if (outputFileName == null) { throw new ArgumentNullException(nameof(outputFileName)); };

            if (Directory.Exists(outputFileName))
            {
                throw new ArgumentException("Specified file output path exists but is a directory.", nameof(outputFileName));
            }

            if (!conversionOptions.HasFlag(ToolFormatConversionOptions.OverwriteExistingOutputFile) && File.Exists(outputFileName))
            {
                throw new InvalidOperationException("Output file already exists and option to overwrite was not specified.");
            }

            if (toolFormat.MatchesToolFormat(ToolFormat.PREfast))
            {
                string sarif = ConvertPREfastToStandardFormat(inputFileName);
                File.WriteAllText(outputFileName, sarif);
            }
            else
            {
                // FileMode settings here will results in an exception being raised if the input 
                // file does not exist, and that an existing output file will be overwritten
                using (var input = File.OpenRead(inputFileName))
                using (var outputTextStream = File.Create(outputFileName))
                using (var outputTextWriter = new StreamWriter(outputTextStream))
                using (var outputJson = new JsonTextWriter(outputTextWriter))
                {
                    if (conversionOptions.HasFlag(ToolFormatConversionOptions.PrettyPrint))
                    {
                        outputJson.Formatting = Formatting.Indented;
                    }

                    using (var output = new ResultLogJsonWriter(outputJson))
                    {
                        ConvertToStandardFormat(toolFormat, input, output);
                    }
                }
            }
        }

        /// <summary>Converts a tool log file to the SARIF format.</summary>
        /// <param name="toolFormat">The tool format of the input file.</param>
        /// <param name="inputFileName">The input log file name.</param>
        /// <param name="outputFileName">The name of the file to which the resulting SARIF log shall be
        /// written. This cannot be a directory.</param>
        public void ConvertToStandardFormat(
            string toolFormat,
            string inputFileName,
            string outputFileName)
        {
            if (toolFormat == ToolFormat.PREfast)
            {
                string sarif = ConvertPREfastToStandardFormat(inputFileName);
                File.WriteAllText(outputFileName, sarif);
                return;
            }

            ConvertToStandardFormat(toolFormat, inputFileName, outputFileName, ToolFormatConversionOptions.None);
        }

        /// <summary>Converts a tool log file represented as a stream into the SARIF format.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
        /// illegal values.</exception>
        /// <param name="toolFormat">The tool format of the input file.</param>
        /// <param name="inputStream">A stream that contains tool log contents.</param>
        /// <param name="outputStream">A stream to which the converted output should be written.</param>
        public void ConvertToStandardFormat(
            string toolFormat,
            Stream inputStream,
            IResultLogWriter outputStream)
        {
            if (toolFormat.MatchesToolFormat(ToolFormat.PREfast))
            {
                throw new ArgumentException("Cannot convert PREfast XML from stream. Call ConvertPREfastToStandardFormat helper instead.");
            };

            if (inputStream == null) { throw new ArgumentNullException(nameof(inputStream)); };
            if (outputStream == null) { throw new ArgumentNullException(nameof(outputStream)); };

            Lazy<ToolFileConverterBase> converter;
            if (_converters.TryGetValue(toolFormat, out converter))
            {
                converter.Value.Convert(inputStream, outputStream);
            }
            else
            {
                throw new ArgumentException("Unrecognized tool specified: " + toolFormat.ToString(), nameof(toolFormat));
            }
        }

        private readonly IDictionary<string, Lazy<ToolFileConverterBase>> _converters = CreateConverterRecords();

        private static Dictionary<string, Lazy<ToolFileConverterBase>> CreateConverterRecords()
        {
            var result = new Dictionary<string, Lazy<ToolFileConverterBase>>();
            CreateConverterRecord<AndroidStudioConverter>(result, ToolFormat.AndroidStudio);
            CreateConverterRecord<CppCheckConverter>(result, ToolFormat.CppCheck);
            CreateConverterRecord<ClangAnalyzerConverter>(result, ToolFormat.ClangAnalyzer);
            CreateConverterRecord<FortifyConverter>(result, ToolFormat.Fortify);
            CreateConverterRecord<FortifyFprConverter>(result, ToolFormat.FortifyFpr);
            CreateConverterRecord<FxCopConverter>(result, ToolFormat.FxCop);
            CreateConverterRecord<SemmleConverter>(result, ToolFormat.SemmleQL);
            CreateConverterRecord<StaticDriverVerifierConverter>(result, ToolFormat.StaticDriverVerifier);
            return result;
        }

        private static void CreateConverterRecord<T>(IDictionary<string, Lazy<ToolFileConverterBase>> dict, string format)
            where T : ToolFileConverterBase, new()
        {
            dict.Add(format, new Lazy<ToolFileConverterBase>(() => new T(), LazyThreadSafetyMode.ExecutionAndPublication));
        }

        /// <summary>Converts a legacy PREfast XML log file into the SARIF format.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
        /// illegal values.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the requested operation is invalid.</exception>
        /// <param name="toolFormat">The tool format of the input file.</param>
        /// <param name="inputFileName">The input log file name.</param>
        /// <returns>The converted PREfast log file in SARIF format.</returns>
        public static string ConvertPREfastToStandardFormat(string inputFileName)
        {
            if (inputFileName == null) { throw new ArgumentNullException(nameof(inputFileName)); };

            return SafeNativeMethods.ConvertToSarif(inputFileName);
        }        
    }

    internal class SafeNativeMethods
    {
        [return: MarshalAs(UnmanagedType.BStr)]
        [DllImport("PREfastXmlSarifConverter", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal static extern string ConvertToSarif([MarshalAs(UnmanagedType.BStr)][In]string prefastFilePath);
    }
}
