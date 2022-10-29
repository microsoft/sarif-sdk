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
            _failureLevels = failureLevels?.ToList() ?? new List<FailureLevel>(new[] { FailureLevel.Error, FailureLevel.Warning }); ;
            _resultKinds = resultKinds?.ToList() ?? new List<ResultKind>(new[] {  ResultKind.Fail });

            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (_resultKinds.Count == 0 || (_resultKinds.Count == 1 && _resultKinds[0] == ResultKind.None))
            {
                throw new ArgumentException("At least one kind is required");
            }

            bool failureLevelsEffectivelyEmpty = _failureLevels == null
                                                    || _failureLevels.Count == 0
                                                    || (_failureLevels.Count == 1 && _failureLevels[0] == FailureLevel.None);

            if (_resultKinds.Contains(ResultKind.Fail))
            {
                if (failureLevelsEffectivelyEmpty)
                {
                    throw new ArgumentException("Failure level required if logging kind 'fail'");
                }
            }
            else
            {
                if (!failureLevelsEffectivelyEmpty)
                {
                    throw new ArgumentException("Failure level must be empty if logging kind does not include 'fail'");
                }
            }
        }

        public bool ShouldLog(Notification notification)
        {
            return _failureLevels.Contains(notification.Level);
        }

        public bool ShouldLog(Result result)
        {
            if (_resultKinds.Contains(result.Kind))
            {
                if (result.Kind == ResultKind.Fail)
                {
                    return _failureLevels.Contains(result.Level);
                }

                return true;
            }

            return false;
        }
    }
}
