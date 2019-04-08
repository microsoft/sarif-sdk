// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Flags]
    internal enum TestRuleBehaviors
    {
        None = 0,
        LogError = 0x1
    }
}