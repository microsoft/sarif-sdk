using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.WorkItems
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
            if (string.IsNullOrEmpty(telemetry.Context.Operation.Id))
            {
                telemetry.Context.Operation.Id = m_operationId;
            }
        }
    }
}
