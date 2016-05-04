// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Writers;

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

        private static readonly Rule UnknownErrorRule = new Rule
        {
            Id = "SV0001",
            Name = "UnknownError",
            ShortDescription = Resources.UnknownErrorRuleDescription,
            FullDescription = Resources.UnknownErrorRuleDescription,
            MessageFormats = new Dictionary<string, string>
            {
                [UnknownErrorFormatSpecifier] = Resources.UnknownErrorMessageFormat
            }
        };

        private const string JsonSyntaxErrorFormatSpecifier = "syntaxError";

        private static readonly Rule JsonSyntaxErrorRule = new Rule
        {
            Id = "SV0002",
            Name = "JsonSyntaxError",
            ShortDescription = Resources.JsonSyntaxErrorRuleDescription,
            FullDescription = Resources.JsonSyntaxErrorRuleDescription,
            MessageFormats = new Dictionary<string, string>
            {
                [JsonSyntaxErrorFormatSpecifier] = Resources.JsonSyntaxErrorMessageFormat
            }
        };

        private const string JsonSchemaValidationErrorFormatSpecifier = "validationError";

        private static readonly Rule JsonSchemaValidationErrorRule = new Rule
        {
            Id = "SV0003",
            Name = "JsonSchemaValidationError",
            ShortDescription = Resources.JsonSchemaValidationErrorRuleDescription,
            FullDescription = Resources.JsonSchemaValidationErrorRuleDescription,
            MessageFormats = new Dictionary<string, string>
            {
                [JsonSchemaValidationErrorFormatSpecifier] = Resources.JsonSchemaValidationErrorMessageFormat
            }
        };

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
                analysisTargets: new[] { instanceFilePath, schemaFilePath },
                verbose: true,
                computeTargetsHash: false,
                logEnvironment: false,
                prereleaseInfo: null,
                invocationTokensToRedact:null);

            _logger.AnalysisStarted();
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
            IRule rule = GetRuleForError(error);
            Result result = MakeResultFromError(error);

            _logger.Log(rule, result);
            _messages.Add(result.FormatForVisualStudio(rule));
        }

        private static IRule GetRuleForError(JsonError error)
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

            Uri analysisTargetUri = new Uri(analysisTargetFilePath);

            switch (error.Kind)
            {
                case JsonErrorKind.Syntax:
                    result.RuleId = JsonSyntaxErrorRule.Id;
                    result.Level = ResultLevel.Error;
                    result.FormattedRuleMessage = new FormattedRuleMessage
                    {
                        FormatId = JsonSyntaxErrorFormatSpecifier,
                        Arguments = new[] { error.Message }
                    };
                    break;

                case JsonErrorKind.Validation:
                    result.RuleId = JsonSchemaValidationErrorRule.Id;
                    result.Level = ResultLevel.Error;
                    result.FormattedRuleMessage = new FormattedRuleMessage
                    {
                        FormatId = JsonSchemaValidationErrorFormatSpecifier,
                        Arguments = new[] { error.Message }
                    };
                    break;

                default:
                    result.RuleId = UnknownErrorRule.Id;
                    result.Level = ResultLevel.Error;
                    result.FormattedRuleMessage = new FormattedRuleMessage
                    {
                        FormatId = UnknownErrorFormatSpecifier,
                        Arguments = new string[] { error.Kind.ToString() }
                    };
                    break;
            }

            Region region;
            if (index != null)
            {
                region = new Region
                {
                    Offset = error.Start,
                    Length = error.Length
                };

                region.Populate(index);
            }
            else
            {
                region = new Region();
            }

            var location = new Location
            {
                AnalysisTarget = new PhysicalLocation
                {
                    Uri = analysisTargetUri,
                    Region = region
                }
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
                        _logger.AnalysisStopped(RuntimeConditions.NoErrors);
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