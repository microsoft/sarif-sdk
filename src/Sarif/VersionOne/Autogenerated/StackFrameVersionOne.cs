// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// A function call within a stack trace.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class StackFrameVersionOne : PropertyBagHolder, ISarifNodeVersionOne
    {
        public static IEqualityComparer<StackFrameVersionOne> ValueComparer => StackFrameVersionOneEqualityComparer.Instance;

        public bool ValueEquals(StackFrameVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.StackFrameVersionOne;
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
        /// A string that identifies the conceptual base for the 'uri' property (if it is relative), e.g.,'$(SolutionDir)' or '%SRCROOT%'.
        /// </summary>
        [DataMember(Name = "uriBaseId", IsRequired = false, EmitDefaultValue = false)]
        public string UriBaseId { get; set; }

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
        /// The name of the module that contains the code of this stack frame.
        /// </summary>
        [DataMember(Name = "module", IsRequired = false, EmitDefaultValue = false)]
        public string Module { get; set; }

        /// <summary>
        /// The thread identifier of the stack frame.
        /// </summary>
        [DataMember(Name = "threadId", IsRequired = false, EmitDefaultValue = false)]
        public int ThreadId { get; set; }

        /// <summary>
        /// The fully qualified name of the method or function that is executing.
        /// </summary>
        [DataMember(Name = "fullyQualifiedLogicalName", IsRequired = true)]
        public string FullyQualifiedLogicalName { get; set; }

        /// <summary>
        /// A key used to retrieve the stack frame logicalLocation from the logicalLocations dictionary, when the 'fullyQualifiedLogicalName' is not unique.
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
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrameVersionOne" /> class.
        /// </summary>
        public StackFrameVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrameVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="uri">
        /// An initialization value for the <see cref="P: Uri" /> property.
        /// </param>
        /// <param name="uriBaseId">
        /// An initialization value for the <see cref="P: UriBaseId" /> property.
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
        /// <param name="threadId">
        /// An initialization value for the <see cref="P: ThreadId" /> property.
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
        public StackFrameVersionOne(string message, Uri uri, string uriBaseId, int line, int column, string module, int threadId, string fullyQualifiedLogicalName, string logicalLocationKey, int address, int offset, IEnumerable<string> parameters, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(message, uri, uriBaseId, line, column, module, threadId, fullyQualifiedLogicalName, logicalLocationKey, address, offset, parameters, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrameVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public StackFrameVersionOne(StackFrameVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Message, other.Uri, other.UriBaseId, other.Line, other.Column, other.Module, other.ThreadId, other.FullyQualifiedLogicalName, other.LogicalLocationKey, other.Address, other.Offset, other.Parameters, other.Properties);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public StackFrameVersionOne DeepClone()
        {
            return (StackFrameVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new StackFrameVersionOne(this);
        }

        private void Init(string message, Uri uri, string uriBaseId, int line, int column, string module, int threadId, string fullyQualifiedLogicalName, string logicalLocationKey, int address, int offset, IEnumerable<string> parameters, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Message = message;
            if (uri != null)
            {
                Uri = new Uri(uri.OriginalString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
            }

            UriBaseId = uriBaseId;
            Line = line;
            Column = column;
            Module = module;
            ThreadId = threadId;
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
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}