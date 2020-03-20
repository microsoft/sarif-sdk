using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WorkItems.Logging
{
    public static class EventIds
    {
        public static EventId LogsToProcessMetrics => new EventId(9001, nameof(LogsToProcessMetrics));
    }
}
