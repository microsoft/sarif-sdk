// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.WorkItems;

namespace Microsoft.CodeAnalysis.Test.Plugins
{
    public class TestWorkItemModelTransformer : SarifWorkItemModelTransformer
    {
        public void Transform(WorkItemModel<TestWorkItemModelContext> workItemModel)
        {
            workItemModel.Tags = workItemModel.Tags ?? new List<string>();

            foreach (string additionalTag in workItemModel.Context.AdditionalTags)
            {
                workItemModel.Tags.Add(additionalTag);
            }
        }

        internal static PerLanguageOption<StringSet> AdditionalTagsOption { get; } =
            new PerLanguageOption<StringSet>(
                "ExtensibilityTests", nameof(AdditionalTags),
                defaultValue: () => { return new StringSet(); });
    }
}
