// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public static class SarifLogProcessorFactory
    {
        public static IActionWrapper<SarifLog> GetActionStage(SarifLogAction action, params string[] args)
        {
            switch (action)
            {
                case SarifLogAction.None:
                {
                    return new GenericMappingAction<SarifLog>(a => a);
                }
                case SarifLogAction.MakeUrisAbsolute:
                {
                    return new GenericMappingAction<SarifLog>(log =>
                    {
                        MakeUrisAbsoluteVisitor visitor = new MakeUrisAbsoluteVisitor();
                        return visitor.VisitSarifLog(log);
                    });
                }
                case SarifLogAction.InsertOptionalData:
                case SarifLogAction.RemoveOptionalData:
                {
                    return new GenericMappingAction<SarifLog>(log =>
                    {
                        bool optionalDataArgValid = Enum.TryParse(args[0], out OptionallyEmittedData optionalData);
                        Debug.Assert(optionalDataArgValid);

                        if (optionalData != 0)
                        {
                            var visitor = new InsertOptionalDataVisitor(optionalData);
                            return visitor.VisitSarifLog(log);
                        }
                        return log;
                    });
                }
                case SarifLogAction.RebaseUri:
                {
                    return new GenericMappingAction<SarifLog>(log =>
                    {
                        bool rebaseRelativeUrisValid = bool.TryParse(args[1], out bool rebaseRelativeUris);
                        Debug.Assert(rebaseRelativeUrisValid);

                        var visitor = new RebaseUriVisitor(args[0], new Uri(args[2]), rebaseRelativeUris);
                        return visitor.VisitSarifLog(log);
                    });
                }
                case SarifLogAction.Merge:
                {
                    bool mergeEmptyLogsArgValid = bool.TryParse(args.Length == 0 ? "true" : args[0], out bool mergeEmptyLogs);
                    Debug.Assert(mergeEmptyLogsArgValid);

                    return new GenericFoldAction<SarifLog>((accumulator, nextLog) =>
                    {
                        if (nextLog.Runs == null)
                        {
                            return accumulator;
                        }

                        if (accumulator.Runs == null)
                        {
                            accumulator.Runs = new List<Run>();
                        }

                        foreach (Run run in nextLog.Runs)
                        {
                            if (run != null &&
                                (mergeEmptyLogs || run?.Results.Count > 0))
                            {
                                accumulator.Runs.Add(run);
                            }
                        }

                        return accumulator;
                    });
                }
                case SarifLogAction.Sort:
                case SarifLogAction.MakeDeterministic:
                {
                    throw new NotImplementedException();
                }
                default:
                    throw new ArgumentException($"Unknown/Not Supported Action {action}.", nameof(action));
            }
        }
    }
}
