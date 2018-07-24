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
    /// Describes a condition relevant to the tool itself, as opposed to being relevant to a target being analyzed by the tool.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.56.0.0")]
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
        /// An identifier for the condition that was encountered.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// The stable, unique identifier of the rule (if any) to which this notification is relevant. If 'ruleKey' is not specified, this member can be used to retrieve rule metadata from the rules dictionary, if it exists.
        /// </summary>
        [DataMember(Name = "ruleId", IsRequired = false, EmitDefaultValue = false)]
        public string RuleId { get; set; }

        /// <summary>
        /// The file and region relevant to this notification.
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
        public NotificationLevel Level { get; set; }

        /// <summary>
        /// The thread identifier of the code that generated the notification.
        /// </summary>
        [DataMember(Name = "threadId", IsRequired = false, EmitDefaultValue = false)]
        public int ThreadId { get; set; }

        /// <summary>
        /// The date and time at which the analysis tool generated the notification.
        /// </summary>
        [DataMember(Name = "time", IsRequired = false, EmitDefaultValue = false)]
        public DateTime Time { get; set; }

        /// <summary>
        /// The runtime exception, if any, relevant to this notification.
        /// </summary>
        [DataMember(Name = "exception", IsRequired = false, EmitDefaultValue = false)]
        public ExceptionData Exception { get; set; }

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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Notification" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="ruleId">
        /// An initialization value for the <see cref="P: RuleId" /> property.
        /// </param>
        /// <param name="physicalLocation">
        /// An initialization value for the <see cref="P: PhysicalLocation" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="level">
        /// An initialization value for the <see cref="P: Level" /> property.
        /// </param>
        /// <param name="threadId">
        /// An initialization value for the <see cref="P: ThreadId" /> property.
        /// </param>
        /// <param name="time">
        /// An initialization value for the <see cref="P: Time" /> property.
        /// </param>
        /// <param name="exception">
        /// An initialization value for the <see cref="P: Exception" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public Notification(string id, string ruleId, PhysicalLocation physicalLocation, Message message, NotificationLevel level, int threadId, DateTime time, ExceptionData exception, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, ruleId, physicalLocation, message, level, threadId, time, exception, properties);
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

            Init(other.Id, other.RuleId, other.PhysicalLocation, other.Message, other.Level, other.ThreadId, other.Time, other.Exception, other.Properties);
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

        private void Init(string id, string ruleId, PhysicalLocation physicalLocation, Message message, NotificationLevel level, int threadId, DateTime time, ExceptionData exception, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            RuleId = ruleId;
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
            Time = time;
            if (exception != null)
            {
                Exception = new ExceptionData(exception);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}