// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IAnalysisContext : IDisposable
    {
        Uri TargetUri { get; set; }

        string MimeType { get; set; }

        HashData Hashes { get; set; }

        Exception TargetLoadException { get; set; }

        bool IsValidAnalysisTarget { get; }

        ReportingDescriptor Rule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        PropertiesDictionary Policy { get; set; }

        IAnalysisLogger Logger { get; set; }

        RuntimeConditions RuntimeErrors { get; set; }
    }
}
