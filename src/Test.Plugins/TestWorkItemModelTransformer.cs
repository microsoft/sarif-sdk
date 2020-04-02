// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.WorkItems;

namespace Microsoft.CodeAnalysis.Test.Plugins
{
    public class TestWorkItemModelTransformer : SarifWorkItemModelTransformer
    {
        public override SarifWorkItemModel Transform(SarifWorkItemModel workItemModel)
        {
            workItemModel.LabelsOrTags = workItemModel.LabelsOrTags ?? new List<string>();

            foreach (string additionalTag in workItemModel.Context.GetProperty(AdditionalTags))
            {
                workItemModel.LabelsOrTags.Add(additionalTag);
            }

            return workItemModel;
        }

        internal static PerLanguageOption<StringSet> AdditionalTags { get; } =
            new PerLanguageOption<StringSet>(
                nameof(TestWorkItemModelTransformer), nameof(AdditionalTags),
                defaultValue: () => { return new StringSet(); });
    }
}
