// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    [CollectionDataContract]
    public class TSLintLog : List<TSLintLogEntry>
    {
    }
}
