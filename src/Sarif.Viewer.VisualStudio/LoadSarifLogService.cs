// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.Sarif.Viewer.ErrorList; 

namespace Microsoft.Sarif.Viewer
{
    public class LoadSarifLogService : SLoadSarifLogService, ILoadSarifLogService
    {
        public void LoadSarifLog(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    ErrorListService.ProcessLogFile(path, SarifViewerPackage.Dte.Solution, ToolFormat.None);
                }
                catch (InvalidCastException) { }
            }
        }
    }
}
