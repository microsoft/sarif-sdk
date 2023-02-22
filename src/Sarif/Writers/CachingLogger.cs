﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    /// This class caches all results and notifications that are passed to it.
    /// Data cached this way can subsequently be replayed into other IAnalysisLogger
    /// instances. The driver framework uses this mechanism to merge results
    /// produced by a multi-threaded analysis into a single output file.
    /// </summary>
    public class CachingLogger : BaseLogger, IAnalysisLogger
    {
        public CachingLogger(IImmutableSet<FailureLevel> levels, IImmutableSet<ResultKind> kinds) : base(levels, kinds)
        {
            // This reader lock is used to ensure only a single writer until
            // logging is complete, after which all threads can read Results.
            _semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        }

        public IDictionary<ReportingDescriptor, IList<Tuple<Result, int?>>> Results { get; set; }

        public IList<Notification> ConfigurationNotifications { get; set; }

        public IList<Tuple<Notification, ReportingDescriptor>> ToolNotifications { get; set; }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the Results
        /// object associated with this logger is fixed and ready to replay.
        /// </summary>
        public bool CacheFinalized { get; private set; }

        private readonly SemaphoreSlim _semaphore;

        public void AnalysisStarted()
        {
        }

        public void AnalysisStopped(RuntimeConditions runtimeConditions)
        {
        }

        public void AnalyzingTarget(IAnalysisContext context)
        {
            _semaphore.Wait();
        }

        public void Log(ReportingDescriptor rule, Result result, int? extensionIndex)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!ShouldLog(result))
            {
                return;
            }

            if (rule.GetType().Name != nameof(ReportingDescriptor))
            {
                rule = rule.DeepClone();
            }

            if (!result.RuleId.IsEqualToOrHierarchicalDescendantOf(rule.Id))
            {
                throw new ArgumentException($"rule.Id is not equal to result.RuleId ({rule.Id} != {result.RuleId})");
            }

            Results ??= new Dictionary<ReportingDescriptor, IList<Tuple<Result, int?>>>();

            if (!Results.TryGetValue(rule, out IList<Tuple<Result, int?>> results))
            {
                results = Results[rule] = new List<Tuple<Result, int?>>();
            }
            results.Add(new Tuple<Result, int?>(result, extensionIndex));
        }

        public void LogConfigurationNotification(Notification notification)
        {
            if (!ShouldLog(notification))
            {
                return;
            }

            ConfigurationNotifications ??= new List<Notification>();
            ConfigurationNotifications.Add(notification);
        }

        public void LogToolNotification(Notification notification, ReportingDescriptor associatedRule)
        {
            if (!ShouldLog(notification))
            {
                return;
            }

            ToolNotifications ??= new List<Tuple<Notification, ReportingDescriptor>>();
            ToolNotifications.Add(new Tuple<Notification, ReportingDescriptor>(notification, associatedRule));
        }

        public void ReleaseLock()
        {
            CacheFinalized = true;
            _semaphore.Release();
        }
    }
}
