// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A web response object.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class Response : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Response> ValueComparer => ResponseEqualityComparer.Instance;

        public bool ValueEquals(Response other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Response;
            }
        }

        /// <summary>
        /// The index within the run.responses array of the response object associated with this result.
        /// </summary>
        [DataMember(Name = "index", IsRequired = false, EmitDefaultValue = false)]
        public int Index { get; set; }

        /// <summary>
        /// The response protocol. Example: 'http'.
        /// </summary>
        [DataMember(Name = "protocol", IsRequired = false, EmitDefaultValue = false)]
        public string Protocol { get; set; }

        /// <summary>
        /// The response version. Example: '1.1'.
        /// </summary>
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        public string Version { get; set; }

        /// <summary>
        /// The response status code. Example: 404.
        /// </summary>
        [DataMember(Name = "statusCode", IsRequired = false, EmitDefaultValue = false)]
        public int StatusCode { get; set; }

        /// <summary>
        /// The response reason. Example: 'Not found'.
        /// </summary>
        [DataMember(Name = "reasonPhrase", IsRequired = false, EmitDefaultValue = false)]
        public int ReasonPhrase { get; set; }

        /// <summary>
        /// The response headers.
        /// </summary>
        [DataMember(Name = "headers", IsRequired = false, EmitDefaultValue = false)]
        public object Headers { get; set; }

        /// <summary>
        /// The body of the response.
        /// </summary>
        [DataMember(Name = "body", IsRequired = false, EmitDefaultValue = false)]
        public string Body { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the response.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Response" /> class.
        /// </summary>
        public Response()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Response" /> class from the supplied values.
        /// </summary>
        /// <param name="index">
        /// An initialization value for the <see cref="P:Index" /> property.
        /// </param>
        /// <param name="protocol">
        /// An initialization value for the <see cref="P:Protocol" /> property.
        /// </param>
        /// <param name="version">
        /// An initialization value for the <see cref="P:Version" /> property.
        /// </param>
        /// <param name="statusCode">
        /// An initialization value for the <see cref="P:StatusCode" /> property.
        /// </param>
        /// <param name="reasonPhrase">
        /// An initialization value for the <see cref="P:ReasonPhrase" /> property.
        /// </param>
        /// <param name="headers">
        /// An initialization value for the <see cref="P:Headers" /> property.
        /// </param>
        /// <param name="body">
        /// An initialization value for the <see cref="P:Body" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Response(int index, string protocol, string version, int statusCode, int reasonPhrase, object headers, string body, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(index, protocol, version, statusCode, reasonPhrase, headers, body, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Response" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Response(Response other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Index, other.Protocol, other.Version, other.StatusCode, other.ReasonPhrase, other.Headers, other.Body, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Response DeepClone()
        {
            return (Response)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Response(this);
        }

        private void Init(int index, string protocol, string version, int statusCode, int reasonPhrase, object headers, string body, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Index = index;
            Protocol = protocol;
            Version = version;
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            Headers = headers;
            Body = body;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}