// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class GenericFoldAction<T> : IFoldAction<T> where T : new()
    {
        readonly Func<T, T, T> _action;

        public GenericFoldAction(Func<T, T, T> action)
        {
            _action = action;
        }

        public T Fold(IEnumerable<T> collection, T accumulator)
        {
            if (accumulator == null)
            {
                throw new ArgumentNullException(nameof(accumulator));
            }
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            foreach (T entry in collection)
            {
                accumulator = _action(accumulator, entry);
            }
            return accumulator;
        }

        public T Fold(IEnumerable<T> collection)
        {
            return Fold(collection, new T());
        }

        // Allows for chaining with later map actions.
        public IEnumerable<T> Act(IEnumerable<T> collection)
        {
            return new List<T>() { Fold(collection) };
        }
    }
}
