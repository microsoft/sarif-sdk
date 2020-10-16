// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.WorkItems.Logging
{
    public static class AssemblyExtensions
    {
        public static void LogIdentity(this Assembly assembly, EventId customEventId = default(EventId), IDictionary<string, object> customDimensions = null)
        {
            ILogger logger = ServiceProviderFactory.ServiceProvider.GetService<ILogger>();

            EventId eventId = (customEventId == default(EventId)) ? EventIds.AssemblyVersion : customEventId;
            customDimensions ??= new Dictionary<string, object>();

            AssemblyName assemblyName = assembly.GetName();
            FileInfo assemblyFileInfo = new FileInfo(assembly.Location);
            Dictionary<string, object> assemblyMetrics = new Dictionary<string, object>(customDimensions);
            assemblyMetrics.Add("Name", assemblyName.Name);
            assemblyMetrics.Add("Version", assemblyName.Version);
            assemblyMetrics.Add("CreationTime", assemblyFileInfo.CreationTime.ToUniversalTime().ToString());

            logger.LogMetrics(eventId, assemblyMetrics);
        }
    }
}
