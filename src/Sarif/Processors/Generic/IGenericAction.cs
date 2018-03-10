﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public interface IGenericAction<T>
    {
        /// <summary>
        /// Wraps either map or fold functionality, in order to allow chaining of map -> fold -> map if needed.
        /// </summary>
        IEnumerable<T> Act(IEnumerable<T> list);
    }
}
