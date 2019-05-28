// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    public class SarifValidationContext : IAnalysisContext
    {
        public bool IsValidAnalysisTarget
        {
            get
            {
                return Path.GetExtension(TargetUri.LocalPath).Equals(".sarif", StringComparison.OrdinalIgnoreCase) ||
                       Path.GetExtension(TargetUri.LocalPath).Equals(".json", StringComparison.OrdinalIgnoreCase);
            }
        }

        public IAnalysisLogger Logger { get; set; }

        public string MimeType
        {
            get { return "application/sarif-json"; }
            set { throw new InvalidOperationException(); }
        }

        public PropertiesDictionary Policy { get; set; }

        public ReportingDescriptor Rule { get; set; }

        public RuntimeConditions RuntimeErrors { get; set; }

        public Exception TargetLoadException { get; set; }

        public bool UpdateInputsToCurrentSarif { get; set; }

        private Uri _uri;

        public Uri TargetUri
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

        public JToken InputLogToken { get; internal set; }

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }
}
