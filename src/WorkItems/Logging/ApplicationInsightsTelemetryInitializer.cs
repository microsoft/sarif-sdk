// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.WorkItems.Logging
{
    /// <summary>
    /// Assigns a GUID as the operation_Id for the Application Insights traces.
    /// </summary>
    internal class ApplicationInsightsTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string m_operationId;

        public ApplicationInsightsTelemetryInitializer()
        {
            m_operationId = Guid.NewGuid().ToString();
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry?.Context?.Operation == null)
            {
                throw new ArgumentNullException(nameof(telemetry));
            }

            if (string.IsNullOrEmpty(telemetry.Context.Operation.Id))
            {
                telemetry.Context.Operation.Id = m_operationId;
            }
        }
    }
}
