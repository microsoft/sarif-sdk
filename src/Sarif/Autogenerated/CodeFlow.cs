// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public partial class CodeFlow : ISarifNode
    {
        public static IEqualityComparer<CodeFlow> ValueComparer => CodeFlowEqualityComparer.Instance;

        public bool ValueEquals(CodeFlow other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.CodeFlow;
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
        public IList<AnnotatedCodeLocation> Locations { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the code flow.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A unique set of strings that provide additional information about the code flow.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlow" /> class.
        /// </summary>
        public CodeFlow()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlow" /> class from the supplied values.
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
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public CodeFlow(string message, IEnumerable<AnnotatedCodeLocation> locations, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Init(message, locations, properties, tags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlow" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public CodeFlow(CodeFlow other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Message, other.Locations, other.Properties, other.Tags);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public CodeFlow DeepClone()
        {
            return (CodeFlow)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new CodeFlow(this);
        }

        private void Init(string message, IEnumerable<AnnotatedCodeLocation> locations, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Message = message;
            if (locations != null)
            {
                var destination_0 = new List<AnnotatedCodeLocation>();
                foreach (var value_0 in locations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new AnnotatedCodeLocation(value_0));
                    }
                }

                Locations = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, string>(properties);
            }

            if (tags != null)
            {
                var destination_1 = new List<string>();
                foreach (var value_1 in tags)
                {
                    destination_1.Add(value_1);
                }

                Tags = destination_1;
            }
        }
    }
}