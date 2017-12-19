// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Sarif.Viewer
{
    /// <summary>
    /// Telemetry event types
    /// </summary>
    internal enum TelemetryEvent
    {
        ViewerExtensionLoaded = 1,
        LogFileOpenedByMenuCommand = 2,
        LogFileOpenedByEditor = 3,
        TaskItemDocumentOpened = 4,
        LogFileRunCreatedByToolName = 5
    }
}
