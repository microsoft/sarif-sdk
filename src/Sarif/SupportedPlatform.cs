// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Flags]
    public enum SupportedPlatform
    {
        Unknown = 0x0,
        Windows = 0x1,
        Linux = 0x2,
        OSX = 0x4,
        All = Windows | Linux | OSX
    }
}
