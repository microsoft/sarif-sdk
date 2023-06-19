// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Visitors;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class ConvertCommand : CommandBase
    {
        public int Run(ConvertOptions convertOptions, IFileSystem fileSystem = null)
        {
            fileSystem ??= Sarif.FileSystem.Instance;

            try
            {
                if (string.IsNullOrEmpty(convertOptions.OutputFilePath))
                {
                    convertOptions.OutputFilePath = convertOptions.InputFilePath + SarifConstants.SarifFileExtension;
                }

                if (fileSystem.DirectoryExists(convertOptions.OutputFilePath))
                {
                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "The output path '{0}' is a directory.",
                            convertOptions.OutputFilePath));
                    return FAILURE;
                }

                if (!ValidateOptions(convertOptions, fileSystem)) { return FAILURE; }

                FilePersistenceOptions logFilePersistenceOptions = FilePersistenceOptions.None;

                OptionallyEmittedData dataToInsert = convertOptions.DataToInsert.ToFlags();

                new ToolFormatConverter().ConvertToStandardFormat(
                                                convertOptions.ToolFormat,
                                                convertOptions.InputFilePath,
                                                convertOptions.OutputFilePath,
                                                logFilePersistenceOptions,
                                                dataToInsert,
                                                convertOptions.PluginAssemblyPath);

#pragma warning disable CS0618 // Type or member is obsolete
                if (convertOptions.NormalizeForGhas || convertOptions.NormalizeForGitHub)
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    SarifLog sarifLog;

                    JsonSerializer serializer = new JsonSerializer()
                    {
                        Formatting = convertOptions.PrettyPrint ? Formatting.Indented : 0,
                    };

                    using (JsonTextReader reader = new JsonTextReader(new StreamReader(convertOptions.OutputFilePath)))
                    {
                        sarifLog = serializer.Deserialize<SarifLog>(reader);
                    }

                    var visitor = new GitHubIngestionVisitor();
                    visitor.VisitSarifLog(sarifLog);

                    using (FileStream stream = File.Create(convertOptions.OutputFilePath))
                    using (StreamWriter streamWriter = new StreamWriter(stream))
                    using (JsonTextWriter writer = new JsonTextWriter(streamWriter))
                    {
                        serializer.Serialize(writer, sarifLog);
                    }
                }
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                Console.WriteLine(ex);
                return FAILURE;
            }

            return SUCCESS;
        }

        private static bool ValidateOptions(ConvertOptions convertOptions, IFileSystem fileSystem)
        {
            bool valid = true;

            valid &= convertOptions.Validate();

            valid &= DriverUtilities.ReportWhetherOutputFileCanBeCreated(convertOptions.OutputFilePath, convertOptions.ForceOverwrite, fileSystem);

            return valid;
        }
    }
}
