// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    /// <summary>
    /// Sarif log extension methods in order to allow for ease of use as an API.
    /// </summary>
    public static class SarifLogExtensionMethods
    {
        public static SarifLog Merge(this IEnumerable<SarifLog> sarifLog)
        {
            return ((GenericFoldAction<SarifLog>)SarifLogProcessorFactory.GetActionStage(SarifLogAction.Merge)).Fold(sarifLog);
        }

        public static IEnumerable<SarifLog> RebaseUri(this IEnumerable<SarifLog> sarifLog, string basePathToken, bool rebaseRelativeUris, Uri uri)
        {
            return SarifLogProcessorFactory.GetActionStage(SarifLogAction.RebaseUri, basePathToken, rebaseRelativeUris.ToString(), uri.AbsoluteUri).Act(sarifLog);
        }

        public static SarifLog RebaseUri(this SarifLog sarifLog, string basePathToken, bool rebaseRelativeUris, Uri uri)
        {
            return (new List<SarifLog>() { sarifLog }).RebaseUri(basePathToken, rebaseRelativeUris, uri).Single();
        }

        public static IEnumerable<SarifLog> MakeUrisAbsolute(this IEnumerable<SarifLog> sarifLogs)
        {
            return SarifLogProcessorFactory.GetActionStage(SarifLogAction.MakeUrisAbsolute).Act(sarifLogs);
        }

        public static SarifLog MakeUrisAbsolute(this SarifLog sarifLog)
        {
            return (new List<SarifLog>() { sarifLog }).MakeUrisAbsolute().Single();
        }
    }
}
