// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Information that describes a run's identity and role within an engineering system process.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class RunAutomationDetails : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<RunAutomationDetails> ValueComparer => RunAutomationDetailsEqualityComparer.Instance;

        public bool ValueEquals(RunAutomationDetails other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.RunAutomationDetails;
            }
        }

        /// <summary>
        /// A description of the identity and role played within the engineering system by this object's containing run object.
        /// </summary>
        [DataMember(Name = "description", IsRequired = false, EmitDefaultValue = false)]
        public virtual Message Description { get; set; }

        /// <summary>
        /// A hierarchical string that uniquely identifies this object's containing run object.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Id { get; set; }

        /// <summary>
        /// A stable, unique identifer for this object's containing run object in the form of a GUID.
        /// </summary>
        [DataMember(Name = "guid", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Guid { get; set; }

        /// <summary>
        /// A stable, unique identifier for the equivalence class of runs to which this object's containing run object belongs in the form of a GUID.
        /// </summary>
        [DataMember(Name = "correlationGuid", IsRequired = false, EmitDefaultValue = false)]
        public virtual string CorrelationGuid { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the run automation details.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunAutomationDetails" /> class.
        /// </summary>
        public RunAutomationDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunAutomationDetails" /> class from the supplied values.
        /// </summary>
        /// <param name="description">
        /// An initialization value for the <see cref="P:Description" /> property.
        /// </param>
        /// <param name="id">
        /// An initialization value for the <see cref="P:Id" /> property.
        /// </param>
        /// <param name="guid">
        /// An initialization value for the <see cref="P:Guid" /> property.
        /// </param>
        /// <param name="correlationGuid">
        /// An initialization value for the <see cref="P:CorrelationGuid" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public RunAutomationDetails(Message description, string id, string guid, string correlationGuid, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(description, id, guid, correlationGuid, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RunAutomationDetails" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public RunAutomationDetails(RunAutomationDetails other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Description, other.Id, other.Guid, other.CorrelationGuid, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual RunAutomationDetails DeepClone()
        {
            return (RunAutomationDetails)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new RunAutomationDetails(this);
        }

        protected virtual void Init(Message description, string id, string guid, string correlationGuid, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (description != null)
            {
                Description = new Message(description);
            }

            Id = id;
            Guid = guid;
            CorrelationGuid = correlationGuid;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}