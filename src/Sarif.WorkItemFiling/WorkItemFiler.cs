// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Json.Schema;
using Microsoft.Json.Schema.Validation;

namespace Microsoft.CodeAnalysis.Sarif.WorkItemFiling
{
    public class WorkItemFiler
    {
        private static readonly Validator s_logFileValidator = CreateLogFileValidator();
        private readonly IFileSystem _fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemFiler"> class.</see>
        /// </summary>
        /// <param name="fileSystem">
        /// An object that implements the <see cref="IFileSystem"/> interface, providing
        /// access to the file system.
        /// </param>
        public WorkItemFiler(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        /// <summary>
        /// Files work items from the results in a SARIF log file.
        /// </summary>
        /// <param name="path">
        /// The path to the SARIF log file.
        /// </param>
        public void FileWorkItems(string path)
        {
            if (path == null) { throw new ArgumentNullException(nameof(path)); }

            ValidateLogFileAgainstSarifSchema(path);
        }

        private void ValidateLogFileAgainstSarifSchema(string path)
        {
            string logFileContents = _fileSystem.ReadAllText(path);

            // The second argument is a file path that the Validator uses when reporting
            // errors. The Validator does not attempt to access this file.
            Result[] errors = s_logFileValidator.Validate(logFileContents, path);

            if (errors?.Length > 0)
            {
                throw new ArgumentException(FormatSchemaErrors(path, errors), nameof(path));
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
    }
}
