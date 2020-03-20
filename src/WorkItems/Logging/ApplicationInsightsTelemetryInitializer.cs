using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;

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
