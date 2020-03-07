// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Describes the response to an HTTP request.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class WebResponse : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<WebResponse> ValueComparer => WebResponseEqualityComparer.Instance;

        public bool ValueEquals(WebResponse other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.WebResponse;
            }
        }

        /// <summary>
        /// The index within the run.webResponses array of the response object associated with this result.
        /// </summary>
        [DataMember(Name = "index", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int Index { get; set; }

        /// <summary>
        /// The response protocol. Example: 'http'.
        /// </summary>
        [DataMember(Name = "protocol", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Protocol { get; set; }

        /// <summary>
        /// The response version. Example: '1.1'.
        /// </summary>
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Version { get; set; }

        /// <summary>
        /// The response status code. Example: 451.
        /// </summary>
        [DataMember(Name = "statusCode", IsRequired = false, EmitDefaultValue = false)]
        public virtual int StatusCode { get; set; }

        /// <summary>
        /// The response reason. Example: 'Not found'.
        /// </summary>
        [DataMember(Name = "reasonPhrase", IsRequired = false, EmitDefaultValue = false)]
        public virtual string ReasonPhrase { get; set; }

        /// <summary>
        /// The response headers.
        /// </summary>
        [DataMember(Name = "headers", IsRequired = false, EmitDefaultValue = false)]
        public virtual IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The body of the response.
        /// </summary>
        [DataMember(Name = "body", IsRequired = false, EmitDefaultValue = false)]
        public virtual ArtifactContent Body { get; set; }

        /// <summary>
        /// Specifies whether a response was received from the server.
        /// </summary>
        [DataMember(Name = "noResponseReceived", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual bool NoResponseReceived { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the response.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponse" /> class.
        /// </summary>
        public WebResponse()
        {
            Index = -1;
            NoResponseReceived = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponse" /> class from the supplied values.
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
        /// <param name="noResponseReceived">
        /// An initialization value for the <see cref="P:NoResponseReceived" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public WebResponse(int index, string protocol, string version, int statusCode, string reasonPhrase, IDictionary<string, string> headers, ArtifactContent body, bool noResponseReceived, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(index, protocol, version, statusCode, reasonPhrase, headers, body, noResponseReceived, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebResponse" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public WebResponse(WebResponse other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Index, other.Protocol, other.Version, other.StatusCode, other.ReasonPhrase, other.Headers, other.Body, other.NoResponseReceived, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual WebResponse DeepClone()
        {
            return (WebResponse)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new WebResponse(this);
        }

        protected virtual void Init(int index, string protocol, string version, int statusCode, string reasonPhrase, IDictionary<string, string> headers, ArtifactContent body, bool noResponseReceived, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Index = index;
            Protocol = protocol;
            Version = version;
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            if (headers != null)
            {
                Headers = new Dictionary<string, string>(headers);
            }

            if (body != null)
            {
                Body = new ArtifactContent(body);
            }

            NoResponseReceived = noResponseReceived;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}