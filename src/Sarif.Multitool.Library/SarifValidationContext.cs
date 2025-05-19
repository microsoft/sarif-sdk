// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class SarifValidationContext : AnalyzeContextBase
    {
        public enum ReportingDescriptorKind
        {
            None,
            Rule,
            Notification,
            Taxon
        }

        public SarifValidationContext()
        {
            CurrentRunIndex = -1;
            CurrentResultIndex = -1;
            CurrentReportingDescriptorKind = ReportingDescriptorKind.None;
        }

        public override bool IsValidAnalysisTarget
        {
            get
            {
                return FileSystem.PathGetExtension(CurrentTarget.Uri.GetFileName()).Equals(SarifConstants.SarifFileExtension, StringComparison.OrdinalIgnoreCase) ||
                       FileSystem.PathGetExtension(CurrentTarget.Uri.GetFileName()).Equals(".json", StringComparison.OrdinalIgnoreCase);
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override IAnalysisLogger Logger { get; set; }

        public override string MimeType
        {
            get { return "application/sarif-json"; }
            set { throw new InvalidOperationException(); }
        }

        public override bool AnalysisComplete { get; set; }

        public override HashData Hashes { get; set; }

        public override ReportingDescriptor Rule { get; set; }

        public override RuntimeConditions RuntimeErrors { get; set; }

        public override IList<Exception> RuntimeExceptions { get; set; }

        public bool UpdateInputsToCurrentSarif { get; set; }

        public string SchemaFilePath { get; internal set; }

        public string InputLogContents { get; internal set; }

        public SarifLog InputLog { get; internal set; }

        public Run CurrentRun { get; internal set; }

        public int CurrentRunIndex { get; internal set; }

        public Result CurrentResult { get; internal set; }

        public int CurrentResultIndex { get; internal set; }

        public ReportingDescriptorKind CurrentReportingDescriptorKind { get; internal set; }

        public JToken InputLogToken { get; internal set; }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
