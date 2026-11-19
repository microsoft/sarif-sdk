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
        private readonly IEnvironmentVariableGetter _environment;

        public ConvertCommand() : this(new EnvironmentVariableGetter())
        {
        }

        public ConvertCommand(IEnvironmentVariableGetter environment)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

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

                AdoPipelineContext adoContext = null;
                if (convertOptions.NormalizeForGHAzDO)
                {
                    AdoPipelineContext.DetectionState adoState =
                        AdoPipelineContext.TryDetect(_environment, out adoContext, out string adoError);
                    if (adoState != AdoPipelineContext.DetectionState.Complete)
                    {
                        Console.Error.WriteLine(
                            adoState == AdoPipelineContext.DetectionState.Partial
                                ? adoError
                                : "--normalize-for-ghazdo requires a complete Azure DevOps pipeline environment.");
                        return FAILURE;
                    }
                }

                FilePersistenceOptions logFilePersistenceOptions = FilePersistenceOptions.None;

                OptionallyEmittedData dataToInsert = convertOptions.DataToInsert.ToFlags();

                new ToolFormatConverter().ConvertToStandardFormat(
                                                convertOptions.ToolFormat,
                                                convertOptions.InputFilePath,
                                                convertOptions.OutputFilePath,
                                                logFilePersistenceOptions,
                                                dataToInsert,
                                                convertOptions.PluginAssemblyPath);

                if (adoContext != null && !TryStampAdoContext(convertOptions, adoContext))
                {
                    return FAILURE;
                }

#pragma warning disable CS0618 // Type or member is obsolete
                if (convertOptions.NormalizeForGhas || convertOptions.NormalizeForGitHub)
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    SarifLog sarifLog;

                    var serializer = new JsonSerializer()
                    {
                        Formatting = convertOptions.PrettyPrint ? Formatting.Indented : 0,
                    };

                    using (var reader = new JsonTextReader(new StreamReader(convertOptions.OutputFilePath)))
                    {
                        sarifLog = serializer.Deserialize<SarifLog>(reader);
                    }

                    var visitor = new GitHubIngestionVisitor();
                    visitor.VisitSarifLog(sarifLog);

                    using (FileStream stream = File.Create(convertOptions.OutputFilePath))
                    using (var streamWriter = new StreamWriter(stream))
                    using (var writer = new JsonTextWriter(streamWriter))
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

        private static bool TryStampAdoContext(ConvertOptions options, AdoPipelineContext context)
        {
            SarifLog log = SarifLog.Load(options.OutputFilePath);
            foreach (Run run in log.Runs)
            {
                if (!context.TryApplyTo(run, out string conflictError))
                {
                    Console.Error.WriteLine(conflictError);
                    return false;
                }
            }

            var serializer = new JsonSerializer
            {
                Formatting = options.PrettyPrint ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None,
            };
            using (FileStream stream = File.Create(options.OutputFilePath))
            using (var streamWriter = new StreamWriter(stream))
            using (var writer = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(writer, log);
            }
            return true;
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
