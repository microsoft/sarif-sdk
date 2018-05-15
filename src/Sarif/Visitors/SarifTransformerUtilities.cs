// Copyright(c) Microsoft.All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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

        public static IList<TTo> TransformList<TFrom, TTo>(IList<TFrom> fromList, Func<TFrom, TTo> convertFunc)
        {
            List<TTo> result = null;

            if (fromList != null && fromList.Count > 0)
            {
                result = new List<TTo>();

                foreach (TFrom fromObject in fromList)
                {
                    result.Add(convertFunc(fromObject));
                }
            }

            return result;
        }

        public static void RemoveSarifPropertyBagItems(PropertyBagHolder holder, SarifVersion version)
        {
            if (holder.Properties != null)
            {
                string prefix = PropertyBagTransformerItemPrefixes[version];
                holder.PropertyNames.Where(n => n.StartsWith(prefix))
                                    .Select(p => p)
                                    .ToList()
                                    .ForEach(k => holder.RemoveProperty(k));
            }
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
    }
}
