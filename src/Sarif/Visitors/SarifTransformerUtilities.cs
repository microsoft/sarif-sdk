// Copyright(c) Microsoft.All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            { SarifVersion.TwoZeroZero, "sarifv2" }
        };

        public static readonly string[] DefaultFullyQualifiedNameDelimiters = { ".", "/", "\\", "::" };

        public static readonly JsonSerializerSettings JsonSettingsV1 = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolverVersionOne.Instance,
            Formatting = Formatting.Indented
        };

        public static readonly JsonSerializerSettings JsonSettingsV2 = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolver.Instance,
            Formatting = Formatting.Indented
        };

        public static readonly JsonSerializerSettings JsonSettingsV1Compact = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolverVersionOne.Instance
        };

        public static readonly JsonSerializerSettings JsonSettingsV2Compact = new JsonSerializerSettings
        {
            ContractResolver = SarifContractResolver.Instance
        };

        #region Text MIME types
        public static HashSet<string> TextMimeTypes = new HashSet<string>()
        {
            "application/ecmascript",
            "application/javascript",
            "application/json",
            "application/rss+xml",
            "application/rtf",
            "application/typescript",
            "application/x-csh",
            "application/xhtml+xml",
            "application/xml",
            "application/x-sh",
            "text/css",
            "text/csv",
            "text/ecmascript",
            "text/html",
            "text/javascript",
            "text/plain",
            "text/richtext",
            "text/sgml",
            "text/tab-separated-values",
            "text/tsv",
            "text/uri-list",
            "text/x-asm",
            "text/x-c",
            "text/x-csharp",
            "text/x-h",
            "text/x-java-source",
            "text/x-java-source,java",
            "text/xml",
            "text/x-pascal"
        };
        #endregion

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

        public static NotificationLevel CreateNotificationLevel(NotificationLevelVersionOne v1NotificationLevel)
        {
            switch (v1NotificationLevel)
            {
                case NotificationLevelVersionOne.Error:
                    return NotificationLevel.Error;
                case NotificationLevelVersionOne.Note:
                    return NotificationLevel.Note;
                default:
                    return NotificationLevel.Warning;
            }
        }

        public static NotificationLevelVersionOne CreateNotificationLevelVersionOne(NotificationLevel v2NotificationLevel)
        {
            switch (v2NotificationLevel)
            {
                case NotificationLevel.Error:
                    return NotificationLevelVersionOne.Error;
                case NotificationLevel.Note:
                    return NotificationLevelVersionOne.Note;
                default:
                    return NotificationLevelVersionOne.Warning;
            }
        }

        public static RuleConfigurationDefaultLevel CreateRuleConfigurationDefaultLevel(ResultLevelVersionOne v1ResultLevel)
        {
            switch (v1ResultLevel)
            {
                case ResultLevelVersionOne.Error:
                    return RuleConfigurationDefaultLevel.Error;
                case ResultLevelVersionOne.Pass:
                    return RuleConfigurationDefaultLevel.Note;
                case ResultLevelVersionOne.Warning:
                    return RuleConfigurationDefaultLevel.Warning;
                default:
                    return RuleConfigurationDefaultLevel.Warning;
            }
        }

        public static ResultLevel CreateResultLevel(ResultLevelVersionOne v1ResultLevel)
        {
            switch (v1ResultLevel)
            {
                case ResultLevelVersionOne.Error:
                    return ResultLevel.Error;
                case ResultLevelVersionOne.Note:
                    return ResultLevel.Note;
                case ResultLevelVersionOne.Pass:
                    return ResultLevel.Pass;
                case ResultLevelVersionOne.Warning:
                    return ResultLevel.Warning;
                case ResultLevelVersionOne.NotApplicable:
                    return ResultLevel.NotApplicable;
                default:
                    return ResultLevel.Default;
            }
        }

        public static ResultLevelVersionOne CreateResultLevelVersionOne(RuleConfigurationDefaultLevel v2DefaultLevel)
        {
            switch (v2DefaultLevel)
            {
                case RuleConfigurationDefaultLevel.Error:
                    return ResultLevelVersionOne.Error;
                case RuleConfigurationDefaultLevel.Note:
                    return ResultLevelVersionOne.Pass;
                case RuleConfigurationDefaultLevel.Warning:
                    return ResultLevelVersionOne.Warning;
                default:
                    return ResultLevelVersionOne.Warning;
            }
        }

        public static ResultLevelVersionOne CreateResultLevelVersionOne(ResultLevel v2ResultLevel)
        {
            switch (v2ResultLevel)
            {
                case ResultLevel.Error:
                    return ResultLevelVersionOne.Error;
                case ResultLevel.Note:
                    return ResultLevelVersionOne.Note;
                case ResultLevel.Pass:
                    return ResultLevelVersionOne.Pass;
                case ResultLevel.Warning:
                    return ResultLevelVersionOne.Warning;
                case ResultLevel.NotApplicable:
                    return ResultLevelVersionOne.NotApplicable;
                default:
                    return ResultLevelVersionOne.Default;
            }
        }

        public static SuppressionStates CreateSuppressionStates(SuppressionStatesVersionOne v1SuppressionStates)
        {
            switch (v1SuppressionStates)
            {
                case SuppressionStatesVersionOne.SuppressedExternally:
                    return SuppressionStates.SuppressedExternally;
                case SuppressionStatesVersionOne.SuppressedInSource:
                    return SuppressionStates.SuppressedInSource;
                default:
                    return SuppressionStates.None;
            }
        }

        public static BaselineState CreateBaselineState(BaselineStateVersionOne v1BaselineState)
        {
            switch (v1BaselineState)
            {
                case BaselineStateVersionOne.Absent:
                    return BaselineState.Absent;
                case BaselineStateVersionOne.Existing:
                    return BaselineState.Existing;
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
                case BaselineState.Existing:
                    return BaselineStateVersionOne.Existing;
                case BaselineState.New:
                    return BaselineStateVersionOne.New;
                default:
                    return BaselineStateVersionOne.None;
            }
        }

        public static CodeFlowLocationImportance CreateCodeFlowLocationImportance(AnnotatedCodeLocationImportanceVersionOne v1AnnotatedCodeLocationImportance)
        {
            switch (v1AnnotatedCodeLocationImportance)
            {
                case AnnotatedCodeLocationImportanceVersionOne.Essential:
                    return CodeFlowLocationImportance.Essential;
                case AnnotatedCodeLocationImportanceVersionOne.Important:
                    return CodeFlowLocationImportance.Important;
                default:
                    return CodeFlowLocationImportance.Important;
            }
        }
    }
}
