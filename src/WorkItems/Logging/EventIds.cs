// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.WorkItems.Logging
{
    public static class EventIds
    {
        public static EventId AssemblyVersion => new EventId(1001, nameof(AssemblyVersion));

        public static EventId LogsToProcessMetrics => new EventId(8001, nameof(LogsToProcessMetrics));

        // EventIds 9000-9009 are reserved for work item filing status
        public static EventId WorkItemFiledCoreMetrics => new EventId(9001, nameof(WorkItemFiledCoreMetrics));
        public static EventId WorkItemCanceledCoreMetrics => new EventId(9002, nameof(WorkItemCanceledCoreMetrics));
        public static EventId WorkItemExceptionCoreMetrics => new EventId(9003, nameof(WorkItemExceptionCoreMetrics));

        public static EventId WorkItemFiledDetailMetrics => new EventId(9010, nameof(WorkItemFiledDetailMetrics));
    }
}
