// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class CodeFlowLocationExtensions
    {
        public static CodeFlowLocationModel ToCodeFlowLocationModel(this CodeFlowLocation codeFlowLocation)
        {
            var model = codeFlowLocation.Location != null
                ? codeFlowLocation.Location.ToCodeFlowLocationModel()
                : new CodeFlowLocationModel();

            model.IsEssential = codeFlowLocation.Importance == CodeFlowLocationImportance.Essential;

            return model;
        }
    }
}
