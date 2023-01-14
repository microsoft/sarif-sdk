// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    public abstract class AnalyzeContextBase : IAnalysisContext, IOptionsProvider
    {
        public AnalyzeContextBase()
        {
            this.Policy = new PropertiesDictionary();
        }

        // All of these properties are persisted to configuration XML and can be
        // passed using that mechanism. All command-line arguments are 
        // candidates to follow this pattern.
        public IEnumerable<IOption> GetOptions()
        {
            return new[]
            {
                MaxFileSizeInKilobytesProperty
            };
        }

        public abstract Uri TargetUri { get; set; }
        public abstract string MimeType { get; set; }
        public abstract HashData Hashes { get; set; }
        public abstract Exception TargetLoadException { get; set; }
        public abstract bool IsValidAnalysisTarget { get; }
        public abstract ReportingDescriptor Rule { get; set; }
        public PropertiesDictionary Policy { get; set; }
        public abstract IAnalysisLogger Logger { get; set; }
        public abstract RuntimeConditions RuntimeErrors { get; set; }
        public abstract bool AnalysisComplete { get; set; }
        public abstract ISet<string> Traces { get; set; }

        public long MaxFileSizeInKilobytes
        {
            get { return this.Policy.GetProperty(MaxFileSizeInKilobytesProperty); }
            set { this.Policy.SetProperty(MaxFileSizeInKilobytesProperty, value); }
        }

        abstract public void Dispose();

        public static long MaxFileSizeInKilobytesDefaultValue { get; set; } = 1024;

        public static PerLanguageOption<long> MaxFileSizeInKilobytesProperty { get; } =
            new PerLanguageOption<long>(
                "CoreSettings", nameof(MaxFileSizeInKilobytes), defaultValue: () => MaxFileSizeInKilobytesDefaultValue,
                "Scan targets that fall below this size threshold (in kilobytes) will not be analyzed. " +
                "It is legal to set this value to 0 (in order to potentially complete an analysis that " +
                "records what scan targets would have been analyzed, given current configuration. " +
                $"Negative values will be discarded in favor of the default of {MaxFileSizeInKilobytesDefaultValue} KB.");
    }
}
