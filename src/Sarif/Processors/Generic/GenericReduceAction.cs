// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class GenericReduceAction<T> : IReduceAction<T> where T : new()
    {
        Func<T, T, T> _action;

        public GenericReduceAction(Func<T, T, T> action)
        {
            _action = action;
        }

        public T Reduce(IEnumerable<T> list, T accumulator)
        {
            if(accumulator == null)
            {
                throw new ArgumentNullException(nameof(accumulator));
            }
            if(list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            
            foreach (var entry in list)
            {
                accumulator = _action(accumulator, entry);
            }
            return accumulator;
        }

        public T Reduce(IEnumerable<T> list)
        {
            return Reduce(list, new T());
        }

        // Allows for chaining with later map actions.
        public IEnumerable<T> Act(IEnumerable<T> list)
        {
            return new List<T>() { Reduce(list) };
        }
    }
}
