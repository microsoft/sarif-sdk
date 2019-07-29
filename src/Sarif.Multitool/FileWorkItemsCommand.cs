// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Driver;
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

            FilteringStrategy filteringStrategy = FilteringStrategyFactory.CreateFilteringStrategy(options.FilteringStrategy);
            GroupingStrategy groupingStrategy = GroupingStrategyFactory.CreateGroupingStrategy(options.GroupingStrategy);

            for (int i = 0; i < sarifLog.Runs.Count; ++i)
            {
                if (sarifLog.Runs[i]?.Results?.Count > 0)
                {
                    // TODO: THESE TWO LINES ARE REPLACED BY A CALL TO SarifSplitter.Split with the specified strategies (or predicates).
                    // It would return a set of SarifLog objects, rather than a set of WorkItemFilingMetadata objects as I currently have it.
                    IList<Result> filteredResults = filteringStrategy.FilterResults(sarifLog.Runs[i].Results);
                    IList<WorkItemFilingMetadata> workItemMetadata = groupingStrategy.GroupResults(filteredResults);

                    // TODO: AND THIS IS WHERE THE CALL TO SarifLog.CreateWorkItemFilingMetadata would transform the logs into metadata,
                    // but here I'm just populating the metadata by hand.
                    AddDummyMetadata(workItemMetadata, projectName);

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

        private void AddDummyMetadata(IList<WorkItemFilingMetadata> workItemFilingMetadata, string projectName)
        {
            int bugNumber = 1;
            foreach (WorkItemFilingMetadata metadata in workItemFilingMetadata)
            {
                metadata.Title = $"Bug #{bugNumber} was added by a partially refactored work item filer.";
                metadata.Description = "This bug is very important. Let's fix it!";
                metadata.AreaPath = $@"{projectName}\TopLevel\SecondLevel\Leaf";
                metadata.Tags = new List<string> { "security", "compliance" };
            }
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
