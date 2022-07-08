// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class TestAnalysisContext : IAnalysisContext
    {
        public bool IsValidAnalysisTarget { get; set; }

        public IAnalysisLogger Logger { get; set; }

        public PropertiesDictionary Policy { get; set; }

        public ReportingDescriptor Rule { get; set; }

        public Exception TargetLoadException { get; set; }

        public Uri TargetUri { get; set; }

        public string MimeType { get; set; }

        public HashData Hashes { get; set; }

        public RuntimeConditions RuntimeErrors { get; set; }

        public TestAnalyzeOptions Options { get; set; }

        public bool AnalysisComplete { get; set; }

        public DefaultTraces Traces { get; set; }

        public bool Disposed { get; private set; }

        public int MaxFileSizeInKilobytes { get; set; }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}
