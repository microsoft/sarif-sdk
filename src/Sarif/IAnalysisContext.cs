// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IAnalysisContext : IDisposable
    {
        string BaselineFilePath { get; set; }

        string OutputFilePath { get; set; }

        bool Inline { get; set; }

        bool PrettyPrint { get; set; }

        bool Force { get; set; }

        IFileSystem FileSystem { get; set; }

        CancellationToken CancellationToken { get; set; }

        IArtifactProvider TargetsProvider { get; set; }

        IEnumeratedArtifact CurrentTarget { get; set; }

        ISet<string> TargetFileSpecifiers { get; set; }

        ISet<FailureLevel> FailureLevels { get; set; }

        ISet<ResultKind> ResultKinds { get; set; }

        OptionallyEmittedData DataToInsert { get; set; }

        OptionallyEmittedData DataToRemove { get; set; }

        bool Recurse { get; set; }

        int Threads { get; set; }

        string MimeType { get; set; }

        HashData Hashes { get; set; }

        Exception RuntimeException { get; set; }

        bool IsValidAnalysisTarget { get; }

        ReportingDescriptor Rule { get; set; }

        PropertiesDictionary Policy { get; set; }

        IAnalysisLogger Logger { get; set; }

        RuntimeConditions RuntimeErrors { get; set; }

        bool AnalysisComplete { get; set; }

        ISet<string> Traces { get; set; }

        long MaxFileSizeInKilobytes { get; set; }
    }
}
