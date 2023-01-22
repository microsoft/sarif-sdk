// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IEnumeratedArtifact
    {
        Uri Uri { get; }

        Stream Stream { get; set; }

        Encoding Encoding { get; set; }

        string Contents { get; set; }
    }
}
