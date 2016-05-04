// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Describes a condition relevant to the tool itself, as opposed to being relevant to a target being analyzed by the tool.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public partial class Notification : ISarifNode
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
        /// The stable, unique identifier of the rule (if any) to which this notification is relevant.
        /// </summary>
        [DataMember(Name = "ruleId", IsRequired = false, EmitDefaultValue = false)]
        public string RuleId { get; set; }

        /// <summary>
        /// The analysis target (if any) to which this notification is relevant.
        /// </summary>
        [DataMember(Name = "analysisTarget", IsRequired = false, EmitDefaultValue = false)]
        public PhysicalLocation AnalysisTarget { get; set; }

        /// <summary>
        /// A string that describes the condition that was encountered.
        /// </summary>
        [DataMember(Name = "message", IsRequired = true)]
        public string Message { get; set; }

        /// <summary>
        /// A value specifying the severity level of the notification.
        /// </summary>
        [DataMember(Name = "level", IsRequired = false, EmitDefaultValue = false)]
        public NotificationLevel Level { get; set; }

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
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// A set of distinct strings that provide additional information about the notification.
        /// </summary>
        [DataMember(Name = "tags", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Tags { get; set; }

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
        /// <param name="analysisTarget">
        /// An initialization value for the <see cref="P: AnalysisTarget" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="level">
        /// An initialization value for the <see cref="P: Level" /> property.
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
        /// <param name="tags">
        /// An initialization value for the <see cref="P: Tags" /> property.
        /// </param>
        public Notification(string id, string ruleId, PhysicalLocation analysisTarget, string message, NotificationLevel level, DateTime time, ExceptionData exception, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Init(id, ruleId, analysisTarget, message, level, time, exception, properties, tags);
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

            Init(other.Id, other.RuleId, other.AnalysisTarget, other.Message, other.Level, other.Time, other.Exception, other.Properties, other.Tags);
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

        private void Init(string id, string ruleId, PhysicalLocation analysisTarget, string message, NotificationLevel level, DateTime time, ExceptionData exception, IDictionary<string, string> properties, IEnumerable<string> tags)
        {
            Id = id;
            RuleId = ruleId;
            if (analysisTarget != null)
            {
                AnalysisTarget = new PhysicalLocation(analysisTarget);
            }

            Message = message;
            Level = level;
            Time = time;
            if (exception != null)
            {
                Exception = new ExceptionData(exception);
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, string>(properties);
            }

            if (tags != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in tags)
                {
                    destination_0.Add(value_0);
                }

                Tags = destination_0;
            }
        }
    }
}