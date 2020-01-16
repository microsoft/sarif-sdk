// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.Sarif;

namespace SarifBaseline.Extensions
{
    public enum BaselineFilteringMode
    {
        None = 0,
        ToIncludedArtifacts
    }

    public static class SarifLogExtensions
    {
        public const string BaselineFilteringPropertyName = "BaselineFiltering";

        public static BaselineFilteringMode GetBaselineFilteringMode(this SarifLog log)
        {
            if (log == null) { throw new ArgumentNullException("log"); }

            BaselineFilteringMode mode;

            if (log.TryGetProperty<string>(SarifLogExtensions.BaselineFilteringPropertyName, out string modeString))
            {
                if (Enum.TryParse<BaselineFilteringMode>(modeString, out mode))
                {
                    return mode;
                }
                else
                {
                    throw new NotImplementedException($"BaselineFilteringMode '{modeString}' unknown.");
                }
            }

            return BaselineFilteringMode.None;
        }

        public static void SetBaselineFilteringMode(this SarifLog log, BaselineFilteringMode mode)
        {
            if (log == null) { throw new ArgumentNullException("log"); }
            log.SetProperty<string>(SarifLogExtensions.BaselineFilteringPropertyName, mode.ToString());
        }

        public static IEnumerable<Run> EnumerateRuns(this SarifLog log)
        {
            if (log?.Runs != null)
            {
                foreach (Run run in log.Runs)
                {
                    yield return run;
                }
            }
        }

        public static Result FindByGuid(this SarifLog log, string guid)
        {
            return log.EnumerateResults().Where(result => result.CorrelationGuid == guid || result.Guid == guid).FirstOrDefault();
        }
    }
}
