// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public abstract class BaseLogger
    {
        public static IImmutableSet<FailureLevel> ErrorWarning = new List<FailureLevel>(new[] { FailureLevel.Error, FailureLevel.Warning }).ToImmutableHashSet();

        public static IImmutableSet<ResultKind> Fail = new List<ResultKind>(new[] { ResultKind.Fail }).ToImmutableHashSet();

        protected readonly IImmutableSet<FailureLevel> _failureLevels;
        protected readonly IImmutableSet<ResultKind> _resultKinds;

        protected BaseLogger(IImmutableSet<FailureLevel> failureLevels, IImmutableSet<ResultKind> resultKinds)
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

            bool failureLevelsEffectivelyEmpty = _failureLevels == null
                                                    || _failureLevels.Count == 0
                                                    || (_failureLevels.Count == 1 && _failureLevels.Contains(FailureLevel.None));

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
