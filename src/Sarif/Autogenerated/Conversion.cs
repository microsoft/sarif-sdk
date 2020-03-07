// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Describes how a converter transformed the output of a static analysis tool from the analysis tool's native output format into the SARIF format.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Conversion : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Conversion> ValueComparer => ConversionEqualityComparer.Instance;

        public bool ValueEquals(Conversion other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Conversion;
            }
        }

        /// <summary>
        /// A tool object that describes the converter.
        /// </summary>
        [DataMember(Name = "tool", IsRequired = true)]
        public virtual Tool Tool { get; set; }

        /// <summary>
        /// An invocation object that describes the invocation of the converter.
        /// </summary>
        [DataMember(Name = "invocation", IsRequired = false, EmitDefaultValue = false)]
        public virtual Invocation Invocation { get; set; }

        /// <summary>
        /// The locations of the analysis tool's per-run log files.
        /// </summary>
        [DataMember(Name = "analysisToolLogFiles", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ArtifactLocation> AnalysisToolLogFiles { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the conversion.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Conversion" /> class.
        /// </summary>
        public Conversion()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Conversion" /> class from the supplied values.
        /// </summary>
        /// <param name="tool">
        /// An initialization value for the <see cref="P:Tool" /> property.
        /// </param>
        /// <param name="invocation">
        /// An initialization value for the <see cref="P:Invocation" /> property.
        /// </param>
        /// <param name="analysisToolLogFiles">
        /// An initialization value for the <see cref="P:AnalysisToolLogFiles" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Conversion(Tool tool, Invocation invocation, IEnumerable<ArtifactLocation> analysisToolLogFiles, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(tool, invocation, analysisToolLogFiles, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Conversion" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Conversion(Conversion other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Tool, other.Invocation, other.AnalysisToolLogFiles, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Conversion DeepClone()
        {
            return (Conversion)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Conversion(this);
        }

        protected virtual void Init(Tool tool, Invocation invocation, IEnumerable<ArtifactLocation> analysisToolLogFiles, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (tool != null)
            {
                Tool = new Tool(tool);
            }

            if (invocation != null)
            {
                Invocation = new Invocation(invocation);
            }

            if (analysisToolLogFiles != null)
            {
                var destination_0 = new List<ArtifactLocation>();
                foreach (var value_0 in analysisToolLogFiles)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ArtifactLocation(value_0));
                    }
                }

                AnalysisToolLogFiles = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}