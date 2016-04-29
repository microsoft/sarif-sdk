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
    public partial class ExceptionData : ISarifNode, IEquatable<ExceptionData>
    {
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
        /// The fully qualified type name of the object that was thrown.
        /// </summary>
        [DataMember(Name = "type", IsRequired = false, EmitDefaultValue = false)]
        public string Type { get; set; }

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

        public override bool Equals(object other)
        {
            return Equals(other as ExceptionData);
        }

        public override int GetHashCode()
        {
            int result = 17;
            unchecked
            {
                if (Type != null)
                {
                    result = (result * 31) + Type.GetHashCode();
                }

                if (Message != null)
                {
                    result = (result * 31) + Message.GetHashCode();
                }

                if (Stack != null)
                {
                    result = (result * 31) + Stack.GetHashCode();
                }

                if (InnerExceptions != null)
                {
                    foreach (var value_0 in InnerExceptions)
                    {
                        result = result * 31;
                        if (value_0 != null)
                        {
                            result = (result * 31) + value_0.GetHashCode();
                        }
                    }
                }
            }

            return result;
        }

        public bool Equals(ExceptionData other)
        {
            if (other == null)
            {
                return false;
            }

            if (Type != other.Type)
            {
                return false;
            }

            if (Message != other.Message)
            {
                return false;
            }

            if (!Object.Equals(Stack, other.Stack))
            {
                return false;
            }

            if (!Object.ReferenceEquals(InnerExceptions, other.InnerExceptions))
            {
                if (InnerExceptions == null || other.InnerExceptions == null)
                {
                    return false;
                }

                if (InnerExceptions.Count != other.InnerExceptions.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < InnerExceptions.Count; ++index_0)
                {
                    if (!Object.Equals(InnerExceptions[index_0], other.InnerExceptions[index_0]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionData" /> class.
        /// </summary>
        public ExceptionData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionData" /> class from the supplied values.
        /// </summary>
        /// <param name="type">
        /// An initialization value for the <see cref="P: Type" /> property.
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
        public ExceptionData(string type, string message, Stack stack, IEnumerable<ExceptionData> innerExceptions)
        {
            Init(type, message, stack, innerExceptions);
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

            Init(other.Type, other.Message, other.Stack, other.InnerExceptions);
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

        private void Init(string type, string message, Stack stack, IEnumerable<ExceptionData> innerExceptions)
        {
            Type = type;
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