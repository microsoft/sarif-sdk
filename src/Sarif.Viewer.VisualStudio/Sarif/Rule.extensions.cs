// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    Name = rule.Name,
                    Category = rule.GetCategory(),
                    Severity = rule.DefaultLevel.ToString(),
                    Description = rule.FullDescription,
                    HelpUri = rule.HelpUri?.AbsoluteUri,

                    // TODO: Replace with real values
                    //Version = "0.0.0",
                    //OwnerName = "John Doe",
                    //OwnerUri = "mailto:johndoe@sarif.microsoft.com",
                    //FeedbackUri = "mailto:toolfeedback@sarif.microsoft.com",
                };
            }

            return model;
        }
    }
}
