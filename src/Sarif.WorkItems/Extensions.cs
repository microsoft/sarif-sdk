// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.WorkItems
{
    public static class Extensions
    {
        public static SarifWorkItemModel CreateWorkItemModel(
            this SarifLog sarifLog, 
            SarifWorkItemContext rootContext = null)
        {
            var sarifWorkItemModel = new SarifWorkItemModel();
            sarifWorkItemModel.InitializeFromLog(sarifLog, rootContext);

            return sarifWorkItemModel;
        }
    }
}
