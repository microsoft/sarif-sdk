// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    internal class BlameHunk : IBlameHunk
    {
        private readonly string name;
        private readonly string email;
        private readonly string commitSha;
        private readonly int lineCount;
        private readonly int finalStartLineNumber;

        public BlameHunk(string name, string email, string commitSha, int lineCount, int finalStartLineNumber)
        {
            this.name = name;
            this.email = email;
            this.commitSha = commitSha;
            this.lineCount = lineCount;
            this.finalStartLineNumber = finalStartLineNumber;
        }

        public string Name => name;

        public string Email => email;

        public string CommitSha => commitSha;

        public int LineCount => lineCount;

        public int FinalStartLineNumber => finalStartLineNumber;

        public bool ContainsLine(int line)
        {
            if (line >= finalStartLineNumber && line <= finalStartLineNumber + lineCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
