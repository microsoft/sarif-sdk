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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public partial class StackFrame : ISarifNode
    {
        public static IEqualityComparer<StackFrame> ValueComparer => StackFrameEqualityComparer.Instance;

        public bool ValueEquals(StackFrame other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

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
        /// Key/value pairs that provide additional information about the stack frame.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A unique set of strings that provide additional information about the stack frame.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

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