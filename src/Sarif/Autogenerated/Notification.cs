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
    /// Describes a condition relevant to the tool itself, as opposed to being relevant to a target being analyzed by the tool.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public partial class Notification : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Notification> ValueComparer => NotificationEqualityComparer.Instance;

        public bool ValueEquals(Notification other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public virtual SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Notification;
            }
        }

        /// <summary>
        /// The locations relevant to this notification.
        /// </summary>
        [DataMember(Name = "locations", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public virtual IList<Location> Locations { get; set; }

        /// <summary>
        /// A message that describes the condition that was encountered.
        /// </summary>
        [DataMember(Name = "message", IsRequired = true)]
        public virtual Message Message { get; set; }

        /// <summary>
        /// A value specifying the severity level of the notification.
        /// </summary>
        [DataMember(Name = "level", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(FailureLevel.Warning)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.EnumConverter))]
        public virtual FailureLevel Level { get; set; }

        /// <summary>
        /// The thread identifier of the code that generated the notification.
        /// </summary>
        [DataMember(Name = "threadId", IsRequired = false, EmitDefaultValue = false)]
        public virtual int ThreadId { get; set; }

        /// <summary>
        /// The Coordinated Universal Time (UTC) date and time at which the analysis tool generated the notification.
        /// </summary>
        [DataMember(Name = "timeUtc", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.DateTimeConverter))]
        public virtual DateTime TimeUtc { get; set; }

        /// <summary>
        /// The runtime exception, if any, relevant to this notification.
        /// </summary>
        [DataMember(Name = "exception", IsRequired = false, EmitDefaultValue = false)]
        public virtual ExceptionData Exception { get; set; }

        /// <summary>
        /// A reference used to locate the descriptor relevant to this notification.
        /// </summary>
        [DataMember(Name = "descriptor", IsRequired = false, EmitDefaultValue = false)]
        public virtual ReportingDescriptorReference Descriptor { get; set; }

        /// <summary>
        /// A reference used to locate the rule descriptor associated with this notification.
        /// </summary>
        [DataMember(Name = "associatedRule", IsRequired = false, EmitDefaultValue = false)]
        public virtual ReportingDescriptorReference AssociatedRule { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the notification.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Notification" /> class.
        /// </summary>
        public Notification()
        {
            Level = FailureLevel.Warning;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Notification" /> class from the supplied values.
        /// </summary>
        /// <param name="locations">
        /// An initialization value for the <see cref="P:Locations" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P:Message" /> property.
        /// </param>
        /// <param name="level">
        /// An initialization value for the <see cref="P:Level" /> property.
        /// </param>
        /// <param name="threadId">
        /// An initialization value for the <see cref="P:ThreadId" /> property.
        /// </param>
        /// <param name="timeUtc">
        /// An initialization value for the <see cref="P:TimeUtc" /> property.
        /// </param>
        /// <param name="exception">
        /// An initialization value for the <see cref="P:Exception" /> property.
        /// </param>
        /// <param name="descriptor">
        /// An initialization value for the <see cref="P:Descriptor" /> property.
        /// </param>
        /// <param name="associatedRule">
        /// An initialization value for the <see cref="P:AssociatedRule" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Notification(IEnumerable<Location> locations, Message message, FailureLevel level, int threadId, DateTime timeUtc, ExceptionData exception, ReportingDescriptorReference descriptor, ReportingDescriptorReference associatedRule, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(locations, message, level, threadId, timeUtc, exception, descriptor, associatedRule, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Notification" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public Notification(Notification other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Locations, other.Message, other.Level, other.ThreadId, other.TimeUtc, other.Exception, other.Descriptor, other.AssociatedRule, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public virtual Notification DeepClone()
        {
            return (Notification)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Notification(this);
        }

        protected virtual void Init(IEnumerable<Location> locations, Message message, FailureLevel level, int threadId, DateTime timeUtc, ExceptionData exception, ReportingDescriptorReference descriptor, ReportingDescriptorReference associatedRule, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (locations != null)
            {
                var destination_0 = new List<Location>();
                foreach (var value_0 in locations)
                {
                    if (value_0 == null)
                    {
                        destination_0.Add(null);
                    }
                    else
                    {
                        destination_0.Add(new Location(value_0));
                    }
                }

                Locations = destination_0;
            }

            if (message != null)
            {
                Message = new Message(message);
            }

            Level = level;
            ThreadId = threadId;
            TimeUtc = timeUtc;
            if (exception != null)
            {
                Exception = new ExceptionData(exception);
            }

            if (descriptor != null)
            {
                Descriptor = new ReportingDescriptorReference(descriptor);
            }

            if (associatedRule != null)
            {
                AssociatedRule = new ReportingDescriptorReference(associatedRule);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}