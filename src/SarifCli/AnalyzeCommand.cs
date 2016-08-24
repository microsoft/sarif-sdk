// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.Json.Schema;
using Microsoft.Json.Schema.Validation;

namespace Microsoft.CodeAnalysis.Sarif.Cli
{
    internal class AnalyzeCommand : AnalyzeCommandBase<SarifValidationContext, AnalyzeOptions>
    {
        public override string Prerelease => VersionConstants.Prerelease;

        private List<Assembly> _defaultPlugInAssemblies;

        public override IEnumerable<Assembly> DefaultPlugInAssemblies
        {
            get
            {
                if (_defaultPlugInAssemblies == null)
                {
                    _defaultPlugInAssemblies = new List<Assembly>
                    {
                        this.GetType().Assembly
                    };
                }

                return _defaultPlugInAssemblies;
            }
        }

        protected override SarifValidationContext CreateContext(AnalyzeOptions options, IAnalysisLogger logger, RuntimeConditions runtimeErrors, string filePath = null)
        {
            SarifValidationContext context = base.CreateContext(options, logger, runtimeErrors, filePath);
            context.SchemaFilePath = options.SchemaFilePath;
            return context;
        }

        protected override void AnalyzeTarget(IEnumerable<ISkimmer<SarifValidationContext>> skimmers, SarifValidationContext context, HashSet<string> disabledSkimmers)
        {
            // The base class knows how to invoke the skimmers that implement smart validation...
            base.AnalyzeTarget(skimmers, context, disabledSkimmers);

            // ... but it doesn't know how to invoke schema validation, which has its own set of rules,
            // so we do that ourselves.
            Validate(context.TargetUri.LocalPath, context.SchemaFilePath, context.Logger);
        }

        private void Validate(string instanceFilePath, string schemaFilePath, IAnalysisLogger logger)
        {
            try
            {
                string instanceText = File.ReadAllText(instanceFilePath);
                PerformSchemaValidation(instanceText, instanceFilePath, schemaFilePath, logger);
            }
            catch (JsonSyntaxException ex)
            {
                ReportResult(ex.Result, logger);
            }
            catch (SchemaValidationException ex)
            {
                ReportInvalidSchemaErrors(ex, schemaFilePath, logger);
            }
            catch (Exception ex)
            {
                LogToolNotification(logger, ex.Message, NotificationLevel.Error, ex);
            }
        }

        private void PerformSchemaValidation(
            string instanceText,
            string instanceFilePath,
            string schemaFilePath,
            IAnalysisLogger logger)
        {
            string schemaText = File.ReadAllText(schemaFilePath);
            JsonSchema schema = SchemaReader.ReadSchema(schemaText, schemaFilePath);

            var validator = new Validator(schema);
            Result[] results = validator.Validate(instanceText, instanceFilePath);

            ReportResults(results, logger);
        }

        private static void ReportInvalidSchemaErrors(
            SchemaValidationException ex,
            string schemaFile,
            IAnalysisLogger logger)
        {
            foreach (Result result in ex.Results)
            {
                result.SetAnalysisTargetUri(schemaFile);
                ReportResult(result, logger);
            }
        }

        private static void ReportResults(
            Result[] results,
            IAnalysisLogger logger)
        {
            foreach (Result result in results)
            {
                ReportResult(result, logger);
            }
        }

        private static void ReportResult(
            Result result,
            IAnalysisLogger logger)
        {
            Rule rule = Json.Schema.Sarif.RuleFactory.GetRuleFromRuleId(result.RuleId);
            logger.Log(rule, result);
        }

        private static void LogToolNotification(
            IAnalysisLogger logger,
            string message,
            NotificationLevel level = NotificationLevel.Note,
            Exception ex = null)
        {
            ExceptionData exceptionData = null;
            if (ex != null)
            {
                exceptionData = new ExceptionData
                {
                    Kind = ex.GetType().FullName,
                    Message = ex.Message,
                    Stack = Stack.CreateStacks(ex).FirstOrDefault()
                };
            }

            TextWriter writer = level == NotificationLevel.Error ? Console.Error : Console.Out;
            writer.WriteLine(message);

            logger.LogToolNotification(new Notification
            {
                Level = level,
                Message = message,
                Exception = exceptionData
            });
        }
    }
}
