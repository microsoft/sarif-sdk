// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class ResultLevelExtensions
    {
        public static string FormatForVisualStudio(this ResultLevel level)
        {
            switch (level)
            {
                case ResultLevel.Error:
                    return "error";

                case ResultLevel.Warning:
                    return "warning";

                default:
                    return "info";
            }
        }
    }
}
