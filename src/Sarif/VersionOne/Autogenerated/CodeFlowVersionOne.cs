// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class CodeFlowVersionOne : PropertyBagHolder, ISarifNodeVersionOne
    {
        public static IEqualityComparer<CodeFlowVersionOne> ValueComparer => CodeFlowVersionOneEqualityComparer.Instance;

        public bool ValueEquals(CodeFlowVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.CodeFlowVersionOne;
            }
        }

        /// <summary>
        /// A message relevant to the code flow
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// An array of 'annotatedCodeLocation' objects, each of which describes a single location visited by the tool in the course of producing the result.
        /// </summary>
        [DataMember(Name = "locations", IsRequired = true)]
        public IList<AnnotatedCodeLocationVersionOne> Locations { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the code flow.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlowVersionOne" /> class.
        /// </summary>
        public CodeFlowVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlowVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="locations">
        /// An initialization value for the <see cref="P: Locations" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public CodeFlowVersionOne(string message, IEnumerable<AnnotatedCodeLocationVersionOne> locations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(message, locations, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlowVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public CodeFlowVersionOne(CodeFlowVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Message, other.Locations, other.Properties);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public CodeFlowVersionOne DeepClone()
        {
            return (CodeFlowVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new CodeFlowVersionOne(this);
        }

        private void Init(string message, IEnumerable<AnnotatedCodeLocationVersionOne> locations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Message = message;
            if (locations != null)
            {
                var destination_0 = new List<AnnotatedCodeLocationVersionOne>();
                foreach (var value_0 in locations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new AnnotatedCodeLocationVersionOne(value_0));
                    }
                }

                Locations = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}