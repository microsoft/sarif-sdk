// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.16.0.0")]
    public partial class CodeFlow : ISarifNode, IEquatable<CodeFlow>
    {
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
        public ISet<string> Tags { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as CodeFlow);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (Message != null)
                {
                    result = (result * 31) + Message.GetHashCode();
                }

                if (Locations != null)
                {
                    foreach (var value_0 in Locations)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }

                if (Properties != null)
                {
                    // Use xor for dictionaries to be order-independent.
                    int xor_0 = 0;
                    foreach (var value_1 in Properties)
                    {
                        xor_0 ^= value_1.Key.GetHashCode();
                        if (value_1.Value != null)
                        {
                            xor_0 ^= value_1.Value.GetHashCode();
                        }
                    }

                    result = (result * 31) + xor_0;
                }

                if (Tags != null)
                {
                    foreach (var value_2 in Tags)
                    {
                        result = result * 31;
                        if (value_2 != null)
                        {
                            result = (result * 31) + value_2.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(CodeFlow other)
        {
            if (other == null)
            {
                return false;
            }

            if (Message != other.Message)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Locations, other.Locations))
            {
                if (Locations == null || other.Locations == null)
                {
                    return false;
                }

                if (Locations.Count != other.Locations.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < Locations.Count; ++index_0)
                {
                    if (!Object.Equals(Locations[index_0], other.Locations[index_0]))
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(Properties, other.Properties))
            {
                if (Properties == null || other.Properties == null || Properties.Count != other.Properties.Count)
                {
                    return false;
                }

                foreach (var value_0 in Properties)
                {
                    string value_1;
                    if (!other.Properties.TryGetValue(value_0.Key, out value_1))
                    {
                        return false;
                    }

                    if (value_0.Value != value_1)
                    {
                        return false;
                    }
                }
            }

            if (!Object.ReferenceEquals(Tags, other.Tags))
            {
                if (Tags == null || other.Tags == null)
                {
                    return false;
                }

                if (!Tags.SetEquals(other.Tags))
                {
                    return false;
                }
            }

            return true;
        }

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
        public CodeFlow(string message, IEnumerable<AnnotatedCodeLocation> locations, IDictionary<string, string> properties, ISet<string> tags)
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

        private void Init(string message, IEnumerable<AnnotatedCodeLocation> locations, IDictionary<string, string> properties, ISet<string> tags)
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
                var destination_1 = new HashSet<string>();
                foreach (var value_1 in tags)
                {
                    destination_1.Add(value_1);
                }

                Tags = destination_1;
            }
        }
    }
}