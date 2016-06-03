// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Serializable]
    public class StringSetCollection : HashSet<string>
    {
        public StringSetCollection() { }

        public StringSetCollection(IEnumerable<string> strings) : base(strings) { }

        protected StringSetCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
