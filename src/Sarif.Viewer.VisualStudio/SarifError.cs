// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Sarif.Viewer
{
    public class SarifError
    {
        public SarifError(string fileName)
        {
            FileName = fileName;
        }

        public string Tool { get; set; }

        public string MimeType { get; set; }

        public bool RegionPopulated { get; set; }

        public Region Region { get; set; }

        public string FileName { get; set; }

        public string ShortMessage { get; set; }

        public string FullMessage { get; set; }

        public SnapshotSpan Span { get; set; }

        public int LineNumber { get; set; }

        public int ColumnNumber { get; set; }

        public string Category { get; set; }

        public ResultKind Kind { get; set; }

        public string RuleId { get; set; }

        public string RuleName { get; set; }

        public string HelpLink { get; set; }

        public bool HasLines { get; internal set; }

        public IEnumerable<AnnotatedCodeLocationModel> Annotations { get; internal set;}

        public override string ToString()
        {
            return ShortMessage;
        }
    }
}