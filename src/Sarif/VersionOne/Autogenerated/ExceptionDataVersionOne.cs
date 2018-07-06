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
    public partial class ExceptionDataVersionOne : ISarifNodeVersionOne
    {
        public static IEqualityComparer<ExceptionDataVersionOne> ValueComparer => ExceptionDataVersionOneEqualityComparer.Instance;

        public bool ValueEquals(ExceptionDataVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.ExceptionDataVersionOne;
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
        public StackVersionOne Stack { get; set; }

        /// <summary>
        /// An array of exception objects each of which is considered a cause of this exception.
        /// </summary>
        [DataMember(Name = "innerExceptions", IsRequired = false, EmitDefaultValue = false)]
        public IList<ExceptionDataVersionOne> InnerExceptions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionDataVersionOne" /> class.
        /// </summary>
        public ExceptionDataVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionDataVersionOne" /> class from the supplied values.
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
        public ExceptionDataVersionOne(string kind, string message, StackVersionOne stack, IEnumerable<ExceptionDataVersionOne> innerExceptions)
        {
            Init(kind, message, stack, innerExceptions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionDataVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public ExceptionDataVersionOne(ExceptionDataVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Kind, other.Message, other.Stack, other.InnerExceptions);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public ExceptionDataVersionOne DeepClone()
        {
            return (ExceptionDataVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new ExceptionDataVersionOne(this);
        }

        private void Init(string kind, string message, StackVersionOne stack, IEnumerable<ExceptionDataVersionOne> innerExceptions)
        {
            Kind = kind;
            Message = message;
            if (stack != null)
            {
                Stack = new StackVersionOne(stack);
            }

            if (innerExceptions != null)
            {
                var destination_0 = new List<ExceptionDataVersionOne>();
                foreach (var value_0 in innerExceptions)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new ExceptionDataVersionOne(value_0));
                    }
                }

                InnerExceptions = destination_0;
            }
        }
    }
}