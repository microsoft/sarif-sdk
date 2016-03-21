// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Validation
{
    public class ResultLogBuilder: IDisposable
    {
        private readonly string _instanceFilePath;
        private readonly string _schemaFilePath;
        private readonly IFileSystem _fileSystem;
        private readonly SarifLogger _logger;
        private readonly List<string> _messages;

        private NewLineIndex _instanceFileIndex;
        private NewLineIndex _schemaFileIndex;

        private const string JsonMimeType = "application/json";

        private const string UnknownErrorFormatSpecifier = "unknownError";

        private static readonly RuleDescriptor UnknownErrorRule = new RuleDescriptor(
            "SV0001",
            "UnknownError",
            Resources.UnknownErrorRuleDescription,
            Resources.UnknownErrorRuleDescription,
            null,           // options
            new Dictionary<string, string>           // formatSpecifiers
            {
                [UnknownErrorFormatSpecifier] = Resources.UnknownErrorMessageFormat
            },
            null,           // helpUri
            null,           // properties
            null);          // tags

        private const string JsonSyntaxErrorFormatSpecifier = "syntaxError";

        private static readonly RuleDescriptor JsonSyntaxErrorRule = new RuleDescriptor(
            "SV0002",
            "JsonSyntaxError",
            Resources.JsonSyntaxErrorRuleDescription,
            Resources.JsonSyntaxErrorRuleDescription,
            null,           // options
            new Dictionary<string, string>           // formatSpecifiers
            {
                [JsonSyntaxErrorFormatSpecifier] = Resources.JsonSyntaxErrorMessageFormat
            },
            null,           // helpUri
            null,           // properties
            null);          // tags

        private const string JsonSchemaValidationErrorFormatSpecifier = "validationError";

        private static readonly RuleDescriptor JsonSchemaValidationErrorRule = new RuleDescriptor(
            "SV0003",
            "JsonSchemaValidationError",
            Resources.JsonSchemaValidationErrorRuleDescription,
            Resources.JsonSchemaValidationErrorRuleDescription,
            null,           // options
            new Dictionary<string, string>           // formatSpecifiers
            {
                [JsonSchemaValidationErrorFormatSpecifier] = Resources.JsonSchemaValidationErrorMessageFormat
            },
            null,           // helpUri
            null,           // properties
            null);          // tags

        public ResultLogBuilder(
            string instanceFilePath,
            string schemaFilePath,
            string outputFilePath,
            IFileSystem fileSystem)
        {
            _instanceFilePath = instanceFilePath;
            _schemaFilePath = schemaFilePath;
            _fileSystem = fileSystem;
            _messages = new List<string>();

            _logger = new SarifLogger(
                outputFilePath,
                true,           // Produce verbose output.
                new[] { instanceFilePath, schemaFilePath },
                false,          // Do not compute target hash.
                null);          // The version of this tool has no prerelease info.
        }

        public IEnumerable<string> BuildLog(IEnumerable<JsonError> errors)
        {
            foreach (JsonError error in errors)
            {
                LogError(error);
            }

            return _messages;
        }

        private NewLineIndex InstanceFileIndex
        {
            get
            {
                if (_instanceFileIndex == null)
                {
                    _instanceFileIndex = new NewLineIndex(_fileSystem.ReadAllText(_instanceFilePath));
                }

                return _instanceFileIndex;
            }
        }

        private NewLineIndex SchemaFileIndex
        {
            get
            {
                if (_schemaFileIndex == null)
                {
                    _schemaFileIndex = new NewLineIndex(_fileSystem.ReadAllText(_schemaFilePath));
                }

                return _schemaFileIndex;
            }
        }

        private void LogError(JsonError error)
        {
            IRuleDescriptor rule = GetRuleDescriptorForError(error);
            Result result = MakeResultFromError(error);

            _logger.Log(rule, result);
            _messages.Add(result.FormatForVisualStudio(rule));
        }

        private static IRuleDescriptor GetRuleDescriptorForError(JsonError error)
        {
            switch (error.Kind)
            {
                case JsonErrorKind.Syntax:
                    return JsonSyntaxErrorRule;

                case JsonErrorKind.Validation:
                    return JsonSchemaValidationErrorRule;

                default:
                    return UnknownErrorRule;
            }
        }

        private Result MakeResultFromError(JsonError error)
        {
            var result = new Result();

            string analysisTargetFilePath;
            NewLineIndex index = null;

            switch (error.Location)
            {
                case JsonErrorLocation.InstanceDocument:
                    analysisTargetFilePath = _instanceFilePath;
                    index = InstanceFileIndex;
                    break;

                case JsonErrorLocation.Schema:
                    analysisTargetFilePath = _schemaFilePath;
                    index = SchemaFileIndex;
                    break;

                default:
                    analysisTargetFilePath = "unknown_file";
                    break;
            }

            Uri analysisTargetUri = analysisTargetFilePath.CreateUriForJsonSerialization();

            switch (error.Kind)
            {
                case JsonErrorKind.Syntax:
                    result.RuleId = JsonSyntaxErrorRule.Id;
                    result.Kind = ResultKind.Error;
                    result.FormattedMessage = new FormattedMessage(
                        JsonSyntaxErrorFormatSpecifier, new string[] { error.Message });
                    break;

                case JsonErrorKind.Validation:
                    result.RuleId = JsonSchemaValidationErrorRule.Id;
                    result.Kind = ResultKind.Error;
                    result.FormattedMessage = new FormattedMessage(
                        JsonSchemaValidationErrorFormatSpecifier, new string[] { error.Message });
                    break;

                default:
                    result.RuleId = UnknownErrorRule.Id;
                    result.Kind = ResultKind.InternalError;
                    result.FormattedMessage = new FormattedMessage(
                        UnknownErrorFormatSpecifier, new string[] { error.Kind.ToString() });
                    break;
            }

            Region region;
            if (index != null)
            {
                region = new Region
                {
                    CharOffset = error.Start,
                    Length = error.Length
                };

                region.Populate(index);
            }
            else
            {
                region = new Region();
            }

            var plc = new PhysicalLocation
            {
                Uri = analysisTargetUri,
                Region = region
            };

            var location = new Location
            {
                AnalysisTarget = new PhysicalLocation(plc)
            };

            result.Locations = new List<Location> { location };

            return result;
        }

        #region IDisposable

        private bool _isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_logger != null)
                    {
                        _logger.Dispose();
                    }
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable
    }
}