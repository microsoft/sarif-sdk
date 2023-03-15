﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    /// This class caches analysis results for unique files (by hash). Consumers can retrieve and use these cached
    /// results in preference of repeating the analysis. A binary drop point, for example, may contain multiple
    /// copies of a common dependency that has been copied to the output directory of every component that
    /// references it. During analysis, this logger will capture and retain results produced for a single copy
    /// of the file. A consumer can consult this cache and retrieve the results for a file copy, in preference
    /// of simply repeating the analysis. This can result in significant performance gains, when that analysis
    /// is expensive (such as in the case of a binary analysis that retrieves and crawls binary PDBs).
    /// </summary>
    public class CacheByFileHashLogger : BaseLogger, IAnalysisLogger
    {
        private bool cacheLoggingData;
        private string currentFileHash;
        public CacheByFileHashLogger(FailureLevelSet levels, ResultKindSet kinds) : base(levels, kinds)
        {
        }

        public Dictionary<string, List<Notification>> HashToNotificationsMap { get; private set; }

        public Dictionary<string, List<Tuple<ReportingDescriptor, Result, int?>>> HashToResultsMap { get; private set; }

        public FileRegionsCache FileRegionsCache { get; set; }


        public void AnalysisStarted()
        {
            HashToNotificationsMap = new Dictionary<string, List<Notification>>();
            HashToResultsMap = new Dictionary<string, List<Tuple<ReportingDescriptor, Result, int?>>>();
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            this.currentFileHash = context.Hashes.Sha256;

            if (HashToResultsMap.ContainsKey(currentFileHash))
            {
                // We already have results cached for this file. This means that we shouldn't collect
                // any data in calls to Log. These calls are the driver framework 'replaying' the
                // cached results via the aggregated logger instance
                cacheLoggingData = false;
            }
            else
            {
                cacheLoggingData = true;
                HashToNotificationsMap[currentFileHash] = new List<Notification>();
                HashToResultsMap[currentFileHash] = new List<Tuple<ReportingDescriptor, Result, int?>>();
            }
        }

        public void TargetAnalyzed(IAnalysisContext _) { }

        public void Log(ReportingDescriptor rule, Result result, int? extensionIndex = null)
        {
            if (!cacheLoggingData) { return; }

            if (!ShouldLog(result))
            {
                return;
            }

            CacheResult(rule, result, extensionIndex);
        }

        private void CacheResult(ReportingDescriptor rule, Result result, int? extensionIndex)
        {
            if (!HashToResultsMap.TryGetValue(currentFileHash, out List<Tuple<ReportingDescriptor, Result, int?>> results))
            {
                results = HashToResultsMap[currentFileHash] = new List<Tuple<ReportingDescriptor, Result, int?>>();
            }
            results.Add(new Tuple<ReportingDescriptor, Result, int?>(rule, result, extensionIndex));
        }

        public void LogConfigurationNotification(Notification notification)
        {
            if (!cacheLoggingData || !ShouldLog(notification)) { return; }

            if (!HashToNotificationsMap.TryGetValue(currentFileHash, out List<Notification> notifications))
            {
                notifications = HashToNotificationsMap[currentFileHash] = new List<Notification>();
            }
            notifications.Add(notification);
        }

        public void LogToolNotification(Notification notification, ReportingDescriptor associatedRule)
        {
            //  In case someone adds something here:
            if (!ShouldLog(notification))
            {
                return;
            }
            //  Add actual functionality below
        }
    }
}
