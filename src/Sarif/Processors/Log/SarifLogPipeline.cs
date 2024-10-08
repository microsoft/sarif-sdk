﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    /// <summary>
    /// Serializable log manipulation pipeline--takes a series of stages, and then executes them sequentially on a log file.
    /// </summary>
    [Serializable]
    public class SarifLogPipeline
    {
#if NETSTANDARD
        [JsonRequired]
#endif
        public List<SarifLogActionTuple> Actions
        {
            get;
            private set;
        }

        private readonly GenericActionPipeline<SarifLog> _pipeline;

        [JsonConstructor]
        public SarifLogPipeline(List<SarifLogActionTuple> actions)
        {
            this.Actions = actions;

            _pipeline = new GenericActionPipeline<SarifLog>(Actions.Select(a => SarifLogProcessorFactory.GetActionStage(a.Action, a.Parameters)));
        }

        public IEnumerable<SarifLog> ApplyPipeline(IEnumerable<SarifLog> logs)
        {
            return _pipeline.Act(logs);
        }

        /// <summary>
        /// Two pipelines are equal if they apply the same steps to the input sarif files.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is SarifLogPipeline other))
            {
                return base.Equals(obj);
            }

            if (other.Actions.Count != this.Actions.Count)
            {
                return false;
            }

            for (int i = 0; i < this.Actions.Count; i++)
            {
                if (!this.Actions[i].Equals(other.Actions[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generated by Visual Studio.  We're overriding Equals() so we need to override this as well.
        /// </summary>
        public override int GetHashCode()
        {
            return 1522684324 + EqualityComparer<List<SarifLogActionTuple>>.Default.GetHashCode(Actions);
        }
    }
}
