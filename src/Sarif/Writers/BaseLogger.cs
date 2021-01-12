// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public class BaseLogger
    {
        private readonly List<FailureLevel> failureLevels;
        private readonly List<ResultKind> resultKinds;

        public BaseLogger(IEnumerable<FailureLevel> failureLevels,
            IEnumerable<ResultKind> resultKinds)
        {
            this.failureLevels = failureLevels?.ToList() ?? new List<FailureLevel> { FailureLevel.Error, FailureLevel.Warning };
            this.resultKinds = resultKinds?.ToList() ?? new List<ResultKind> { ResultKind.Fail };

            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (resultKinds.Any(kind => kind != ResultKind.Fail))
            {
                if (failureLevels.Contains(FailureLevel.Error))
                {
                    throw new ArgumentException("Invalid kind & level combination");
                }
            }
        }

        public bool ShouldLog(Notification notification)
        {
            return failureLevels.Contains(notification.Level);
        }

        public bool ShouldLog(Result result)
        {
            // If resultKinds is the default value (Fail), we should just filter based on failureLevels
            if (resultKinds.Count == 1 && resultKinds[0] == ResultKind.Fail)
            {
                return failureLevels.Contains(result.Level);
            }

            return resultKinds.Contains(result.Kind) && failureLevels.Contains(result.Level);
        }
    }
}
