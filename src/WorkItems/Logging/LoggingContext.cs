// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Microsoft.CodeAnalysis.WorkItems.Logging
{
    public class LoggingContext : IDisposable
    {
        public LoggingContext(ILogger logger, string scopeName)
        {
            this.Loggger = logger;
            this.ScopeName = scopeName;
            this.Scope = this.Loggger.BeginScope(this.ScopeName);

            this.Loggger.LogDebug($"Begin scope - {this.ScopeName}");
        }

        public ILogger Loggger
        {
            get;
        }

        private IDisposable Scope
        {
            get;
        }

        private string ScopeName
        {
            get;
        }

        public void Dispose()
        {
            this.Loggger?.LogDebug($"End scope - {this.ScopeName}");

            if (this.Scope != null)
            {
                this.Scope.Dispose();
            }
        }
    }
}
