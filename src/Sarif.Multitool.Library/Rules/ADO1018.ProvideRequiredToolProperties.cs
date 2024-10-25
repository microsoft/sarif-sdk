﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

using Microsoft.Json.Pointer;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    public class AdoProvideRequiredToolProperties
        : BaseProvideRequiredToolProperties
    {
        /// <summary>
        /// ADO1018
        /// </summary>
        public override string Id => RuleId.ADOProvideToolDriverProperties;

        public override MultiformatMessageString FullDescription => new MultiformatMessageString() { Text = RuleResources.ADO1018_ProvideRequiredToolProperties_FullDescription_Text };

        private readonly List<string> _messageResourceNames = new List<string>()
        {
            nameof(RuleResources.ADO1018_ProvideRequiredToolProperties_Error_MissingDriverFullName_Text)
        };

        protected override ICollection<string> MessageResourceNames => _messageResourceNames.Concat(BaseMessageResourceNames).ToList();

        public override HashSet<RuleKind> RuleKinds => new HashSet<RuleKind>(new[] { RuleKind.Ado });

        protected override string ServiceName => RuleResources.ServiceName_ADO;

        public AdoProvideRequiredToolProperties()
        {
            this.DefaultConfiguration.Level = FailureLevel.Error;
        }

        protected override void Analyze(Run run, string runPointer)
        {
            base.Analyze(run.Tool, runPointer.AtProperty(SarifPropertyName.Tool));

            if (run.Tool?.Driver != null && string.IsNullOrWhiteSpace(run.Tool.Driver.FullName))
            {
                // {0}: This 'driver' object does not provide a 'fullName' value. This property is required by the {1} service.
                LogResult(
                    runPointer
                        .AtProperty(SarifPropertyName.Tool)
                        .AtProperty(SarifPropertyName.Driver),
                    nameof(RuleResources.ADO1018_ProvideRequiredToolProperties_Error_MissingDriverFullName_Text));
            }
        }
    }
}
