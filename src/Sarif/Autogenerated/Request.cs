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
    /// A web request object.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class Request : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Request> ValueComparer => RequestEqualityComparer.Instance;

        public bool ValueEquals(Request other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Request;
            }
        }

        /// <summary>
        /// The index within the run.requests array of the request object associated with this result.
        /// </summary>
        [DataMember(Name = "index", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(-1)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int Index { get; set; }

        /// <summary>
        /// The request protocol. Example: 'http'.
        /// </summary>
        [DataMember(Name = "protocol", IsRequired = false, EmitDefaultValue = false)]
        public string Protocol { get; set; }

        /// <summary>
        /// The request version. Example: '1.1'.
        /// </summary>
        [DataMember(Name = "version", IsRequired = false, EmitDefaultValue = false)]
        public string Version { get; set; }

        /// <summary>
        /// The target of the request.
        /// </summary>
        [DataMember(Name = "target", IsRequired = false, EmitDefaultValue = false)]
        public string Target { get; set; }

        /// <summary>
        /// The HTTP method. Well-known values are 'GET', 'PUT', 'POST', 'DELETE', 'PATCH', 'HEAD', 'OPTIONS', 'TRACE', 'CONNECT'.
        /// </summary>
        [DataMember(Name = "method", IsRequired = false, EmitDefaultValue = false)]
        public string Method { get; set; }

        /// <summary>
        /// The request headers.
        /// </summary>
        [DataMember(Name = "headers", IsRequired = false, EmitDefaultValue = false)]
        public object Headers { get; set; }

        /// <summary>
        /// The request parameters.
        /// </summary>
        [DataMember(Name = "parameters", IsRequired = false, EmitDefaultValue = false)]
        public object Parameters { get; set; }

        /// <summary>
        /// The body of the request.
        /// </summary>
        [DataMember(Name = "body", IsRequired = false, EmitDefaultValue = false)]
        public ArtifactContent Body { get; set; }

        /// <summary>
        /// Specifies whether a response was received for the request.
        /// </summary>
        [DataMember(Name = "noResponseReceived", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool NoResponseReceived { get; set; }

        /// <summary>
        /// The reason for the request failure.
        /// </summary>
        [DataMember(Name = "failureReason", IsRequired = false, EmitDefaultValue = false)]
        public string FailureReason { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the request.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request" /> class.
        /// </summary>
        public Request()
        {
            Index = -1;
            NoResponseReceived = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request" /> class from the supplied values.
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
        /// <param name="noResponseReceived">
        /// An initialization value for the <see cref="P:NoResponseReceived" /> property.
        /// </param>
        /// <param name="failureReason">
        /// An initialization value for the <see cref="P:FailureReason" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Request(int index, string protocol, string version, string target, string method, object headers, object parameters, ArtifactContent body, bool noResponseReceived, string failureReason, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(index, protocol, version, target, method, headers, parameters, body, noResponseReceived, failureReason, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Request(Request other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Index, other.Protocol, other.Version, other.Target, other.Method, other.Headers, other.Parameters, other.Body, other.NoResponseReceived, other.FailureReason, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Request DeepClone()
        {
            return (Request)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Request(this);
        }

        private void Init(int index, string protocol, string version, string target, string method, object headers, object parameters, ArtifactContent body, bool noResponseReceived, string failureReason, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Index = index;
            Protocol = protocol;
            Version = version;
            Target = target;
            Method = method;
            Headers = headers;
            Parameters = parameters;
            if (body != null)
            {
                Body = new ArtifactContent(body);
            }

            NoResponseReceived = noResponseReceived;
            FailureReason = failureReason;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}