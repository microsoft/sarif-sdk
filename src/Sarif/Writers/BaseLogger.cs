// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public abstract class BaseLogger
    {
        protected readonly List<FailureLevel> _failureLevels;
        protected readonly List<ResultKind> _resultKinds;

        protected BaseLogger(IEnumerable<FailureLevel> failureLevels,
            IEnumerable<ResultKind> resultKinds)
        {
            if (failureLevels == null || failureLevels.Count() == 0)
            {
                _failureLevels = new List<FailureLevel> { FailureLevel.Error, FailureLevel.Warning };
            }
            else
            {
                _failureLevels = failureLevels.ToList();
            }

            if (resultKinds == null || resultKinds.Count() == 0)
            {
                _resultKinds = new List<ResultKind> { ResultKind.Fail };
            }
            else
            {
                _resultKinds = resultKinds.ToList();
            }

            ValidateParameters();
        }

        private void ValidateParameters()
        {
            //  If we got here, both resultKinds and failurelevels can neither be null nor empty.
            //  If resultKinds does not include "fail" then failureLevels MUST be none/zero
            if (!_resultKinds.Contains(ResultKind.Fail))
            {
                if (_failureLevels.Count > 1 || _failureLevels[0] != FailureLevel.None)
                {
                    throw new ArgumentException("Invalid kind & level combination");
                }
            }
        }

        public bool ShouldLog(Notification notification)
        {
            return _failureLevels.Contains(notification.Level);
        }

        public bool ShouldLog(Result result)
        {
            // If resultKinds is the default value (Fail), we should just filter based on failureLevels
            if (_resultKinds.Count == 1 && _resultKinds[0] == ResultKind.Fail)
            {
                return _failureLevels.Contains(result.Level);
            }

            return _resultKinds.Contains(result.Kind) && _failureLevels.Contains(result.Level);
        }
    }
}
