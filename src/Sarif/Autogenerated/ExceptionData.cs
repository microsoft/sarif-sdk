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
    /// Describes a runtime exception encountered during the execution of an analysis tool.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class ExceptionData : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<ExceptionData> ValueComparer => ExceptionDataEqualityComparer.Instance;

        public bool ValueEquals(ExceptionData other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.ExceptionData;
            }
        }

        /// <summary>
        /// A string that identifies the kind of exception, for example, the fully qualified type name of an object that was thrown, or the symbolic name of a signal.
        /// </summary>
        [DataMember(Name = "kind", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Kind { get; set; }

        /// <summary>
        /// A message that describes the exception.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Message { get; set; }

        /// <summary>
        /// The sequence of function calls leading to the exception.
        /// </summary>
        [DataMember(Name = "stack", IsRequired = false, EmitDefaultValue = false)]
        public virtual Stack Stack { get; set; }

        /// <summary>
        /// An array of exception objects each of which is considered a cause of this exception.
        /// </summary>
        [DataMember(Name = "innerExceptions", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<ExceptionData> InnerExceptions { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the exception.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionData" /> class.
        /// </summary>
        public ExceptionData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionData" /> class from the supplied values.
        /// </summary>
        /// <param name="kind">
        /// An initialization value for the <see cref="P:Kind" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P:Message" /> property.
        /// </param>
        /// <param name="stack">
        /// An initialization value for the <see cref="P:Stack" /> property.
        /// </param>
        /// <param name="innerExceptions">
        /// An initialization value for the <see cref="P:InnerExceptions" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public ExceptionData(string kind, string message, Stack stack, IEnumerable<ExceptionData> innerExceptions, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(kind, message, stack, innerExceptions, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionData" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ExceptionData(ExceptionData other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Kind, other.Message, other.Stack, other.InnerExceptions, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual ExceptionData DeepClone()
        {
            return (ExceptionData)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ExceptionData(this);
        }

        protected virtual void Init(string kind, string message, Stack stack, IEnumerable<ExceptionData> innerExceptions, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Kind = kind;
            Message = message;
            if (stack != null)
            {
                Stack = new Stack(stack);
            }

            if (innerExceptions != null)
            {
                var destination_0 = new List<ExceptionData>();
                foreach (var value_0 in innerExceptions)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ExceptionData(value_0));
                    }
                }

                InnerExceptions = destination_0;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}