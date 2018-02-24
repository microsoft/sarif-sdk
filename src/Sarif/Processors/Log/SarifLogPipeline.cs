// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class ActionTuple
    {
        public SarifLogAction Action;
        public string[] Parameters;
    }

    /// <summary>
    /// Serializable log manipulation pipeline--takes a series of stages, and then executes them sequentially on a log file.
    /// </summary>
    [Serializable]
    public class SarifLogPipeline
    {
        [JsonRequired]
        public List<ActionTuple> Actions
        {
            get;
            private set;
        }
        
        private GenericActionPipeline<SarifLog> _pipeline;

        [JsonConstructor]
        public SarifLogPipeline(List<ActionTuple> actions)
        {
            this.Actions = actions;
            
            _pipeline = new GenericActionPipeline<SarifLog>(Actions.Select(a => SarifLogProcessorFactory.GetActionStage(a.Action, a.Parameters)));
        }

        public IEnumerable<SarifLog> ApplyPipeline(IEnumerable<SarifLog> logs)
        {
            return _pipeline.Act(logs);
        }
    }
}
