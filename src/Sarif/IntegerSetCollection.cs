// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Serializable]
    public class IntegerSetCollection : HashSet<int>
    {
        public IntegerSetCollection() { }

        public IntegerSetCollection(IEnumerable<int> values) : base(values) { }

        protected IntegerSetCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
