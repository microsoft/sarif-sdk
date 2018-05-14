// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class RuleExtensions
    {
        public static RuleModel ToRuleModel(this IRule rule, string defaultRuleId)
        {
            RuleModel model;

            if (rule == null)
            {
                model = new RuleModel()
                {
                    Id = defaultRuleId,
                };
            }
            else
            {
                model = new RuleModel()
                {
                    Id = rule.Id,
                    Name = rule.Name?.Text,
                    Category = rule.GetCategory(),
                    Description = rule.FullDescription?.Text,
                    HelpUri = rule.HelpLocation?.Uri.AbsoluteUri
                };
            }

            return model;
        }
    }
}
