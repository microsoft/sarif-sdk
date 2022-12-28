#if DEBUG
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
#pragma warning disable CS0618
    public class AnalyzeTestContext : AnalyzeContextBase
#pragma warning restore CS0618
    {
        public override Exception TargetLoadException { get; set; }

        public override bool IsValidAnalysisTarget => true;

        public override IAnalysisLogger Logger { get; set; }

        public override ReportingDescriptor Rule { get; set; }

        public override string MimeType { get; set; }

        public override HashData Hashes { get; set; }

        public override RuntimeConditions RuntimeErrors { get; set; }

        public override Uri TargetUri { get; set; }

        public override bool AnalysisComplete { get; set; }

        public override DefaultTraces Traces { get; set; }

        public override void Dispose() { }
    }
}
#endif
