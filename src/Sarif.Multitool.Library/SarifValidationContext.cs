// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class SarifValidationContext : AnalyzerContextBase
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
                return Path.GetExtension(TargetUri.LocalPath).Equals(SarifConstants.SarifFileExtension, StringComparison.OrdinalIgnoreCase) ||
                       Path.GetExtension(TargetUri.LocalPath).Equals(".json", StringComparison.OrdinalIgnoreCase);
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

        public override Exception TargetLoadException { get; set; }

        public bool UpdateInputsToCurrentSarif { get; set; }

        private Uri _uri;

        public override Uri TargetUri
        {
            get
            {
                return _uri;
            }

            set
            {
                if (_uri != null)
                {
                    throw new InvalidOperationException(MultitoolResources.ErrorIllegalContextReuse);
                }

                _uri = value;
            }
        }

        public string SchemaFilePath { get; internal set; }

        public string InputLogContents { get; internal set; }

        public SarifLog InputLog { get; internal set; }

        public Run CurrentRun { get; internal set; }

        public int CurrentRunIndex { get; internal set; }

        public Result CurrentResult { get; internal set; }

        public int CurrentResultIndex { get; internal set; }

        public ReportingDescriptorKind CurrentReportingDescriptorKind { get; internal set; }

        public JToken InputLogToken { get; internal set; }

        public override DefaultTraces Traces { get; set; }

        public override void Dispose()
        {
            // Nothing to dispose.
        }
    }
}
