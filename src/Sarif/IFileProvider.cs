// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IArtifactProvider
    {
        IEnumerable<IArtifact> Artifacts { get; }
    }

    public interface IArtifact
    {
        Uri Uri { get; set; }

        string Text { get; set;  }

        Encoding Encoding { get; set; }
    }
}
