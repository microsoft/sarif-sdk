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
    public partial class ExceptionData : ISarifNode
    {
        public static IEqualityComparer<ExceptionData> ValueComparer => ExceptionDataEqualityComparer.Instance;

        public bool ValueEquals(ExceptionData other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
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
        public string Kind { get; set; }

        /// <summary>
        /// A string that describes the exception.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// The sequence of function calls leading to the exception.
        /// </summary>
        [DataMember(Name = "stack", IsRequired = false, EmitDefaultValue = false)]
        public Stack Stack { get; set; }

        /// <summary>
        /// An array of exception objects each of which is considered a cause of this exception.
        /// </summary>
        [DataMember(Name = "innerExceptions", IsRequired = false, EmitDefaultValue = false)]
        public IList<ExceptionData> InnerExceptions { get; set; }

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
        /// An initialization value for the <see cref="P: Kind" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="stack">
        /// An initialization value for the <see cref="P: Stack" /> property.
        /// </param>
        /// <param name="innerExceptions">
        /// An initialization value for the <see cref="P: InnerExceptions" /> property.
        /// </param>
        public ExceptionData(string kind, string message, Stack stack, IEnumerable<ExceptionData> innerExceptions)
        {
            Init(kind, message, stack, innerExceptions);
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

            Init(other.Kind, other.Message, other.Stack, other.InnerExceptions);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ExceptionData DeepClone()
        {
            return (ExceptionData)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new ExceptionData(this);
        }

        private void Init(string kind, string message, Stack stack, IEnumerable<ExceptionData> innerExceptions)
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
        }
    }
}