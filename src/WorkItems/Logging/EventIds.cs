// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Microsoft.WorkItems.Logging
{
    public static class EventIds
    {
        public static EventId None => new EventId(0, nameof(None));

        public static EventId LogsToProcessMetrics => new EventId(9001, nameof(LogsToProcessMetrics));
        public static EventId WorkItemFiledCoreMetrics => new EventId(9002, nameof(WorkItemFiledCoreMetrics));
        public static EventId WorkItemFiledDetailMetrics => new EventId(9003, nameof(WorkItemFiledDetailMetrics));
    }
}
