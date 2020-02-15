// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItems;
using Microsoft.CodeAnalysis.Test.Plugins;
using Xunit;

namespace Test.Plugins
{
    public class ContextGeneratingHelper
    {
        [Fact]
        public void GenerateSampleContextToTestPlugin()
        {
            var sarifWorkItemContext = new SarifWorkItemContext();
            sarifWorkItemContext.ProjectUri = new System.Uri("https://github.com/michaelcfanning/bug-dummy");
            sarifWorkItemContext.SecurityToken = "xxx";
            sarifWorkItemContext.AddWorkItemModelTransformer(new TestWorkItemModelTransformer());
            sarifWorkItemContext.SetProperty(TestWorkItemModelTransformer.AdditionalTags, new StringSet(new[] { "extensionAddedTestTag" }));
            sarifWorkItemContext.SaveToXml(@"e:\repros\AddTagForGithub.xml");

            sarifWorkItemContext.ProjectUri = new System.Uri("https://secretscantest.visualstudio.com/RTAKET-DELME");
            sarifWorkItemContext.SecurityToken = "xxx";
            sarifWorkItemContext.SaveToXml(@"e:\repros\AddTagForAdo.xml");
        }
    }
}
