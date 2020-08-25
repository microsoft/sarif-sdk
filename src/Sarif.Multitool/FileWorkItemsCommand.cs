// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.WorkItems;
using Microsoft.Json.Schema;
using Microsoft.Json.Schema.Validation;
using Microsoft.TeamFoundation.Common;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    /// <summary>
    /// A class that drives SARIF work item filing. This class is responsible for 
    /// collecting and verifying all options relevant to driving the work item filing
    /// process. These options may be retrieved from a serialized version of the 
    /// aggregated configuration (currently rendered as XML, via the PropertiesDictionary
    /// class). Command-line arguments will override any options specified in the 
    /// file-based serialized configuration (if present). After verifying that all
    /// configured options are valid, the command will instantiate an instance of 
    /// SarifWorkItemFiler in order to complete the work.
    /// </summary>
    public class FileWorkItemsCommand : CommandBase
    {
        [ThreadStatic]
        internal static bool s_validateOptionsOnly;

        public int Run(FileWorkItemsOptions options)
            => Run(options, new FileSystem());

        public int Run(FileWorkItemsOptions options, IFileSystem fileSystem)
        {
            using (var filingContext = new SarifWorkItemContext())
            {
                if (!string.IsNullOrEmpty(options.ConfigurationFilePath))
                {
                    filingContext.LoadFromXml(options.ConfigurationFilePath);
                }

                if (!ValidateOptions(options, filingContext, fileSystem))
                {
                    return FAILURE;
                }

                // For unit tests: allow us to just validate the options and return.
                if (s_validateOptionsOnly) { return SUCCESS; }

                string logFileContents = fileSystem.ReadAllText(options.InputFilePath);

                if (!options.DoNotValidate)
                {
                    EnsureValidSarifLogFile(logFileContents, options.InputFilePath);
                }

                if (options.SplittingStrategy != SplittingStrategy.None)
                {
                    filingContext.SplittingStrategy = options.SplittingStrategy;
                }

                if (options.SyncWorkItemMetadata != null)
                {
                    filingContext.SyncWorkItemMetadata = options.SyncWorkItemMetadata.Value;
                }

                if (options.ShouldFileUnchanged != null)
                {
                    filingContext.ShouldFileUnchanged = options.ShouldFileUnchanged.Value;
                }

                if (options.DataToRemove.ToFlags() != OptionallyEmittedData.None)
                {
                    filingContext.DataToRemove = options.DataToRemove.ToFlags();
                }

                if (options.DataToInsert.ToFlags() != OptionallyEmittedData.None)
                {
                    filingContext.DataToInsert = options.DataToInsert.ToFlags();
                }

                SarifLog sarifLog = null;
                using (var filer = new SarifWorkItemFiler(filingContext.WorkItemFilerUri, filingContext))
                {
                    sarifLog = filer.FileWorkItems(logFileContents);
                }

                // By the time we're here, we have updated options.OutputFilePath with the 
                // options.InputFilePath argument (in the presence of --inline) and validated
                // that we can write to this location with one exception: we do not currently
                // handle inlining to a read-only location.
                string outputFilePath = options.OutputFilePath;
                if (!string.IsNullOrEmpty(outputFilePath))
                {
                    Formatting formatting = options.PrettyPrint ? Formatting.Indented : Formatting.None;

                    string sarifLogText = JsonConvert.SerializeObject(sarifLog, formatting);

                    fileSystem.WriteAllText(outputFilePath, sarifLogText);
                }
            }

            return SUCCESS;
        }

        private bool ValidateOptions(FileWorkItemsOptions options, SarifWorkItemContext sarifWorkItemContext, IFileSystem fileSystem)
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
            // If it succeeds, we can assign the result to a Uri-valued property (persisted in the
            // context object); if it fails, we can produce a helpful error message.

            valid &= ValidateHostUri(options.workItemFilingUri, sarifWorkItemContext);

            valid &= options.ValidateOutputOptions();

            if (!string.IsNullOrEmpty(options.OutputFilePath))
            {
                valid &= DriverUtilities.ReportWhetherOutputFileCanBeCreated(options.OutputFilePath, options.Force, fileSystem);
            }

            valid &= EnsurePersonalAccessToken(sarifWorkItemContext);

            return valid;
        }

        // Validates that the URI coming from either the commandline or configuration is valid for filing work items.  A null or empty 
        // value is valid if and only if a plug-in will later provide the value.  This cannot be determined until plugins are loaded later,
        // so at this point an empty value passes validation.
        private static bool ValidateHostUri(string workItemUriString, SarifWorkItemContext workItemFilingConfiguration)
        {

            Uri workItemUri = null;
            if (!string.IsNullOrEmpty(workItemUriString) && !Uri.TryCreate(workItemUriString, UriKind.RelativeOrAbsolute, out workItemUri))
            {
                // A valid URI could not be created from the value '{0}' of the '{1}' option.
                Console.Error.WriteLine(MultitoolResources.WorkItemFiling_ErrorUriIsNotLegal);
                return false;
            }

            // Any command-line argument that's provided overrides values specified in the configuration.
            workItemFilingConfiguration.WorkItemFilerUri = workItemUri ?? workItemFilingConfiguration.WorkItemFilerUri;

            if (!workItemFilingConfiguration.WorkItemFilerUri.IsAbsoluteUri && !workItemFilingConfiguration.WorkItemFilerUri.OriginalString.IsNullOrEmpty())
            {
                string optionDescription = workItemUri != null ? "--project-uri" : "--configuration";

                // The value '{0}' of the '{1}' option is not an absolute URI.
                Console.Error.WriteLine(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        MultitoolResources.WorkItemFiling_ErrorUriIsNotAbsolute,
                        workItemFilingConfiguration.WorkItemFilerUri.OriginalString,
                        optionDescription));
                return false;
            }

            return true;
        }

        private static bool EnsurePersonalAccessToken(SarifWorkItemContext workItemFilingConfiguration)
        {
            string pat = Environment.GetEnvironmentVariable("SarifWorkItemFilingPat");

            if (!string.IsNullOrEmpty(pat))
            {
                workItemFilingConfiguration.PersonalAccessToken = pat;
            }

            if (string.IsNullOrEmpty(workItemFilingConfiguration.PersonalAccessToken))
            {
                // "No security token was provided. Populate the 'SarifWorkItemFilingPat' environment 
                // variable with a valid personal access token or pass a token in a configuration file using
                // the --configuration arguement."
                Console.Error.WriteLine(MultitoolResources.WorkItemFiling_NoPatFound);
                return false;
            }

            return true;
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

            foreach (Result error in errors)
            {
                sb.AppendLine(error.FormatForVisualStudio(RuleFactory.GetRuleFromRuleId(error.RuleId)));
            }

            return sb.ToString();
        }
    }
}