// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Cli.Rules
{
    public class HashAlgorithmsMustBeUnique : SarifValidationSkimmerBase
    {
        public override string FullDescription => RuleResources.SV0006_HashAlgorithmsMustBeUnique;

        public override ResultLevel DefaultLevel => ResultLevel.Error;

        /// <summary>
        /// SV0005
        /// </summary>
        public override string Id => RuleId.HashAlgorithmsMustBeUnique;

        protected override IEnumerable<string> FormatIds
        {
            get
            {
                return new string[]
                {
                    nameof(RuleResources.SV0006_Default)
                };
            }
        }

        protected override void Analyze(FileData fileData, string fileKey, string filePointer)
        {
            if (fileData.Hashes != null)
            {
                foreach (AlgorithmKind algorithmKind in fileData.Hashes.Select(h => h.Algorithm).Distinct())
                {
                    if (fileData.Hashes.Count(h => h.Algorithm == algorithmKind) > 1)
                    {
                        string hashesPointer = filePointer.AtProperty(SarifPropertyName.Hashes);

                        LogResult(
                            hashesPointer,
                            nameof(RuleResources.SV0006_Default),
                            algorithmKind.ToString());
                    }
                }
            }
        }
    }
}
