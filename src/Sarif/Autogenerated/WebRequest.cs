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
    /// Describes an HTTP request.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class WebRequest : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<WebRequest> ValueComparer => WebRequestEqualityComparer.Instance;

        public bool ValueEquals(WebRequest other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.WebRequest;
            }
        }

        /// <summary>
        /// The index within the run.webRequests array of the request object associated with this result.
        /// </summary>
        [DataMember(Name = "index", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual int Index { get; set; }

        /// <summary>
        /// The request protocol. Example: 'http'.
        /// </summary>
        [DataMember(Name = "protocol", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Protocol { get; set; }

        /// <summary>
        /// The request version. Example: '1.1'.
        /// </summary>
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Version { get; set; }

        /// <summary>
        /// The target of the request.
        /// </summary>
        [DataMember(Name = "target", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Target { get; set; }

        /// <summary>
        /// The HTTP method. Well-known values are 'GET', 'PUT', 'POST', 'DELETE', 'PATCH', 'HEAD', 'OPTIONS', 'TRACE', 'CONNECT'.
        /// </summary>
        [DataMember(Name = "method", IsRequired = false, EmitDefaultValue = false)]
        public virtual string Method { get; set; }

        /// <summary>
        /// The request headers.
        /// </summary>
        [DataMember(Name = "headers", IsRequired = false, EmitDefaultValue = false)]
        public virtual IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The request parameters.
        /// </summary>
        [DataMember(Name = "parameters", IsRequired = false, EmitDefaultValue = false)]
        public virtual IDictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// The body of the request.
        /// </summary>
        [DataMember(Name = "body", IsRequired = false, EmitDefaultValue = false)]
        public virtual ArtifactContent Body { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the request.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequest" /> class.
        /// </summary>
        public WebRequest()
        {
            Index = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequest" /> class from the supplied values.
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
        /// <param name="target">
        /// An initialization value for the <see cref="P:Target" /> property.
        /// </param>
        /// <param name="method">
        /// An initialization value for the <see cref="P:Method" /> property.
        /// </param>
        /// <param name="headers">
        /// An initialization value for the <see cref="P:Headers" /> property.
        /// </param>
        /// <param name="parameters">
        /// An initialization value for the <see cref="P:Parameters" /> property.
        /// </param>
        /// <param name="body">
        /// An initialization value for the <see cref="P:Body" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public WebRequest(int index, string protocol, string version, string target, string method, IDictionary<string, string> headers, IDictionary<string, string> parameters, ArtifactContent body, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(index, protocol, version, target, method, headers, parameters, body, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRequest" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public WebRequest(WebRequest other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Index, other.Protocol, other.Version, other.Target, other.Method, other.Headers, other.Parameters, other.Body, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual WebRequest DeepClone()
        {
            return (WebRequest)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new WebRequest(this);
        }

        protected virtual void Init(int index, string protocol, string version, string target, string method, IDictionary<string, string> headers, IDictionary<string, string> parameters, ArtifactContent body, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Index = index;
            Protocol = protocol;
            Version = version;
            Target = target;
            Method = method;
            if (headers != null)
            {
                Headers = new Dictionary<string, string>(headers);
            }

            if (parameters != null)
            {
                Parameters = new Dictionary<string, string>(parameters);
            }

            if (body != null)
            {
                Body = new ArtifactContent(body);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}