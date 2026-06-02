// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

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

        // A rule id token: a known producer prefix followed by its numeric band.
        private static readonly Regex s_ruleId =
            new Regex(@"\b(?:AI|SARIF|GHAzDO|ADO|Base)\d{3,4}\b");

        // A rule id in canonical moniker form: `Id.PascalName`, backtick-wrapped,
        // id and name joined by a dot (e.g. `AI2011.DoNotPersistFingerprints`).
        private static readonly Regex s_quotedMoniker =
            new Regex(@"`(?:AI|SARIF|GHAzDO|ADO|Base)\d{3,4}\.[A-Za-z][A-Za-z0-9]*`");

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

        // Every rule id named anywhere in ReleaseHistory.md must be introduced by
        // its `Id.PascalName` moniker at least once within the same entry. A bare
        // back-reference (`AI1010` after `AI1010.ProvideEvidenceBackingUri`) is
        // fine — the moniker already carries id AND intent so the reader never
        // consults an id table. This is enforced file-wide: history is held to
        // the same bar, with each id's era-correct name (rule ids get renumbered,
        // so an old entry names the rule it meant at the time, not today's owner).
        [Fact]
        public void ReleaseHistory_RuleIdsUseQuotedMonikers()
        {
            string path = FindReleaseHistory();
            string[] lines = File.ReadAllLines(path);

            var offenders = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                var monikered = new HashSet<string>();
                foreach (Match moniker in s_quotedMoniker.Matches(line))
                {
                    string inner = moniker.Value.Trim('`');
                    monikered.Add(inner.Substring(0, inner.IndexOf('.')));
                }

                foreach (Match id in s_ruleId.Matches(line))
                {
                    if (!monikered.Contains(id.Value))
                    {
                        offenders.Add($"  line {i + 1}: '{id.Value}' has no `Id.Name` moniker in its entry: {line.Trim()}");
                    }
                }
            }

            offenders.Should().BeEmpty(
                "every rule id in ReleaseHistory.md must be introduced by a backtick-wrapped `Id.PascalName` moniker " +
                "(e.g. `AI2011.DoNotPersistFingerprints`) at least once within its entry; bare back-references to an " +
                "already-monikered id are fine. The moniker states both the rule's id and its intent so a reader never " +
                "has to consult an id table. Use the rule's era-correct name — ids get renumbered over time:\n" +
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
