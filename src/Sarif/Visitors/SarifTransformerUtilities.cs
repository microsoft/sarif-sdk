// Copyright(c) Microsoft.All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
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
                suppressions = new List<Suppression>();
                suppressions.Add(new Suppression { Kind = SuppressionKind.External });
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
    }
}