// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    public class SarifLogBaselinerFactory
    {
        public static ISarifLogBaseliner CreateSarifLogBaseliner(SarifBaselineType logBaselinerType)
        {
            switch (logBaselinerType)
            {
                case SarifBaselineType.Strict:
                    return new SarifLogBaseliner(Result.ValueComparer);
                case SarifBaselineType.Standard:
                    return new SarifLogBaseliner(DefaultBaseline.ResultBaselineEquals.DefaultInstance);
                default:
                    return new SarifLogBaseliner(Result.ValueComparer);
            }
        }
    }
}
