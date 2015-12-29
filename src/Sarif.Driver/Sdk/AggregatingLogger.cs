// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public class AggregatingLogger : IDisposable, IResultLogger
    {
        public AggregatingLogger() : this(null)
        {
        }

        public AggregatingLogger(IEnumerable<IResultLogger> loggers)
        {
            this.Loggers = loggers != null ?
                new List<IResultLogger>(loggers) :
                new List<IResultLogger>();
        }

        public IList<IResultLogger> Loggers { get; set; }

        public void Dispose()
        {
            foreach (IResultLogger logger in Loggers)
            {
                using (logger as IDisposable) { };
            }
        }

        public void Log(ResultKind messageKind, IAnalysisContext context, Region region, string formatSpecifierId, params string[] arguments)
        {
            foreach (IResultLogger logger in Loggers)
            {
                logger.Log(messageKind, context, region, formatSpecifierId, arguments);
            }
        }
    }
}
