// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    public abstract class BaseLogger
    {
        public readonly static FailureLevelSet ErrorWarningNote = new FailureLevelSet(new[] { FailureLevel.Error, FailureLevel.Warning, FailureLevel.Note });
        public readonly static FailureLevelSet ErrorWarning = new FailureLevelSet(new[] { FailureLevel.Error, FailureLevel.Warning });

        public readonly static ResultKindSet Fail = new ResultKindSet(new List<ResultKind>(new[] { ResultKind.Fail }));

        protected readonly FailureLevelSet _failureLevels;
        protected readonly ResultKindSet _resultKinds;

        private readonly int _resultsLimitPerRuleTarget;
        private readonly Dictionary<Tuple<string, Uri>, int> _resultsCount;
        protected int _resultsLimited = 0;

        protected BaseLogger(FailureLevelSet failureLevels, ResultKindSet resultKinds, int resultsLimitPerRuleTarget)
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

            _resultsLimitPerRuleTarget = resultsLimitPerRuleTarget;
            if (_resultsLimitPerRuleTarget > 0)
            {
                _resultsCount = new Dictionary<Tuple<string, Uri>, int>();
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
            if (_resultsCount != null)
            {
                Location firstLocation = result.Locations.First();
                if (firstLocation.PhysicalLocation == null ||
                    result.Run?.OriginalUriBaseIds == null ||
                    !firstLocation.PhysicalLocation.ArtifactLocation.TryReconstructAbsoluteUri(result.Run.OriginalUriBaseIds, out Uri locationUri))
                {
                    locationUri = firstLocation.PhysicalLocation.ArtifactLocation.Uri;
                }
                var resultKey = new Tuple<string, Uri>(result.RuleId, locationUri);

                if (_resultsCount.TryGetValue(resultKey, out int currentCount))
                {
                    if (currentCount >= _resultsLimitPerRuleTarget)
                    {
                        ++_resultsLimited;
                        return false;
                    }

                    _resultsCount[resultKey] = currentCount + 1;
                }
                else
                {
                    _resultsCount.Add(resultKey, 1);
                }
            }

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
