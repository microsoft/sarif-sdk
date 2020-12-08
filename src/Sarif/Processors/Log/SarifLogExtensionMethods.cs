// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    /// <summary>
    /// Sarif log extension methods in order to allow for ease of use as an API.
    /// </summary>
    public static class SarifLogExtensionMethods
    {
        public static SarifLog Merge(this IEnumerable<SarifLog> sarifLog, bool mergeEmptyLogs = true)
        {
            return ((GenericFoldAction<SarifLog>)SarifLogProcessorFactory.GetActionStage(SarifLogAction.Merge, mergeEmptyLogs.ToString())).Fold(sarifLog);
        }

        public static IEnumerable<SarifLog> RebaseUri(this IEnumerable<SarifLog> sarifLog, string basePathToken, bool rebaseRelativeUris, Uri uri)
        {
            return SarifLogProcessorFactory.GetActionStage(SarifLogAction.RebaseUri, basePathToken, rebaseRelativeUris.ToString(), uri.AbsoluteUri).Act(sarifLog);
        }

        public static SarifLog RebaseUri(this SarifLog sarifLog, string basePathToken, bool rebaseRelativeUris, Uri uri)
        {
            return new List<SarifLog>() { sarifLog }.RebaseUri(basePathToken, rebaseRelativeUris, uri).Single();
        }

        public static IEnumerable<SarifLog> MakeUrisAbsolute(this IEnumerable<SarifLog> sarifLogs)
        {
            return SarifLogProcessorFactory.GetActionStage(SarifLogAction.MakeUrisAbsolute).Act(sarifLogs);
        }

        public static SarifLog MakeUrisAbsolute(this SarifLog sarifLog)
        {
            return new List<SarifLog>() { sarifLog }.MakeUrisAbsolute().Single();
        }

        public static IEnumerable<SarifLog> RemoveOptionalData(this IEnumerable<SarifLog> sarifLogs, OptionallyEmittedData optionalData)
        {
            return SarifLogProcessorFactory.GetActionStage(SarifLogAction.RemoveOptionalData, optionalData.ToString()).Act(sarifLogs);
        }

        public static SarifLog RemoveOptionalData(this SarifLog sarifLog, OptionallyEmittedData optionalData)
        {
            return new List<SarifLog>() { sarifLog }.RemoveOptionalData(optionalData).Single();
        }

        public static IEnumerable<SarifLog> InsertOptionalData(this IEnumerable<SarifLog> sarifLogs, OptionallyEmittedData optionalData)
        {
            return SarifLogProcessorFactory.GetActionStage(SarifLogAction.InsertOptionalData, optionalData.ToString()).Act(sarifLogs);
        }

        public static SarifLog InsertOptionalData(this SarifLog sarifLog, OptionallyEmittedData optionalData)
        {
            return new List<SarifLog>() { sarifLog }.InsertOptionalData(optionalData).Single();
        }
    }
}
