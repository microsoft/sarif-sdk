﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    [Serializable]
    [JsonConverter(typeof(TypedPropertiesDictionaryConverter))]
    public class IntegerSet : HashSet<int>
    {
        public IntegerSet() { }

        public IntegerSet(IEnumerable<int> values) : base(values) { }

        protected IntegerSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
