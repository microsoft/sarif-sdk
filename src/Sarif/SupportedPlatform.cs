// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Flags]
    public enum SupportedPlatform
    {
        Unknown = 0,
        Windows = 1,
        Linux = 2,
        OSX = 4,
        All = Windows | Linux | OSX
    }
}
