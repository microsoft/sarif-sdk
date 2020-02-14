// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.WorkItems;
using Xunit;

namespace Test.Plugins
{
    public class ContextGeneratingHelper
    {
        [Fact]
        public void GenerateSampleContextToTestPlugin()
        {
            var sarifWorkItemContext = new SarifWorkItemContext();
            //sarifWorkItemContext.AddWorkItemModelTransformer()
        }
    }
}
