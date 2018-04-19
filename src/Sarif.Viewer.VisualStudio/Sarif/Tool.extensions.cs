// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class ToolExtensions
    {
        public static ToolModel ToToolModel(this Tool tool)
        {
            if (tool == null)
            {
                return null;
            }

            ToolModel model = new ToolModel()
            {
                Name = tool.Name,
                FullName = tool.FullName,
                Version = tool.Version,
                SemanticVersion = tool.SemanticVersion,
            };

            return model;
        }
    }
}
