// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    class TestFilterProcessor : GenericReduceAction<SarifLog>
    {
        public TestFilterProcessor() : base((acc, list) => { return acc; }) { }
    }
}
