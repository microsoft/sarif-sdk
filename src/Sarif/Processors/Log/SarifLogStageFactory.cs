// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class SarifLogProcessorFactory
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
                case SarifLogAction.RebaseUri:
                {
                    return new GenericMappingAction<SarifLog>(log =>
                    {
                        bool castRelativeUrisArg = bool.TryParse(args[1], out bool rebaseRelativeUris);
                        Debug.Assert(castRelativeUrisArg);

                        RebaseUriVisitor visitor = new RebaseUriVisitor(args[0], new Uri(args[2]), rebaseRelativeUris);
                        return visitor.VisitSarifLog(log);
                    });
                }
                case SarifLogAction.Merge:
                {
                    return new GenericFoldAction<SarifLog>(mergeFunction);
                }
                case SarifLogAction.Sort:
                {
                    throw new NotImplementedException();
                }
                case SarifLogAction.MakeDeterministic:
                {
                    throw new NotImplementedException();
                }
                default:
                    throw new ArgumentException($"Unknown/Not Supported Action {action}.", nameof(action));
            }
        }

        private readonly static Func<SarifLog, SarifLog, SarifLog> mergeFunction =
            (accumulator, nextLog) =>
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
                    if (run != null)
                    {
                        accumulator.Runs.Add(run);
                    }
                }

                return accumulator;
            };
    }
}
