// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Composition;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    [Export(typeof(ReportingDescriptor)), Export(typeof(Skimmer<TestAnalysisContext>))]
    internal class ExceptionRaisingRule : TestRuleBase
    {
        internal static ExceptionCondition s_exceptionCondition;

        private ExceptionCondition _exceptionCondition;

        public ExceptionRaisingRule()
        {
            _exceptionCondition = s_exceptionCondition;

            if (_exceptionCondition == ExceptionCondition.InvokingConstructor)
            {
                throw new InvalidOperationException(nameof(ExceptionCondition.InvokingConstructor));
            }
        }

        public string ExceptionRaisingRuleId = "TEST1001";

        public override SupportedPlatform SupportedPlatforms
        {
            get
            {
                if(_exceptionCondition ==ExceptionCondition.InvalidPlatform)
                {
                    return SupportedPlatform.Unknown;
                }
                return SupportedPlatform.All;
            }
        }

        public override string Id
        {
            get
            {
                if (_exceptionCondition == ExceptionCondition.AccessingId)
                {
                    throw new InvalidOperationException(nameof(ExceptionCondition.AccessingId));
                }
                return ExceptionRaisingRuleId;
            }
        }


        public override string Name
        {
            get
            {
                if (_exceptionCondition == ExceptionCondition.AccessingName)
                {
                    throw new InvalidOperationException(nameof(ExceptionCondition.AccessingName));
                }
                return nameof(ExceptionRaisingRule);
            }
        }

        public override void Analyze(TestAnalysisContext context)
        {
            switch (_exceptionCondition)
            {
                case ExceptionCondition.InvokingAnalyze:
                {
                    throw new InvalidOperationException(nameof(ExceptionCondition.InvokingAnalyze));
                }

                case ExceptionCondition.ParsingTarget:
                {
                    Errors.LogTargetParseError(
                        context,
                        new Region
                        {
                            StartLine = 42,
                            StartColumn = 54
                        },
                        "Could not parse target.");
                    break;
                }

                case ExceptionCondition.LoadingPdb:
                {
                    Errors.LogExceptionLoadingPdb(context, new InvalidOperationException("Test message"));
                    break;
                }

                default:
                {
                    context.Logger.Log(this,
                        new Result()
                        {
                            RuleId = Id,
                            Level = FailureLevel.Warning,
                            Kind = ResultKind.Fail,
                            Message = new Message { Text = "Default message from exception raising rule." }
                        });

                    break;
                }
            }
        }

        public override AnalysisApplicability CanAnalyze(TestAnalysisContext context, out string reasonIfNotApplicable)
        {
            reasonIfNotApplicable = null;
            if (_exceptionCondition == ExceptionCondition.InvokingCanAnalyze)
            {
                throw new InvalidOperationException(nameof(ExceptionCondition.InvokingCanAnalyze));
            }

            if (context.Options.RegardAnalysisTargetAsNotApplicable)
            {
                reasonIfNotApplicable = "testing NotApplicableToSpecifiedTarget";
                return AnalysisApplicability.NotApplicableToSpecifiedTarget;
            }

            return AnalysisApplicability.ApplicableToSpecifiedTarget;
        }

        public override void Initialize(TestAnalysisContext context)
        {
            if (_exceptionCondition == ExceptionCondition.InvokingInitialize)
            {
                throw new InvalidOperationException(nameof(ExceptionCondition.InvokingInitialize));
            }
        }
    }
}
