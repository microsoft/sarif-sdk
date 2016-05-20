// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class ValidatingVisitor : SarifRewritingVisitor, IDisposable
    {
        private static Rule FileUriCanBeOmitted = new Rule()
        {
            Id = "SARIF001",
            ShortDescription = "File 'uri' property matches its files dictionary key and can be omitted.",
            Name = nameof(FileUriCanBeOmitted),
            MessageFormats = new Diction
        };

        private static Rule LogicalLocationNameCanBeOmitted = new Rule()
        {
            Id = "SARIF002",
            ShortDescription = "File 'uri' property matches its files dictionary key and can be omitted.",
            Name = nameof(LogicalLocationNameCanBeOmitted)
        };



        private SarifLogger _sarifLog;

        public ValidatingVisitor(TextWriter writer)
        {
            var tool = Tool.CreateFromAssemblyData();
            tool.Name = "SarifValidatingVisitor";

            _sarifLog = new SarifLogger(writer, verbose: true, tool: tool);
        }

        public override Run VisitRun(Run node)
        {
            ValidateFileKeysAreNotDuplicatedAsFileDataUris(node.Files);
            ValidateLogicalLocationKeysAreNotDuplicatedAsLogicalLocationNames(node.LogicalLocations);

            return base.VisitRun(node);
        }

        private void ValidateLogicalLocationKeysAreNotDuplicatedAsLogicalLocationNames(IDictionary<string, LogicalLocation> logicalLocations)
        {
            if (logicalLocations == null) { return; }
        }

        private void ValidateFileKeysAreNotDuplicatedAsFileDataUris(IDictionary<string, FileData> files)
        {
            if (files == null) { return; }
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

