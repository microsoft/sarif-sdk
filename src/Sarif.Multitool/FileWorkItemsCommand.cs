﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Microsoft.CodeAnalysis.Sarif.WorkItemFiling;
using Microsoft.Json.Schema;
using Microsoft.Json.Schema.Validation;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class FileWorkItemsCommand : CommandBase
    {
        public int Run(FileWorkItemsOptions options)
            => Run(options, new FileSystem());

        public int Run(FileWorkItemsOptions options, IFileSystem fileSystem)
        {
            if (!ValidateOptions(options)) { return 1; }

            if (fileSystem.FileExists(options.OutputFilePath) && !options.Force)
            {
                throw new ArgumentException($"The output file '{options.OutputFilePath}' already exists. Use --force to overrwrite.");
            }

            // For unit tests: allow us to just validate the options and return.
            if (options.TestOptionValidation) { return 0; }

            if (options.Inline)
            {
                options.OutputFilePath = options.InputFilePath;
                options.Force = true;
            }

            string projectName = options.ProjectUri.GetProjectName();

            string logFileContents = fileSystem.ReadAllText(options.InputFilePath);
            EnsureValidSarifLogFile(logFileContents, options.InputFilePath);

            FilingTarget filingTarget = FilingTargetFactory.CreateFilingTarget(options.ProjectUriString);
            var filer = new WorkItemFiler(filingTarget);

            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(logFileContents);

            for (int runIndex = 0; runIndex < sarifLog.Runs.Count; ++runIndex)
            {
                if (sarifLog.Runs[runIndex]?.Results?.Count > 0)
                {
                    var visitor = new PerRunPerRuleSplittingVisitor();
                    visitor.VisitRun(sarifLog.Runs[runIndex]);

                    IList<WorkItemFilingMetadata> workItemMetadata = new List<WorkItemFilingMetadata>(visitor.SplitSarifLogs.Count);

                    for (int splitFileIndex = 0; splitFileIndex < visitor.SplitSarifLogs.Count; splitFileIndex++)
                    {
                        SarifLog splitLog = visitor.SplitSarifLogs[splitFileIndex];
                        workItemMetadata.Add(splitLog.CreateWorkItemFilingMetadata(projectName));
                    }

                    try
                    {
                        filer.FileWorkItems(options.ProjectUri, workItemMetadata, options.PersonalAccessToken).Wait();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine(ex);
                    }
                }
            }

            return 0;
        }
        private bool ValidateOptions(FileWorkItemsOptions options)
        {
            bool valid = true;

            // The CommandLine package lets you declare an option of any type with a constructor
            // that accepts a string. We could have declared the property corresponding to the
            // "project-uri" option to be of type Uri, in which case CommandLine would have
            // populated it by invoking the Uri(string) ctor. This overload requires its argument
            // to be an absolute URI. This happens to be what we want; the problem is that if the
            // user supplies a relative URI, CommandLine produces this error:
            //
            //   Option 'p, project-uri' is defined with a bad format.
            //   Required option 'p, project-uri' is missing.
            //
            // That's not as helpful as a message like this:
            //
            //   The value of the 'p, project-uri' option must be an absolute URI.
            //
            // Therefore we declare the property corresponding to "project-uri" as a string.
            // We convert it to a URI with the ctor Uri(string, UriKind.RelativeOrAbsolute).
            // If it succeeds, we can assign the result to a Uri-valued property; if it fails,
            // we can produce a helpful error message.
            options.ProjectUri = new Uri(options.ProjectUriString, UriKind.RelativeOrAbsolute);

            if (!options.ProjectUri.IsAbsoluteUri)
            {
                string optionDescription = CommandUtilities.GetOptionDescription<FileWorkItemsOptions>(nameof(options.ProjectUriString));
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MultitoolResources.WorkItemFiling_ErrorUriIsNotAbsolute,
                        options.ProjectUriString,
                        optionDescription));
                valid = false;
            }

            valid = ValidateOutputFileOptions(options) && valid;

            return valid;
        }

        private bool ValidateOutputFileOptions(SingleFileOptionsBase options)
        {
            bool valid = true;

            if ((options.OutputFilePath != null && options.Inline) || (options.OutputFilePath == null && !options.Inline))
            {
                string inlineOptionsDescription = CommandUtilities.GetOptionDescription<SingleFileOptionsBase>(nameof(options.Inline));
                string outputFilePathOptionDescription = CommandUtilities.GetOptionDescription<SingleFileOptionsBase>(nameof(options.OutputFilePath));

                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MultitoolResources.ErrorOutputFilePathAndInline,
                        outputFilePathOptionDescription,
                        inlineOptionsDescription));

                valid = false;
            }

            return valid;
        }

        private void EnsureValidSarifLogFile(string logFileContents, string logFilePath)
        {
            Validator validator = CreateLogFileValidator();

            // The second argument is a file path that the Validator uses when reporting
            // errors. The Validator does not attempt to access this file.
            Result[] errors = validator.Validate(logFileContents, logFilePath);

            if (errors?.Length > 0)
            {
                throw new ArgumentException(FormatSchemaErrors(logFilePath, errors), nameof(logFilePath));
            }
        }

        private static Validator CreateLogFileValidator()
        {
            JsonSchema schema = GetSarifSchema();
            return new Validator(schema);
        }

        private static JsonSchema GetSarifSchema()
        {
            const string SarifSchemaResource = "Microsoft.CodeAnalysis.Sarif.Multitool.sarif-2.1.0.json";
            Assembly thisAssembly = typeof(FileWorkItemsCommand).Assembly;

            string sarifSchemaText;
            using (Stream sarifSchemaStream = thisAssembly.GetManifestResourceStream(SarifSchemaResource))
            using (var reader = new StreamReader(sarifSchemaStream))
            {
                sarifSchemaText = reader.ReadToEnd();
            }

            // The second argument is a file path that the SchemaReader uses when reporting
            // errors. The SchemaReader does not attempt to access this file. Since we obtained
            // the schema from an embedded resource rather than from a file, we use the name
            // of the resource.
            return SchemaReader.ReadSchema(sarifSchemaText, SarifSchemaResource);
        }

        private string FormatSchemaErrors(string path, IEnumerable<Result> errors)
        {
            string firstMessageLine = string.Format(CultureInfo.CurrentCulture, MultitoolResources.ErrorInvalidSarifLogFile, path);

            var sb = new StringBuilder(firstMessageLine);
            sb.AppendLine();

            foreach (var error in errors)
            {
                sb.AppendLine(error.FormatForVisualStudio(RuleFactory.GetRuleFromRuleId(error.RuleId)));
            }

            return sb.ToString();
        }
    }
}
