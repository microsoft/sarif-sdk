// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IConsoleCapture
    {
        string Text { get; }
        Task<string> Capture(StreamReader reader, CancellationToken cancellationToken);
    }
}
