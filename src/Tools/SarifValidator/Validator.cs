// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.JSON.Core.Parser;
using Microsoft.JSON.Core.Parser.TreeItems;
using Microsoft.JSON.Core.Schema;
using Microsoft.JSON.Core.Schema.Drafts.Draft4;

namespace Microsoft.CodeAnalysis.Sarif.SarifValidator
{
    internal static class Validator
    {
        internal static IEnumerable<JsonError> ValidateFile(string instanceFilePath, string schemaFilePath)
        {
            string instanceText = File.ReadAllText(instanceFilePath);
            string schemaText = File.ReadAllText(schemaFilePath);

            return Validate(instanceText, schemaText);
        }

        private static IEnumerable<JsonError> Validate(string instanceText, string schemaText)
        {
            List<JsonError> errors = new List<JsonError>();

            JSONDocument instanceDocument = JSONParser.Parse(instanceText);
            AddSyntaxErrors(instanceDocument, JsonErrorLocation.InstanceDocument, errors);

            JSONDocument schemaDocument = JSONParser.Parse(schemaText);
            AddSyntaxErrors(schemaDocument, JsonErrorLocation.Schema, errors);

            var loader = new JSONDocumentLoader();
            loader.SetCacheItem(new JSONDocumentLoadResult(instanceDocument));
            loader.SetCacheItem(new JSONDocumentLoadResult(schemaDocument));

            JSONSchemaDraft4EvaluationTreeNode tree =
                JSONSchemaDraft4EvaluationTreeProducer.CreateEvaluationTreeAsync(
                        instanceDocument,
                        (JSONObject)schemaDocument.TopLevelValue,
                        loader,
                        Enumerable.Empty<IJSONSchemaFormatHandler>()).Result;
            AddValidationErrors(tree.ValidationIssues, errors);

            return errors;
        }

        private static void AddSyntaxErrors(JSONDocument document, JsonErrorLocation location, List<JsonError> errors)
        {
            foreach (Tuple<JSONParseItem, JSONParseError> parserError in document.GetContainedParseErrors())
            {
                errors.Add(new JsonError
                {
                    Kind = JsonErrorKind.Syntax,
                    Start = parserError.Item1.Start,
                    Length = parserError.Item1.Length,
                    Location = location,
                    Message = parserError.Item2.Text ?? parserError.Item2.ErrorType.ToString()
                });
            }
        }

        private static void AddValidationErrors(HashSet<JSONSchemaValidationIssue> validationIssues, List<JsonError> errors)
        {
            foreach (JSONSchemaValidationIssue issue in validationIssues)
            {
                errors.Add(new JsonError
                {
                    Kind = JsonErrorKind.Validation,
                    Start = issue.TargetItem.Start,
                    Length = issue.TargetItem.Length,
                    Location = JsonErrorLocation.InstanceDocument,
                    Message = issue.Message
                });
            }
        }
    }
}