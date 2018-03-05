// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public interface IFoldAction<T> : IGenericAction<T>
    {
        /// <summary>
        /// Take an action on each sarif log, return the accumulated result.
        /// </summary>
        /// <param name="sarifLogs"></param>
        /// <param name="accumulator"></param>
        /// <returns></returns>
        T Fold(IEnumerable<T> list, T accumulator);
    }
}
