// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class AnalyzeTestContext : IAnalysisContext
    {
        public Exception TargetLoadException { get; set; }

        public bool IsValidAnalysisTarget 
        { 
            get => true; 
            set => throw new InvalidOperationException(); 
        }

        public IAnalysisLogger Logger { get; set; }

        public ReportingDescriptor Rule { get; set; }

        public PropertiesDictionary Policy { get; set; }

        public string MimeType { get; set; }

        public HashData Hashes { get; set; }

        public RuntimeConditions RuntimeErrors { get; set; }

        public Uri TargetUri { get; set; }

        public bool AnalysisComplete { get; set; }

        public void Dispose() { }
    }
}
