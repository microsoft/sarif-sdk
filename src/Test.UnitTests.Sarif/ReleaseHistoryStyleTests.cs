// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using FluentAssertions;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests
{
    public class ReleaseHistoryStyleTests
    {
        // A release-notes entry states the abstract essentials of the change,
        // not its implementation or motivation. A run-on bullet is a PR
        // description in disguise; this cap flags one so the mechanics get
        // elided. The full convention lives in .github/copilot-instructions.md.
        private const int MaxEntryLength = 300;

        [Fact]
        public void ReleaseHistory_CurrentSectionEntriesAreTerse()
        {
            string path = FindReleaseHistory();
            string[] lines = File.ReadAllLines(path);

            (int start, int end) = GetCurrentSectionRange(lines);

            var offenders = new List<string>();
            for (int i = start; i < end; i++)
            {
                string line = lines[i];
                if (line.StartsWith("* ") && line.Length > MaxEntryLength)
                {
                    offenders.Add($"  line {i + 1} ({line.Length} chars): {line.Substring(0, 60)}...");
                }
            }

            offenders.Should().BeEmpty(
                $"each entry in the most recent ReleaseHistory.md section must stay under {MaxEntryLength} characters. " +
                "Exceeding it signals a failure to elide detail that already lives in the implementation: " +
                "state the abstract essentials of the change and leave mechanics (exact types, parameter names, edge cases) " +
                "to the API docs and PR description:\n" +
                string.Join("\n", offenders));
        }

        // Locate ReleaseHistory.md by walking up from the test binary to the
        // repo root. This is deliberately git-independent: the CI build runs in
        // a container where `git rev-parse` fails on "dubious ownership", so a
        // git-based enlistment probe returns null there.
        private static string FindReleaseHistory()
        {
            for (DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
                 dir != null;
                 dir = dir.Parent)
            {
                string candidate = Path.Combine(dir.FullName, "ReleaseHistory.md");
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            throw new FileNotFoundException(
                "Could not locate ReleaseHistory.md by walking up from " + AppContext.BaseDirectory);
        }

        // The current section spans the first '## ' version heading to the next
        // one (or end of file). Older sections may predate the terseness
        // convention and are not validated.
        private static (int start, int end) GetCurrentSectionRange(string[] lines)
        {
            int start = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("## "))
                {
                    start = i + 1;
                    break;
                }
            }

            start.Should().BeGreaterThan(0, "ReleaseHistory.md must contain at least one '## ' version section");

            int end = lines.Length;
            for (int i = start; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("## "))
                {
                    end = i;
                    break;
                }
            }

            return (start, end);
        }
    }
}
