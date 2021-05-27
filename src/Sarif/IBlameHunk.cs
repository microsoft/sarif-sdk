// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public interface IBlameHunk
    {
        string Name { get; }

        string Email { get; }

        string CommitSha { get; }

        int LineCount { get; }

        int FinalStartLineNumber { get; }

        bool ContainsLine(int line);
    }
}
