// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public abstract class DriverCommand<T> : CommandBase
    {
        abstract public int Run(T options);
    }
}
