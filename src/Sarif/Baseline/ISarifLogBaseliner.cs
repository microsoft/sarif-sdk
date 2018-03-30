// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Baseline
{
    public interface ISarifLogBaseliner
    {
        Run CreateBaselinedRun(Run baseLine, Run nextLog);
    }
}
