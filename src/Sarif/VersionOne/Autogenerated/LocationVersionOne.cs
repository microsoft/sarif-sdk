// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// The location where an analysis tool produced a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class LocationVersionOne : PropertyBagHolder, ISarifNodeVersionOne
    {
        public static IEqualityComparer<LocationVersionOne> ValueComparer => LocationVersionOneEqualityComparer.Instance;

        public bool ValueEquals(LocationVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.LocationVersionOne;
            }
        }

        /// <summary>
        /// Identifies the file that the analysis tool was instructed to scan. This need not be the same as the file where the result actually occurred.
        /// </summary>
        [DataMember(Name = "analysisTarget", IsRequired = false, EmitDefaultValue = false)]
        public PhysicalLocationVersionOne AnalysisTarget { get; set; }

        /// <summary>
        /// Identifies the file where the analysis tool produced the result.
        /// </summary>
        [DataMember(Name = "resultFile", IsRequired = false, EmitDefaultValue = false)]
        public PhysicalLocationVersionOne ResultFile { get; set; }

        /// <summary>
        /// The human-readable fully qualified name of the logical location where the analysis tool produced the result. If 'logicalLocationKey' is not specified, this member is can used to retrieve the location logicalLocation from the logicalLocations dictionary, if one exists.
        /// </summary>
        [DataMember(Name = "fullyQualifiedLogicalName", IsRequired = false, EmitDefaultValue = false)]
        public string FullyQualifiedLogicalName { get; set; }

        /// <summary>
        /// A key used to retrieve the location logicalLocation from the logicalLocations dictionary, when the string specified by 'fullyQualifiedLogicalName' is not unique.
        /// </summary>
        [DataMember(Name = "logicalLocationKey", IsRequired = false, EmitDefaultValue = false)]
        public string LogicalLocationKey { get; set; }

        /// <summary>
        /// The machine-readable fully qualified name for the logical location where the analysis tool produced the result, such as the mangled function name provided by a C++ compiler that encodes calling convention, return type and other details along with the function name.
        /// </summary>
        [DataMember(Name = "decoratedName", IsRequired = false, EmitDefaultValue = false)]
        public string DecoratedName { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationVersionOne" /> class.
        /// </summary>
        public LocationVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="analysisTarget">
        /// An initialization value for the <see cref="P: AnalysisTarget" /> property.
        /// </param>
        /// <param name="resultFile">
        /// An initialization value for the <see cref="P: ResultFile" /> property.
        /// </param>
        /// <param name="fullyQualifiedLogicalName">
        /// An initialization value for the <see cref="P: FullyQualifiedLogicalName" /> property.
        /// </param>
        /// <param name="logicalLocationKey">
        /// An initialization value for the <see cref="P: LogicalLocationKey" /> property.
        /// </param>
        /// <param name="decoratedName">
        /// An initialization value for the <see cref="P: DecoratedName" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public LocationVersionOne(PhysicalLocationVersionOne analysisTarget, PhysicalLocationVersionOne resultFile, string fullyQualifiedLogicalName, string logicalLocationKey, string decoratedName, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(analysisTarget, resultFile, fullyQualifiedLogicalName, logicalLocationKey, decoratedName, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public LocationVersionOne(LocationVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.AnalysisTarget, other.ResultFile, other.FullyQualifiedLogicalName, other.LogicalLocationKey, other.DecoratedName, other.Properties);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public LocationVersionOne DeepClone()
        {
            return (LocationVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new LocationVersionOne(this);
        }

        private void Init(PhysicalLocationVersionOne analysisTarget, PhysicalLocationVersionOne resultFile, string fullyQualifiedLogicalName, string logicalLocationKey, string decoratedName, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (analysisTarget != null)
            {
                AnalysisTarget = new PhysicalLocationVersionOne(analysisTarget);
            }

            if (resultFile != null)
            {
                ResultFile = new PhysicalLocationVersionOne(resultFile);
            }

            FullyQualifiedLogicalName = fullyQualifiedLogicalName;
            LogicalLocationKey = logicalLocationKey;
            DecoratedName = decoratedName;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}