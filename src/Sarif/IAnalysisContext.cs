// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IAnalysisContext : IDisposable
    {
        // TODO place these target-relevant properties
        // in an ITargetDescriptor object

        Uri TargetUri { get; set; }

        string MimeType { get; set; }

        Exception TargetLoadException { get; set;  }

        bool IsValidAnalysisTarget { get;  }

        IRule Rule { get; set; }

        PropertyBag Policy { get; set; }
        
        IAnalysisLogger Logger { get; set; }

        RuntimeConditions RuntimeErrors { get; set; }
    }
}
