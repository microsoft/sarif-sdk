// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class ValidatingVisitor : SarifRewritingVisitor, IDisposable
    {
        private static Rule InvalidUri = new Rule()
        {
            Id = "SARIF001",
            ShortDescription = "Uri was not valid.",
            Name = nameof(InvalidUri)
        };

        private SarifLogger _sarifLog;

        public ValidatingVisitor(TextWriter writer)
        {
            var tool = Tool.CreateFromAssemblyData();
            tool.Name = "SarifValidatingVisitor";

            _sarifLog = new SarifLogger(writer, verbose: true, tool: tool);
        }

        public override FileData VisitFileData(FileData node)
        {
            ValidateUri(node.Uri);
            return base.VisitFileData(node);
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            ValidateUri(node.Uri);
            return base.VisitPhysicalLocation(node);
        }

        public override StackFrame VisitStackFrame(StackFrame node)
        {
            ValidateUri(node.Uri);
            return base.VisitStackFrame(node);
        }

        public override Rule VisitRule(Rule node)
        {
            ValidateUri(node.HelpUri);
            return base.VisitRule(node);
        }

        public override FileChange VisitFileChange(FileChange node)
        {
            ValidateUri(node.Uri);
            return base.VisitFileChange(node);
        }

        private void ValidateUri(Uri uri)
        {
            if (!uri.IsWellFormedOriginalString())
            {
                // 'uri' member of '{0}' instance is not valid: '{1}'
                string message = string.Format(
                    SdkResources.SARIF001_InvalidUri,
                    "fileData",
                    uri.OriginalString);

                _sarifLog.Log(InvalidUri,
                    new Result
                    {
                        RuleId = InvalidUri.Id,
                        Message = message
                    });
            }
        }

        public void Dispose()
        {
            if (_sarifLog != null)
            {
                _sarifLog.Dispose();
                _sarifLog = null;
            }
        }
    }
}

