// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{ 
    public class GenericActionPipeline<T> : IGenericAction<T>
    {
        IEnumerable<IGenericAction<T>> _stages;

        public GenericActionPipeline(IEnumerable<IGenericAction<T>> stages)
        {
            _stages = stages;
        }

        public IEnumerable<T> Act(IEnumerable<T> list)
        {
            IEnumerable<T> intermediate = list;
            foreach(var action in _stages)
            {
                intermediate = action.Act(intermediate);
            }
            return intermediate;
        }
    }
}
