// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A portion of an analysis tool's output that a converter transformed into a result object.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class AnalysisToolLogFileContents : ISarifNode
    {
        public static IEqualityComparer<AnalysisToolLogFileContents> ValueComparer => AnalysisToolLogFileContentsEqualityComparer.Instance;

        public bool ValueEquals(AnalysisToolLogFileContents other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.AnalysisToolLogFileContents;
            }
        }

        /// <summary>
        /// The region of an analysis tool's log file that was transformed into the result object.
        /// </summary>
        [DataMember(Name = "region", IsRequired = false, EmitDefaultValue = false)]
        public Region Region { get; set; }

        /// <summary>
        /// The text of that region of the analysis tool's log file that was transformed into the result object.
        /// </summary>
        [DataMember(Name = "snippet", IsRequired = false, EmitDefaultValue = false)]
        public string Snippet { get; set; }

        /// <summary>
        /// A string that represents the location of the analysis tool's log file as a valid URI.
        /// </summary>
        [DataMember(Name = "analysisToolLogFileUri", IsRequired = false, EmitDefaultValue = false)]
        public Uri AnalysisToolLogFileUri { get; set; }

        /// <summary>
        /// A string that identifies the conceptual base for the 'analysisToolLogFileUri' property (if it is relative), e.g.,'$(LogDir)' or '%LOGROOT%'.
        /// </summary>
        [DataMember(Name = "analysisToolLogFileUriBaseId", IsRequired = false, EmitDefaultValue = false)]
        public string AnalysisToolLogFileUriBaseId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisToolLogFileContents" /> class.
        /// </summary>
        public AnalysisToolLogFileContents()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisToolLogFileContents" /> class from the supplied values.
        /// </summary>
        /// <param name="region">
        /// An initialization value for the <see cref="P: Region" /> property.
        /// </param>
        /// <param name="snippet">
        /// An initialization value for the <see cref="P: Snippet" /> property.
        /// </param>
        /// <param name="analysisToolLogFileUri">
        /// An initialization value for the <see cref="P: AnalysisToolLogFileUri" /> property.
        /// </param>
        /// <param name="analysisToolLogFileUriBaseId">
        /// An initialization value for the <see cref="P: AnalysisToolLogFileUriBaseId" /> property.
        /// </param>
        public AnalysisToolLogFileContents(Region region, string snippet, Uri analysisToolLogFileUri, string analysisToolLogFileUriBaseId)
        {
            Init(region, snippet, analysisToolLogFileUri, analysisToolLogFileUriBaseId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisToolLogFileContents" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public AnalysisToolLogFileContents(AnalysisToolLogFileContents other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Region, other.Snippet, other.AnalysisToolLogFileUri, other.AnalysisToolLogFileUriBaseId);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public AnalysisToolLogFileContents DeepClone()
        {
            return (AnalysisToolLogFileContents)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new AnalysisToolLogFileContents(this);
        }

        private void Init(Region region, string snippet, Uri analysisToolLogFileUri, string analysisToolLogFileUriBaseId)
        {
            if (region != null)
            {
                Region = new Region(region);
            }

            Snippet = snippet;
            if (analysisToolLogFileUri != null)
            {
                AnalysisToolLogFileUri = new Uri(analysisToolLogFileUri.OriginalString, analysisToolLogFileUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            AnalysisToolLogFileUriBaseId = analysisToolLogFileUriBaseId;
        }
    }
}