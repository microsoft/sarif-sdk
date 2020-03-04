// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class GenericActionPipeline<T> : IActionWrapper<T>
    {
        readonly IEnumerable<IActionWrapper<T>> _stages;

        public GenericActionPipeline(IEnumerable<IActionWrapper<T>> stages)
        {
            _stages = stages;
        }

        public IEnumerable<T> Act(IEnumerable<T> collection)
        {
            IEnumerable<T> intermediate = collection;
            foreach (IActionWrapper<T> action in _stages)
            {
                intermediate = action.Act(intermediate);
            }
            return intermediate;
        }
    }
}
