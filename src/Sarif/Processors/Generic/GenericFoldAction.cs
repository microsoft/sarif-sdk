// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class GenericFoldAction<T> : IFoldAction<T> where T : new()
    {
        Func<T, T, T> _action;

        public GenericFoldAction(Func<T, T, T> action)
        {
            _action = action;
        }

        public T Fold(IEnumerable<T> list, T accumulator)
        {
            if (accumulator == null)
            {
                throw new ArgumentNullException(nameof(accumulator));
            }
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            
            foreach (var entry in list)
            {
                accumulator = _action(accumulator, entry);
            }
            return accumulator;
        }

        public T Fold(IEnumerable<T> list)
        {
            return Fold(list, new T());
        }

        // Allows for chaining with later map actions.
        public IEnumerable<T> Act(IEnumerable<T> list)
        {
            return new List<T>() { Fold(list) };
        }
    }
}
