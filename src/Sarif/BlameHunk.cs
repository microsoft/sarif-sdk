// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

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

        public string Name { get => name; }
        public string Email { get => email; }
        public string CommitSha { get => commitSha; }

        public int LineCount { get => lineCount; set => throw new System.NotImplementedException(); }

        public int FinalStartLineNumber { get => finalStartLineNumber; }

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
