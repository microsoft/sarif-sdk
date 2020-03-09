// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    internal class TestMappingProcessor : GenericMappingAction<int>
    {
        public static Func<int, int> internalFunction = a => { return a + 1; };

        public TestMappingProcessor() : base(internalFunction) { }
    }
}
