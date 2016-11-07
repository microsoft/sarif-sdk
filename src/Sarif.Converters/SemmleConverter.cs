// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    /// <summary>
    /// Converts a log file from the Semmle format to the SARIF format.
    /// </summary>
    internal class SemmleConverter : ToolFileConverterBase
    {
        public override void Convert(Stream input, IResultLogWriter output)
        {
        }
    }
}
