// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 
using System;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    class TestFoldProcessor : GenericFoldAction<int>
    {
        public static Func<int, int, int> internalFunction = (acc, value) => { return acc + value; };

        public TestFoldProcessor() : base(internalFunction) { }
    }
}
