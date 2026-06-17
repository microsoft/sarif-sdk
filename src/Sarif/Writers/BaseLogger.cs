// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public abstract class BaseLogger
    {
        public readonly static FailureLevelSet ErrorWarningNote = new FailureLevelSet([FailureLevel.Error, FailureLevel.Warning, FailureLevel.Note]);
        public readonly static FailureLevelSet ErrorWarning = new FailureLevelSet([FailureLevel.Error, FailureLevel.Warning]);

        public readonly static ResultKindSet Fail = new ResultKindSet(new List<ResultKind>([ResultKind.Fail]));

        protected readonly FailureLevelSet _failureLevels;
        protected readonly ResultKindSet _resultKinds;

        protected BaseLogger(FailureLevelSet failureLevels, ResultKindSet resultKinds)
        {
            if (failureLevels == null && resultKinds == null)
            {
                _failureLevels = ErrorWarning;
                _resultKinds = Fail;
            }
            else
            {
                _failureLevels = failureLevels;
                _resultKinds = resultKinds;
            }

            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (_resultKinds.Count == 0 || (_resultKinds.Count == 1 && _resultKinds.Contains(ResultKind.None)))
            {
                throw new ArgumentException("At least one kind is required");
            }

            bool failureLevelsEffectivelyEmpty = _failureLevels == null || _failureLevels.Count == 0;

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
