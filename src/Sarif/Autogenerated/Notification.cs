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
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.62.0.0")]
    public partial class Notification : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<Notification> ValueComparer => NotificationEqualityComparer.Instance;

        public bool ValueEquals(Notification other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.Notification;
            }
        }

        /// <summary>
        /// The artifact and region relevant to this notification.
        /// </summary>
        [DataMember(Name = "physicalLocation", IsRequired = false, EmitDefaultValue = false)]
        public PhysicalLocation PhysicalLocation { get; set; }

        /// <summary>
        /// A message that describes the condition that was encountered.
        /// </summary>
        [DataMember(Name = "message", IsRequired = true)]
        public Message Message { get; set; }

        /// <summary>
        /// A value specifying the severity level of the notification.
        /// </summary>
        [DataMember(Name = "level", IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(FailureLevel.Warning)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.EnumConverter))]
        public FailureLevel Level { get; set; }

        /// <summary>
        /// The thread identifier of the code that generated the notification.
        /// </summary>
        [DataMember(Name = "threadId", IsRequired = false, EmitDefaultValue = false)]
        public int ThreadId { get; set; }

        /// <summary>
        /// The Coordinated Universal Time (UTC) date and time at which the analysis tool generated the notification.
        /// </summary>
        [DataMember(Name = "timeUtc", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.DateTimeConverter))]
        public DateTime TimeUtc { get; set; }

        /// <summary>
        /// The runtime exception, if any, relevant to this notification.
        /// </summary>
        [DataMember(Name = "exception", IsRequired = false, EmitDefaultValue = false)]
        public ExceptionData Exception { get; set; }

        /// <summary>
        /// A reference used to locate the descriptor relevant to this notification.
        /// </summary>
        [DataMember(Name = "notificationDescriptorReference", IsRequired = true)]
        public ReportingDescriptorReference NotificationDescriptorReference { get; set; }

        /// <summary>
        /// A reference used to locate the rule descriptor associated with this notification.
        /// </summary>
        [DataMember(Name = "associatedRuleDescriptorReference", IsRequired = false, EmitDefaultValue = false)]
        public ReportingDescriptorReference AssociatedRuleDescriptorReference { get; set; }

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
        /// <param name="physicalLocation">
        /// An initialization value for the <see cref="P:PhysicalLocation" /> property.
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
        /// <param name="notificationDescriptorReference">
        /// An initialization value for the <see cref="P:NotificationDescriptorReference" /> property.
        /// </param>
        /// <param name="associatedRuleDescriptorReference">
        /// An initialization value for the <see cref="P:AssociatedRuleDescriptorReference" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P:Properties" /> property.
        /// </param>
        public Notification(PhysicalLocation physicalLocation, Message message, FailureLevel level, int threadId, DateTime timeUtc, ExceptionData exception, ReportingDescriptorReference notificationDescriptorReference, ReportingDescriptorReference associatedRuleDescriptorReference, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(physicalLocation, message, level, threadId, timeUtc, exception, notificationDescriptorReference, associatedRuleDescriptorReference, properties);
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

            Init(other.PhysicalLocation, other.Message, other.Level, other.ThreadId, other.TimeUtc, other.Exception, other.NotificationDescriptorReference, other.AssociatedRuleDescriptorReference, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public Notification DeepClone()
        {
            return (Notification)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new Notification(this);
        }

        private void Init(PhysicalLocation physicalLocation, Message message, FailureLevel level, int threadId, DateTime timeUtc, ExceptionData exception, ReportingDescriptorReference notificationDescriptorReference, ReportingDescriptorReference associatedRuleDescriptorReference, IDictionary<string, SerializedPropertyInfo> properties)
        {
            if (physicalLocation != null)
            {
                PhysicalLocation = new PhysicalLocation(physicalLocation);
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

            if (notificationDescriptorReference != null)
            {
                NotificationDescriptorReference = new ReportingDescriptorReference(notificationDescriptorReference);
            }

            if (associatedRuleDescriptorReference != null)
            {
                AssociatedRuleDescriptorReference = new ReportingDescriptorReference(associatedRuleDescriptorReference);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}