// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public abstract class DriverCommand<T> 
    {
        abstract public int Run(T options);

        public const int FAILURE = 1;
        public const int SUCCESS = 0;
    }
}
