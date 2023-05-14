// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IAnalysisContext : IDisposable
    {
        IList<VersionControlDetails> VersionControlProvenance { get; set; }

        string PostUri { get; set; }

        string OutputFilePath { get; set; }

        string BaselineFilePath { get; set; }

        string EventsFilePath { get; set; }

        string ConfigurationFilePath { get; set; }

        public bool Quiet { get; set; }

        public bool RichReturnCode { get; set; }

        public string AutomationId { get; set; }

        public Guid? AutomationGuid { get; set; }

        FilePersistenceOptions OutputFileOptions { get; set; }

        IFileSystem FileSystem { get; set; }

        CancellationToken CancellationToken { get; set; }

        IArtifactProvider TargetsProvider { get; set; }

        IEnumeratedArtifact CurrentTarget { get; set; }

        DriverEventSource EventSource { get; set; }

        public ISet<string> InvocationPropertiesToLog { get; set; }

        ISet<string> TargetFileSpecifiers { get; set; }

        ISet<string> PluginFilePaths { get; set; }

        FailureLevelSet FailureLevels { get; set; }

        ResultKindSet ResultKinds { get; set; }

        public ISet<string> InsertProperties { get; set; }

        OptionallyEmittedData DataToInsert { get; set; }

        OptionallyEmittedData DataToRemove { get; set; }

        bool Recurse { get; set; }

        int Threads { get; set; }

        HashData Hashes { get; set; }

        IList<Exception> RuntimeExceptions { get; set; }

        bool IsValidAnalysisTarget { get; }

        ReportingDescriptor Rule { get; set; }

        PropertiesDictionary Policy { get; set; }

        IAnalysisLogger Logger { get; set; }

        RuntimeConditions RuntimeErrors { get; set; }

        bool AnalysisComplete { get; set; }

        ISet<string> Traces { get; set; }

        long MaxFileSizeInKilobytes { get; set; }

        int TimeoutInMilliseconds { get; set; }
    }
}
