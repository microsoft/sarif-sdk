// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A function call within a stack trace.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.11.0.0")]
    public partial class StackFrame : ISarifNode, IEquatable<StackFrame>
    {
        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.StackFrame;
            }
        }

        /// <summary>
        /// A message relevant to this stack frame.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// The uri of the source code file to which this stack frame refers.
        /// </summary>
        [DataMember(Name = "uri", IsRequired = false, EmitDefaultValue = false)]
        public Uri Uri { get; set; }

        /// <summary>
        /// The line of the location to which this stack frame refers.
        /// </summary>
        [DataMember(Name = "line", IsRequired = false, EmitDefaultValue = false)]
        public int Line { get; set; }

        /// <summary>
        /// The line of the location to which this stack frame refers.
        /// </summary>
        [DataMember(Name = "column", IsRequired = false, EmitDefaultValue = false)]
        public int Column { get; set; }

        /// <summary>
        /// The name of the module that contains the code that is executing.
        /// </summary>
        [DataMember(Name = "module", IsRequired = false, EmitDefaultValue = false)]
        public string Module { get; set; }

        /// <summary>
        /// The fully qualified name of the method or function that is executing.
        /// </summary>
        [DataMember(Name = "fullyQualifiedLogicalName", IsRequired = false, EmitDefaultValue = false)]
        public string FullyQualifiedLogicalName { get; set; }

        /// <summary>
        /// A string used as a key into the logicalLocations dictionary, in case the string specified by 'fullyQualifiedLogicalName' is not unique.
        /// </summary>
        [DataMember(Name = "logicalLocationKey", IsRequired = false, EmitDefaultValue = false)]
        public string LogicalLocationKey { get; set; }

        /// <summary>
        /// The address of the method or function that is executing.
        /// </summary>
        [DataMember(Name = "address", IsRequired = false, EmitDefaultValue = false)]
        public int Address { get; set; }

        /// <summary>
        /// The offset from the method or function that is executing.
        /// </summary>
        [DataMember(Name = "offset", IsRequired = false, EmitDefaultValue = false)]
        public int Offset { get; set; }

        /// <summary>
        /// The parameters of the call that is executing.
        /// </summary>
        [DataMember(Name = "parameters", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Parameters { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional details about the stack frame.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A unique set of strings that provide additional information for the stack frame.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

        public override bool Equals(object other)
        {
            return Equals(other as StackFrame);
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

                if (Uri != null)
                {
                    result = (result * 31) + Uri.GetHashCode();
                }

                result = (result * 31) + Line.GetHashCode();
                result = (result * 31) + Column.GetHashCode();
                if (Module != null)
                {
                    result = (result * 31) + Module.GetHashCode();
                }

                if (FullyQualifiedLogicalName != null)
                {
                    result = (result * 31) + FullyQualifiedLogicalName.GetHashCode();
                }

                if (LogicalLocationKey != null)
                {
                    result = (result * 31) + LogicalLocationKey.GetHashCode();
                }

                result = (result * 31) + Address.GetHashCode();
                result = (result * 31) + Offset.GetHashCode();
                if (Parameters != null)
                {
                    foreach (var value_0 in Parameters)
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

        public bool Equals(StackFrame other)
        {
            if (other == null)
            {
                return false;
            }

            if (Message != other.Message)
            {
                return false;
            }

            if (Uri != other.Uri)
            {
                return false;
            }

            if (Line != other.Line)
            {
                return false;
            }

            if (Column != other.Column)
            {
                return false;
            }

            if (Module != other.Module)
            {
                return false;
            }

            if (FullyQualifiedLogicalName != other.FullyQualifiedLogicalName)
            {
                return false;
            }

            if (LogicalLocationKey != other.LogicalLocationKey)
            {
                return false;
            }

            if (Address != other.Address)
            {
                return false;
            }

            if (Offset != other.Offset)
            {
                return false;
            }

            if (!Object.ReferenceEquals(Parameters, other.Parameters))
            {
                if (Parameters == null || other.Parameters == null)
                {
                    return false;
                }

                if (Parameters.Count != other.Parameters.Count)
                {
                    return false;
                }

                for (int index_0 = 0; index_0 < Parameters.Count; ++index_0)
                {
                    if (Parameters[index_0] != other.Parameters[index_0])
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

                if (Tags.Count != other.Tags.Count)
                {
                    return false;
                }

                for (int index_1 = 0; index_1 < Tags.Count; ++index_1)
                {
                    if (Tags[index_1] != other.Tags[index_1])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrame" /> class.
        /// </summary>
        public StackFrame()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrame" /> class from the supplied values.
        /// </summary>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="uri">
        /// An initialization value for the <see cref="P: Uri" /> property.
        /// </param>
        /// <param name="line">
        /// An initialization value for the <see cref="P: Line" /> property.
        /// </param>
        /// <param name="column">
        /// An initialization value for the <see cref="P: Column" /> property.
        /// </param>
        /// <param name="module">
        /// An initialization value for the <see cref="P: Module" /> property.
        /// </param>
        /// <param name="fullyQualifiedLogicalName">
        /// An initialization value for the <see cref="P: FullyQualifiedLogicalName" /> property.
        /// </param>
        /// <param name="logicalLocationKey">
        /// An initialization value for the <see cref="P: LogicalLocationKey" /> property.
        /// </param>
        /// <param name="address">
        /// An initialization value for the <see cref="P: Address" /> property.
        /// </param>
        /// <param name="offset">
        /// An initialization value for the <see cref="P: Offset" /> property.
        /// </param>
        /// <param name="parameters">
        /// An initialization value for the <see cref="P: Parameters" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public StackFrame(string message, Uri uri, int line, int column, string module, string fullyQualifiedLogicalName, string logicalLocationKey, int address, int offset, IEnumerable<string> parameters, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Init(message, uri, line, column, module, fullyQualifiedLogicalName, logicalLocationKey, address, offset, parameters, properties, tags);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrame" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public StackFrame(StackFrame other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Message, other.Uri, other.Line, other.Column, other.Module, other.FullyQualifiedLogicalName, other.LogicalLocationKey, other.Address, other.Offset, other.Parameters, other.Properties, other.Tags);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public StackFrame DeepClone()
        {
            return (StackFrame)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new StackFrame(this);
        }

        private void Init(string message, Uri uri, int line, int column, string module, string fullyQualifiedLogicalName, string logicalLocationKey, int address, int offset, IEnumerable<string> parameters, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Message = message;
            if (uri != null)
            {
                Uri = new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            Line = line;
            Column = column;
            Module = module;
            FullyQualifiedLogicalName = fullyQualifiedLogicalName;
            LogicalLocationKey = logicalLocationKey;
            Address = address;
            Offset = offset;
            if (parameters != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in parameters)
                {
                    destination_0.Add(value_0);
                }

                Parameters = destination_0;
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