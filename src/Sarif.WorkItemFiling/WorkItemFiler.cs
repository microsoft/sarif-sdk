// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Json.Schema;
using Microsoft.Json.Schema.Validation;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public class WorkItemFiler
    {
        private static readonly Validator s_logFileValidator = CreateLogFileValidator();
        private static readonly Task<IEnumerable<Result>> s_noFiledResults = Task.FromResult(new List<Result>().AsEnumerable());

        private readonly FilingTargetBase _filingTarget;
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemFiler"> class.</see>
        /// </summary>
        /// <param name="fileSystem">
        /// An object that implements the <see cref="IFileSystem"/> interface, providing
        /// access to the file system.
        /// </param>
        /// <param name="filingTarget">
        /// An object that represents the system (for example, GitHub or AzureDevOps)
        /// to which the work items will be filed.
        /// </param>
        public WorkItemFiler(FilingTargetBase filingTarget, IFileSystem fileSystem)
        {
            _filingTarget = filingTarget ?? throw new ArgumentNullException(nameof(filingTarget));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>
        /// Files work items from the results in a SARIF log file.
        /// </summary>
        /// <param name="logFilePath">
        /// The path to the SARIF log file.
        /// </param>
        /// <returns>
        /// The set of results that were filed as work items.
        /// </returns>
        public async Task<IEnumerable<Result>> FileWorkItems(string logFilePath)
        {
            if (logFilePath == null) { throw new ArgumentNullException(nameof(logFilePath)); }

            string logFileContents = _fileSystem.ReadAllText(logFilePath);

            EnsureValidSarifLogFile(logFileContents, logFilePath);

            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(logFileContents);

            // CONSIDER: Multiple runs?
            if (sarifLog.Runs[0]?.Results?.Count > 0)
            {
                // TODO: Extract this filtering logic into some sort of "filtering strategy" object.
                IList<Result> filteredResults = FilterResults(sarifLog.Runs[0].Results);

                // TODO: Bucketing. (We will want some sort of "bucketing strategy" object.)

                return filteredResults.Any()
                    ? await _filingTarget.FileWorkItems(filteredResults)
                    : await s_noFiledResults;
            }
            else
            {
                // There were no results at all in the log.
                return await s_noFiledResults;
            }
        }

        private void EnsureValidSarifLogFile(string logFileContents, string logFilePath)
        {
            // The second argument is a file path that the Validator uses when reporting
            // errors. The Validator does not attempt to access this file.
            Result[] errors = s_logFileValidator.Validate(logFileContents, logFilePath);

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
            const string SarifSchemaResource = "Microsoft.CodeAnalysis.Sarif.WorkItemFiling.sarif-2.1.0.json";
            Assembly thisAssembly = typeof(WorkItemFiler).Assembly;

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
            string firstMessageLine = string.Format(CultureInfo.CurrentCulture, Resources.ErrorInvalidSarifLogFile, path);

            var sb = new StringBuilder(firstMessageLine);
            sb.AppendLine();

            foreach (var error in errors)
            {
                sb.AppendLine(error.FormatForVisualStudio(RuleFactory.GetRuleFromRuleId(error.RuleId)));
            }

            return sb.ToString();
        }

        // Only file new results as bugs.
        private IList<Result> FilterResults(IList<Result> allResults)
           => allResults.Where(r => r.BaselineState == BaselineState.New).ToList();
    }
}
