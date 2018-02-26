// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public static class SarifLogExtensionMethods
    {
        public static SarifLog Merge(this IEnumerable<SarifLog> sarifLog)
        {
            return ((GenericFoldAction<SarifLog>)SarifLogProcessorFactory.GetActionStage(SarifLogAction.Merge)).Fold(sarifLog);
        }

        public static IEnumerable<SarifLog> RebaseUri(this IEnumerable<SarifLog> sarifLog, string baseName, Uri uri)
        {
            return SarifLogProcessorFactory.GetActionStage(SarifLogAction.RebaseUri, baseName, uri.AbsoluteUri).Act(sarifLog);
        }

        public static SarifLog RebaseUri(this SarifLog sarifLog, string baseName, Uri uri)
        {
            return (new List<SarifLog>() { sarifLog }).RebaseUri(baseName, uri).Single();
        }
    }
}
