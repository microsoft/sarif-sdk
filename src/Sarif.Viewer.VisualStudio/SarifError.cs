// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Sdk;
using Microsoft.VisualStudio.Text;

namespace SarifViewer
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

        public string Message { get; set; }

        public SnapshotSpan Span { get; set; }

        public int LineNumber { get; set; }

        public int ColumnNumber { get; set; }

        public bool IsError { get; set; }

        public string ErrorCode { get; set; }

        public string HelpLink { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}