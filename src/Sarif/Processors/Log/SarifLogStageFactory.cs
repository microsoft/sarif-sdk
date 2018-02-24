// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class SarifLogProcessorFactory
    {
        public static IGenericAction<SarifLog> GetActionStage(SarifLogAction action, params string[] args)
        {
            switch (action)
            {
                case SarifLogAction.None:
                    return new GenericMappingAction<SarifLog>(a => a);
                case SarifLogAction.RebaseUri:
                    return new GenericMappingAction<SarifLog>(log =>
                    {
                        RebaseUriVisitor visitor = new RebaseUriVisitor(args[0], new Uri(args[1]));
                        return visitor.VisitSarifLog(log);
                    });
                case SarifLogAction.Merge:
                    return new GenericReduceAction<SarifLog>(mergeFunction);
                case SarifLogAction.Sort:
                    throw new NotImplementedException("Future work, not yet supported.");
                case SarifLogAction.MakeDeterministic:
                    throw new NotImplementedException("Future work, not yet supported");
                default:
                    throw new ArgumentException($"Unknown/Not Supported Action {action}.", nameof(action));
            }
        }
        
        private static Func<SarifLog, SarifLog, SarifLog> mergeFunction =
            (accumulator, nextLog) =>
            {
                if (nextLog.Runs == null)
                {
                    return accumulator;
                }

                foreach (var run in nextLog.Runs)
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
