// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class GenericMappingAction<T> : IMapAction<T>
    {
        public Func<T, T> Action;

        public GenericMappingAction(Func<T, T> action)
        {
            Action = action;
        }

        public IEnumerable<T> Map(IEnumerable<T> collection)
        {
            List<T> output = new List<T>();
            foreach (T value in collection)
            {
                output.Add(Action.Invoke(value));
            }
            return output;
        }

        public IEnumerable<T> Act(IEnumerable<T> list)
        {
            return Map(list);
        }
    }
}
