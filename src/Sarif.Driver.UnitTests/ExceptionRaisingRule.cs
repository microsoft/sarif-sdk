﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    internal class ExceptionRaisingRule : PropertyBagHolder, IRule, ISkimmer<TestAnalysisContext>
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

        public Uri HelpUri { get; set; }

        public string Help => null;

        public string Id
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

        public ResultLevel DefaultLevel => ResultLevel.Warning;

        public string Name
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

        public string FullDescription => "Test Rule Description";

        public string ShortDescription => throw new NotImplementedException();

        public string RichDescription => throw new NotImplementedException();

        public IDictionary<string, string> Options => throw new NotImplementedException();

        public IDictionary<string, string> MessageTemplates
        {
            get
            {
                return new Dictionary<string, string> { { nameof(SdkResources.NotApplicable_InvalidMetadata) , SdkResources.NotApplicable_InvalidMetadata }};
            }
        }

        public IDictionary<string, string> RichMessageTemplates => throw new NotImplementedException();

        internal override IDictionary<string, SerializedPropertyInfo> Properties
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public RuleConfiguration Configuration
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Analyze(TestAnalysisContext context)
        {
            if (_exceptionCondition == ExceptionCondition.InvokingAnalyze)
            {
                throw new InvalidOperationException(nameof(ExceptionCondition.InvokingAnalyze));
            }

            if (_exceptionCondition == ExceptionCondition.ParsingTarget)
            {
                Errors.LogTargetParseError(
                    context,
                    new Region
                    {
                        StartLine = 42,
                        StartColumn = 54
                    },
                    "Could not parse target.");
            }

            if (_exceptionCondition == ExceptionCondition.LoadingPdb)
            {
                Errors.LogExceptionLoadingPdb(context, new InvalidOperationException("Test message"));
            }
        }

        public AnalysisApplicability CanAnalyze(TestAnalysisContext context, out string reasonIfNotApplicable)
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

        public void Initialize(TestAnalysisContext context)
        {
            if (_exceptionCondition == ExceptionCondition.InvokingInitialize)
            {
                throw new InvalidOperationException(nameof(ExceptionCondition.InvokingInitialize));
            }
        }
    }
}
