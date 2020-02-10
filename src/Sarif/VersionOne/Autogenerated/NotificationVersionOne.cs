// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// Describes a condition relevant to the tool itself, as opposed to being relevant to a target being analyzed by the tool.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class NotificationVersionOne : PropertyBagHolder, ISarifNodeVersionOne
    {
        public static IEqualityComparer<NotificationVersionOne> ValueComparer => NotificationVersionOneEqualityComparer.Instance;

        public bool ValueEquals(NotificationVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.NotificationVersionOne;
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
        /// A key used to retrieve the rule metadata from the rules dictionary that is relevant to the notificationn.
        /// </summary>
        [DataMember(Name = "ruleKey", IsRequired = false, EmitDefaultValue = false)]
        public string RuleKey { get; set; }

        /// <summary>
        /// The file and region relevant to this notification.
        /// </summary>
        [DataMember(Name = "physicalLocation", IsRequired = false, EmitDefaultValue = false)]
        public PhysicalLocationVersionOne PhysicalLocation { get; set; }

        /// <summary>
        /// A string that describes the condition that was encountered.
        /// </summary>
        [DataMember(Name = "message", IsRequired = true)]
        public string Message { get; set; }

        /// <summary>
        /// A value specifying the severity level of the notification.
        /// </summary>
        [DataMember(Name = "level", IsRequired = false, EmitDefaultValue = false)]
        public NotificationLevelVersionOne Level { get; set; }

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
        public ExceptionDataVersionOne Exception { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the notification.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationVersionOne" /> class.
        /// </summary>
        public NotificationVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="ruleId">
        /// An initialization value for the <see cref="P: RuleId" /> property.
        /// </param>
        /// <param name="ruleKey">
        /// An initialization value for the <see cref="P: RuleKey" /> property.
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
        public NotificationVersionOne(string id, string ruleId, string ruleKey, PhysicalLocationVersionOne physicalLocation, string message, NotificationLevelVersionOne level, int threadId, DateTime time, ExceptionDataVersionOne exception, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, ruleId, ruleKey, physicalLocation, message, level, threadId, time, exception, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public NotificationVersionOne(NotificationVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.RuleId, other.RuleKey, other.PhysicalLocation, other.Message, other.Level, other.ThreadId, other.Time, other.Exception, other.Properties);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public NotificationVersionOne DeepClone()
        {
            return (NotificationVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new NotificationVersionOne(this);
        }

        private void Init(string id, string ruleId, string ruleKey, PhysicalLocationVersionOne physicalLocation, string message, NotificationLevelVersionOne level, int threadId, DateTime time, ExceptionDataVersionOne exception, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            RuleId = ruleId;
            RuleKey = ruleKey;
            if (physicalLocation != null)
            {
                PhysicalLocation = new PhysicalLocationVersionOne(physicalLocation);
            }

            Message = message;
            Level = level;
            ThreadId = threadId;
            Time = time;
            if (exception != null)
            {
                Exception = new ExceptionDataVersionOne(exception);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}