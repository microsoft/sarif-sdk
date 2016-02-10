// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Driver.Sdk;

namespace Microsoft.Sarif.Viewer
{
    public class AnnotatedCodeLocationModel
    {
        public int Index { get; set; }

        public string Message { get; set; }

        public string FilePath { get; set; }

        public Region Region { get; set; }

        public AnnotatedCodeLocationKind Kind { get; set; }

        public string Location { get { return Region.FormatForVisualStudio(); } }

    }
}