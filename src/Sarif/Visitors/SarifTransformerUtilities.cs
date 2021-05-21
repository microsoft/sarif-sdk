// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.VersionOne;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public static class SarifTransformerUtilities
    {
        public static readonly Dictionary<SarifVersion, string> PropertyBagTransformerItemPrefixes = new Dictionary<SarifVersion, string>()
        {
            { SarifVersion.OneZeroZero, "sarifv1" },
            { SarifVersion.Current, "sarifv2" }
        };

        public static readonly string[] DefaultFullyQualifiedNameDelimiters = { ".", "/", "\\", "::" };

        public static readonly JsonSerializerSettings JsonSettingsV1Indented = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolverVersionOne.Instance,
            Formatting = Formatting.Indented
        };

        public static readonly JsonSerializerSettings JsonSettingsIndented = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        public static readonly JsonSerializerSettings JsonSettingsV1Compact = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolverVersionOne.Instance
        };

        public static readonly Dictionary<AlgorithmKindVersionOne, string> AlgorithmKindNameMap = new Dictionary<AlgorithmKindVersionOne, string>
        {
            { AlgorithmKindVersionOne.Sha1, "sha-1" },
            { AlgorithmKindVersionOne.Sha3, "sha-3" },
            { AlgorithmKindVersionOne.Sha224, "sha-224" },
            { AlgorithmKindVersionOne.Sha256, "sha-256" },
            { AlgorithmKindVersionOne.Sha384, "sha-384" },
            { AlgorithmKindVersionOne.Sha512, "sha-512" }
        };

        public static readonly Dictionary<string, AlgorithmKindVersionOne> AlgorithmNameKindMap = new Dictionary<string, AlgorithmKindVersionOne>
        {
            { "sha-1", AlgorithmKindVersionOne.Sha1 },
            { "sha-3", AlgorithmKindVersionOne.Sha3 },
            { "sha-224", AlgorithmKindVersionOne.Sha224 },
            { "sha-256", AlgorithmKindVersionOne.Sha256 },
            { "sha-384", AlgorithmKindVersionOne.Sha384 },
            { "sha-512", AlgorithmKindVersionOne.Sha512 }
        };

        public static string CreateDisambiguatedName(string baseName, int index)
        {
            return $"{baseName}-{index.ToString(CultureInfo.InvariantCulture)}";
        }

        public static FailureLevel CreateFailureLevel(NotificationLevelVersionOne v1NotificationLevel)
        {
            switch (v1NotificationLevel)
            {
                case NotificationLevelVersionOne.Error:
                    return FailureLevel.Error;
                case NotificationLevelVersionOne.Note:
                    return FailureLevel.Note;
                default:
                    return FailureLevel.Warning;
            }
        }

        public static NotificationLevelVersionOne CreateNotificationLevelVersionOne(FailureLevel v2FailureLevel)
        {
            switch (v2FailureLevel)
            {
                case FailureLevel.Error:
                    return NotificationLevelVersionOne.Error;
                case FailureLevel.Note:
                    return NotificationLevelVersionOne.Note;
                default:
                    return NotificationLevelVersionOne.Warning;
            }
        }

        public static FailureLevel CreateReportingConfigurationDefaultLevel(ResultLevelVersionOne v1ResultLevel)
        {
            switch (v1ResultLevel)
            {
                case ResultLevelVersionOne.Error:
                    return FailureLevel.Error;
                case ResultLevelVersionOne.Pass:
                    return FailureLevel.Note;
                case ResultLevelVersionOne.Warning:
                    return FailureLevel.Warning;
                default:
                    return FailureLevel.Warning;
            }
        }

        public static FailureLevel CreateFailureLevel(ResultLevelVersionOne v1ResultLevel)
        {
            switch (v1ResultLevel)
            {
                case ResultLevelVersionOne.Error:
                    return FailureLevel.Error;
                case ResultLevelVersionOne.Note:
                    return FailureLevel.Note;
                case ResultLevelVersionOne.Pass:
                    return FailureLevel.None;
                case ResultLevelVersionOne.Warning:
                    return FailureLevel.Warning;
                case ResultLevelVersionOne.NotApplicable:
                    return FailureLevel.None;
                default:
                    return FailureLevel.Warning;
            }
        }

        public static ResultKind CreateResultKind(ResultLevelVersionOne v1ResultLevel)
        {
            switch (v1ResultLevel)
            {
                case ResultLevelVersionOne.Error:
                    return ResultKind.Fail;
                case ResultLevelVersionOne.Note:
                    return ResultKind.Fail;
                case ResultLevelVersionOne.Pass:
                    return ResultKind.Pass;
                case ResultLevelVersionOne.Warning:
                    return ResultKind.Fail;
                case ResultLevelVersionOne.NotApplicable:
                    return ResultKind.NotApplicable;
                default:
                    return ResultKind.Fail;
            }
        }

        public static ResultLevelVersionOne CreateResultLevelVersionOne(FailureLevel v2DefaultLevel)
        {
            switch (v2DefaultLevel)
            {
                case FailureLevel.Error:
                    return ResultLevelVersionOne.Error;
                case FailureLevel.Note:
                    return ResultLevelVersionOne.Pass;
                case FailureLevel.Warning:
                    return ResultLevelVersionOne.Warning;
                default:
                    return ResultLevelVersionOne.Warning;
            }
        }

        public static ResultLevelVersionOne CreateResultLevelVersionOne(FailureLevel v2FailureLevel, ResultKind v2ResultKind)
        {
            if (v2ResultKind != ResultKind.Fail)
            {
                v2FailureLevel = FailureLevel.None;
            }

            switch (v2FailureLevel)
            {
                case FailureLevel.Error:
                    return ResultLevelVersionOne.Error;
                case FailureLevel.Note:
                    return ResultLevelVersionOne.Note;
                case FailureLevel.Warning:
                    return ResultLevelVersionOne.Warning;
                case FailureLevel.None:
                    return CreateResultLevelVersionOneFromResultKind(v2ResultKind);
                default:
                    return ResultLevelVersionOne.Default;
            }
        }

        private static ResultLevelVersionOne CreateResultLevelVersionOneFromResultKind(ResultKind v2ResultKind)
        {
            switch (v2ResultKind)
            {
                case ResultKind.Pass:
                    return ResultLevelVersionOne.Pass;
                case ResultKind.NotApplicable:
                    return ResultLevelVersionOne.NotApplicable;
                // no mapped values for review, open
                default:
                    return ResultLevelVersionOne.Default;
            }
        }

        public static List<Suppression> CreateSuppressions(SuppressionStatesVersionOne v1SuppressionStates)
        {
            List<Suppression> suppressions = null;

            if (v1SuppressionStates.HasFlag(SuppressionStatesVersionOne.SuppressedExternally))
            {
                suppressions = new List<Suppression>
                {
                    new Suppression { Kind = SuppressionKind.External }
                };
            }

            if (v1SuppressionStates.HasFlag(SuppressionStatesVersionOne.SuppressedInSource))
            {
                suppressions = suppressions ?? new List<Suppression>();
                suppressions.Add(new Suppression { Kind = SuppressionKind.InSource });
            }

            return suppressions;
        }

        public static SuppressionStatesVersionOne CreateSuppressionStatesVersionOne(IList<Suppression> v2Suppressions)
        {
            if (v2Suppressions == null)
            {
                return SuppressionStatesVersionOne.None;
            }

            bool isSuppressedExternally = false;
            bool isSuppressedInSource = false;

            foreach (Suppression suppression in v2Suppressions)
            {
                switch (suppression.Kind)
                {
                    case SuppressionKind.External:
                        isSuppressedExternally = true;
                        break;
                    case SuppressionKind.InSource:
                        isSuppressedInSource = true;
                        break;
                }

                if (isSuppressedExternally && isSuppressedInSource)
                {
                    return SuppressionStatesVersionOne.SuppressedInSource | SuppressionStatesVersionOne.SuppressedExternally;
                }
            }

            if (isSuppressedInSource)
            {
                return SuppressionStatesVersionOne.SuppressedInSource;
            }

            if (isSuppressedExternally)
            {
                return SuppressionStatesVersionOne.SuppressedExternally;
            }

            return SuppressionStatesVersionOne.None;
        }

        public static BaselineState CreateBaselineState(BaselineStateVersionOne v1BaselineState)
        {
            switch (v1BaselineState)
            {
                case BaselineStateVersionOne.Absent:
                    return BaselineState.Absent;
                case BaselineStateVersionOne.Existing:
                    return BaselineState.Unchanged;
                case BaselineStateVersionOne.New:
                    return BaselineState.New;
                default:
                    return BaselineState.None;
            }
        }
        public static BaselineStateVersionOne CreateBaselineStateVersionOne(BaselineState v2BaselineState)
        {
            switch (v2BaselineState)
            {
                case BaselineState.Absent:
                    return BaselineStateVersionOne.Absent;
                case BaselineState.Unchanged:
                case BaselineState.Updated:
                    return BaselineStateVersionOne.Existing;
                case BaselineState.New:
                    return BaselineStateVersionOne.New;
                default:
                    return BaselineStateVersionOne.None;
            }
        }

        public static ThreadFlowLocationImportance CreateThreadFlowLocationImportance(AnnotatedCodeLocationImportanceVersionOne v1AnnotatedCodeLocationImportance)
        {
            switch (v1AnnotatedCodeLocationImportance)
            {
                case AnnotatedCodeLocationImportanceVersionOne.Essential:
                    return ThreadFlowLocationImportance.Essential;
                case AnnotatedCodeLocationImportanceVersionOne.Important:
                    return ThreadFlowLocationImportance.Important;
                case AnnotatedCodeLocationImportanceVersionOne.Unimportant:
                    return ThreadFlowLocationImportance.Unimportant;
                default:
                    return ThreadFlowLocationImportance.Important;
            }
        }

        public static AnnotatedCodeLocationImportanceVersionOne CreateAnnotatedCodeLocationImportance(ThreadFlowLocationImportance v2ThreadFlowLocationImportance)
        {
            switch (v2ThreadFlowLocationImportance)
            {
                case ThreadFlowLocationImportance.Essential:
                    return AnnotatedCodeLocationImportanceVersionOne.Essential;
                case ThreadFlowLocationImportance.Important:
                    return AnnotatedCodeLocationImportanceVersionOne.Important;
                case ThreadFlowLocationImportance.Unimportant:
                    return AnnotatedCodeLocationImportanceVersionOne.Unimportant;
                default:
                    return AnnotatedCodeLocationImportanceVersionOne.Important;
            }
        }

        public interface IBlameHunk
        {
            string Name { get; set; }

            string Email { get; set; }

            string CommitSha { get; set; }

            int LineCount { get; }

            int FinalStartLineNumber { get; }

            bool ContainsLine(int line);
        }

        private class BlameHunk : IBlameHunk
        {
            private readonly string _Name;
            private readonly string _Email;
            private readonly string _CommitSha;
            private readonly int _LineCount;
            private readonly int _FinalStartLineNumber;

            public BlameHunk(string name, string email, string commitSha, int lineCount, int finalStartLineNumber)
            {
                _Name = name;
                _Email = email;
                _CommitSha = commitSha;
                _LineCount = lineCount;
                _FinalStartLineNumber = finalStartLineNumber;
            }

            public string Name { get => _Name; set => throw new System.NotImplementedException(); }
            public string Email { get => _Email; set => throw new System.NotImplementedException(); }
            public string CommitSha { get => _CommitSha; set => throw new System.NotImplementedException(); }

            public int LineCount { get => _LineCount; set => throw new System.NotImplementedException(); }

            public int FinalStartLineNumber { get => _FinalStartLineNumber; set => throw new System.NotImplementedException(); }

            public bool ContainsLine(int line)
            {
                if (line >= _FinalStartLineNumber && line <= _FinalStartLineNumber + _LineCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static IEnumerable<IBlameHunk> ParseBlameInformation(string blameText)
        {
            string[] lines = blameText.Split('\n');
            var blameHunks = new List<BlameHunk>();

            string commitShaPattern = @"^(?<hash>[0-9a-f]{40}).*$";
            int commitShaLength = 40;
            string authorTZPattern = @"^author-tz";
            string authorTimePattern = @"^author-time";
            string authorMailPattern = @"^author-mail";
            string authorPattern = @"^author";

            string name = null, email = null, commitSha = null;
            int lineCount = 0, finalStartLineNumber = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                if (Regex.IsMatch(lines[i], commitShaPattern))
                {
                    string currentCommitSha = lines[i].Substring(0, commitShaLength - 1);

                    if (!currentCommitSha.Equals(commitSha))
                    {
                        if(commitSha != null)
                        {
                            // we have seen at least one valid commit detail before,
                            // so flush the existing data
                            blameHunks.Add(new BlameHunk(name, email, commitSha, lineCount, finalStartLineNumber));
                        }

                        // we are observing a fresh commit region
                        commitSha = currentCommitSha;
                        string[] commitInfoLine = lines[i].Split(' ');
                        finalStartLineNumber = int.Parse(commitInfoLine[2]);
                        lineCount = int.Parse(commitInfoLine[3]);
                    }
                }
                else if (Regex.IsMatch(lines[i], authorTZPattern))
                {
                    continue;
                }
                else if (Regex.IsMatch(lines[i], authorTimePattern))
                {
                    continue;
                }
                else if (Regex.IsMatch(lines[i], authorMailPattern))
                {
                    email = lines[i].Substring(13, (lines[i].Length - 1));
                    continue;
                }
                else if (Regex.IsMatch(lines[i], authorPattern))
                {
                    name = lines[i].Substring(6);
                    continue;
                }
            }

            return blameHunks;
        }
    }
}
