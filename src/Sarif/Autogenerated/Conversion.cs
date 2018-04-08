// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Describes how a converter transformed the output of a static analysis tool from the analysis tool's native output format into the SARIF format.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class Conversion : ISarifNode
    {
        public static IEqualityComparer<Conversion> ValueComparer => ConversionEqualityComparer.Instance;

        public bool ValueEquals(Conversion other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
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
        public Tool Tool { get; set; }

        /// <summary>
        /// An invocation object that describes the invocation of the converter.
        /// </summary>
        [DataMember(Name = "invocation", IsRequired = false, EmitDefaultValue = false)]
        public Invocation Invocation { get; set; }

        /// <summary>
        /// The location of the analysis tool's log file.
        /// </summary>
        [DataMember(Name = "analysisToolLogFileLocation", IsRequired = false, EmitDefaultValue = false)]
        public FileLocation AnalysisToolLogFileLocation { get; set; }

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
        /// An initialization value for the <see cref="P: Tool" /> property.
        /// </param>
        /// <param name="invocation">
        /// An initialization value for the <see cref="P: Invocation" /> property.
        /// </param>
        /// <param name="analysisToolLogFileLocation">
        /// An initialization value for the <see cref="P: AnalysisToolLogFileLocation" /> property.
        /// </param>
        public Conversion(Tool tool, Invocation invocation, FileLocation analysisToolLogFileLocation)
        {
            Init(tool, invocation, analysisToolLogFileLocation);
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

            Init(other.Tool, other.Invocation, other.AnalysisToolLogFileLocation);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Conversion DeepClone()
        {
            return (Conversion)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Conversion(this);
        }

        private void Init(Tool tool, Invocation invocation, FileLocation analysisToolLogFileLocation)
        {
            if (tool != null)
            {
                Tool = new Tool(tool);
            }

            if (invocation != null)
            {
                Invocation = new Invocation(invocation);
            }

            if (analysisToolLogFileLocation != null)
            {
                AnalysisToolLogFileLocation = new FileLocation(analysisToolLogFileLocation);
            }
        }
    }
}