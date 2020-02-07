// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.VersionOne
{
    /// <summary>
    /// An annotation used to express code flows through a method or other locations that are related to a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.52.0.0")]
    public partial class AnnotatedCodeLocationVersionOne : PropertyBagHolder, ISarifNodeVersionOne
    {
        public static IEqualityComparer<AnnotatedCodeLocationVersionOne> ValueComparer => AnnotatedCodeLocationVersionOneEqualityComparer.Instance;

        public bool ValueEquals(AnnotatedCodeLocationVersionOne other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNodeVersionOne" />.
        /// </summary>
        public SarifNodeKindVersionOne SarifNodeKindVersionOne
        {
            get
            {
                return SarifNodeKindVersionOne.AnnotatedCodeLocationVersionOne;
            }
        }

        /// <summary>
        /// OBSOLETE (use "step" instead): An identifier for the location, unique within the scope of the code flow within which it occurs.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.VersionOne.Readers.AnnotatedCodeLocationIdConverterVersionOne))]
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int Id { get; set; }

        /// <summary>
        /// The 0-based sequence number of the location in the code flow within which it occurs.
        /// </summary>
        [DataMember(Name = "step", IsRequired = false, EmitDefaultValue = false)]
        public int Step { get; set; }

        /// <summary>
        /// A file location to which this annotation refers.
        /// </summary>
        [DataMember(Name = "physicalLocation", IsRequired = false, EmitDefaultValue = false)]
        public PhysicalLocationVersionOne PhysicalLocation { get; set; }

        /// <summary>
        /// The fully qualified name of the method or function that is executing.
        /// </summary>
        [DataMember(Name = "fullyQualifiedLogicalName", IsRequired = false, EmitDefaultValue = false)]
        public string FullyQualifiedLogicalName { get; set; }

        /// <summary>
        /// A key used to retrieve the annotation's logicalLocation from the logicalLocations dictionary.
        /// </summary>
        [DataMember(Name = "logicalLocationKey", IsRequired = false, EmitDefaultValue = false)]
        public string LogicalLocationKey { get; set; }

        /// <summary>
        /// The name of the module that contains the code that is executing.
        /// </summary>
        [DataMember(Name = "module", IsRequired = false, EmitDefaultValue = false)]
        public string Module { get; set; }

        /// <summary>
        /// The thread identifier of the code that is executing.
        /// </summary>
        [DataMember(Name = "threadId", IsRequired = false, EmitDefaultValue = false)]
        public int ThreadId { get; set; }

        /// <summary>
        /// A message relevant to this annotation.
        /// </summary>
        [DataMember(Name = "message", IsRequired = false, EmitDefaultValue = false)]
        public string Message { get; set; }

        /// <summary>
        /// Categorizes the location.
        /// </summary>
        [DataMember(Name = "kind", IsRequired = false, EmitDefaultValue = false)]
        public AnnotatedCodeLocationKindVersionOne Kind { get; set; }

        /// <summary>
        /// Classifies state transitions in code locations relevant to a taint analysis.
        /// </summary>
        [DataMember(Name = "taintKind", IsRequired = false, EmitDefaultValue = false)]
        public TaintKindVersionOne TaintKind { get; set; }

        /// <summary>
        /// The fully qualified name of the target on which this location operates. For an annotation of kind 'call', for example, the target refers to the fully qualified logical name of the function called from this location.
        /// </summary>
        [DataMember(Name = "target", IsRequired = false, EmitDefaultValue = false)]
        public string Target { get; set; }

        /// <summary>
        /// An ordered set of strings that comprise input or return values for the current operation. For an annotation of kind 'call', for example, this property may hold the ordered list of arguments passed to the callee.
        /// </summary>
        [DataMember(Name = "values", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Values { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys specifies a variable or expression, the associated value of which represents the variable or expression value. For an annotation of kind 'continuation', for example, this dictionary might hold the current assumed values of a set of global variables.
        /// </summary>
        [DataMember(Name = "state", IsRequired = false, EmitDefaultValue = false)]
        public IDictionary<string, string> State { get; set; }

        /// <summary>
        /// A key used to retrieve the target's logicalLocation from the logicalLocations dictionary.
        /// </summary>
        [DataMember(Name = "targetKey", IsRequired = false, EmitDefaultValue = false)]
        public string TargetKey { get; set; }

        /// <summary>
        /// OBSOLETE (use "importance" instead): True if this location is essential to understanding the code flow in which it occurs.
        /// </summary>
        [DataMember(Name = "essential", IsRequired = false, EmitDefaultValue = false)]
        [JsonProperty("essential", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Essential { get; set; }

        /// <summary>
        /// Specifies the importance of this location in understanding the code flow in which it occurs. The order from most to least important is "essential", "important", "unimportant". Default: "important".
        /// </summary>
        [DataMember(Name = "importance", IsRequired = false, EmitDefaultValue = false)]
        public AnnotatedCodeLocationImportanceVersionOne Importance { get; set; }

        /// <summary>
        /// The source code at the specified location.
        /// </summary>
        [DataMember(Name = "snippet", IsRequired = false, EmitDefaultValue = false)]
        public string Snippet { get; set; }

        /// <summary>
        /// A set of messages relevant to the current annotated code location.
        /// </summary>
        [DataMember(Name = "annotations", IsRequired = false, EmitDefaultValue = false)]
        public IList<AnnotationVersionOne> Annotations { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the code location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocationVersionOne" /> class.
        /// </summary>
        public AnnotatedCodeLocationVersionOne()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocationVersionOne" /> class from the supplied values.
        /// </summary>
        /// <param name="id">
        /// An initialization value for the <see cref="P: Id" /> property.
        /// </param>
        /// <param name="step">
        /// An initialization value for the <see cref="P: Step" /> property.
        /// </param>
        /// <param name="physicalLocation">
        /// An initialization value for the <see cref="P: PhysicalLocation" /> property.
        /// </param>
        /// <param name="fullyQualifiedLogicalName">
        /// An initialization value for the <see cref="P: FullyQualifiedLogicalName" /> property.
        /// </param>
        /// <param name="logicalLocationKey">
        /// An initialization value for the <see cref="P: LogicalLocationKey" /> property.
        /// </param>
        /// <param name="module">
        /// An initialization value for the <see cref="P: Module" /> property.
        /// </param>
        /// <param name="threadId">
        /// An initialization value for the <see cref="P: ThreadId" /> property.
        /// </param>
        /// <param name="message">
        /// An initialization value for the <see cref="P: Message" /> property.
        /// </param>
        /// <param name="kind">
        /// An initialization value for the <see cref="P: Kind" /> property.
        /// </param>
        /// <param name="taintKind">
        /// An initialization value for the <see cref="P: TaintKind" /> property.
        /// </param>
        /// <param name="target">
        /// An initialization value for the <see cref="P: Target" /> property.
        /// </param>
        /// <param name="values">
        /// An initialization value for the <see cref="P: Values" /> property.
        /// </param>
        /// <param name="state">
        /// An initialization value for the <see cref="P: State" /> property.
        /// </param>
        /// <param name="targetKey">
        /// An initialization value for the <see cref="P: TargetKey" /> property.
        /// </param>
        /// <param name="essential">
        /// An initialization value for the <see cref="P: Essential" /> property.
        /// </param>
        /// <param name="importance">
        /// An initialization value for the <see cref="P: Importance" /> property.
        /// </param>
        /// <param name="snippet">
        /// An initialization value for the <see cref="P: Snippet" /> property.
        /// </param>
        /// <param name="annotations">
        /// An initialization value for the <see cref="P: Annotations" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public AnnotatedCodeLocationVersionOne(int id, int step, PhysicalLocationVersionOne physicalLocation, string fullyQualifiedLogicalName, string logicalLocationKey, string module, int threadId, string message, AnnotatedCodeLocationKindVersionOne kind, TaintKindVersionOne taintKind, string target, IEnumerable<string> values, IDictionary<string, string> state, string targetKey, bool essential, AnnotatedCodeLocationImportanceVersionOne importance, string snippet, IEnumerable<AnnotationVersionOne> annotations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, step, physicalLocation, fullyQualifiedLogicalName, logicalLocationKey, module, threadId, message, kind, taintKind, target, values, state, targetKey, essential, importance, snippet, annotations, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocationVersionOne" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public AnnotatedCodeLocationVersionOne(AnnotatedCodeLocationVersionOne other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.Step, other.PhysicalLocation, other.FullyQualifiedLogicalName, other.LogicalLocationKey, other.Module, other.ThreadId, other.Message, other.Kind, other.TaintKind, other.Target, other.Values, other.State, other.TargetKey, other.Essential, other.Importance, other.Snippet, other.Annotations, other.Properties);
        }

        ISarifNodeVersionOne ISarifNodeVersionOne.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public AnnotatedCodeLocationVersionOne DeepClone()
        {
            return (AnnotatedCodeLocationVersionOne)DeepCloneCore();
        }

        private ISarifNodeVersionOne DeepCloneCore()
        {
            return new AnnotatedCodeLocationVersionOne(this);
        }

        private void Init(int id, int step, PhysicalLocationVersionOne physicalLocation, string fullyQualifiedLogicalName, string logicalLocationKey, string module, int threadId, string message, AnnotatedCodeLocationKindVersionOne kind, TaintKindVersionOne taintKind, string target, IEnumerable<string> values, IDictionary<string, string> state, string targetKey, bool essential, AnnotatedCodeLocationImportanceVersionOne importance, string snippet, IEnumerable<AnnotationVersionOne> annotations, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            Step = step;
            if (physicalLocation != null)
            {
                PhysicalLocation = new PhysicalLocationVersionOne(physicalLocation);
            }

            FullyQualifiedLogicalName = fullyQualifiedLogicalName;
            LogicalLocationKey = logicalLocationKey;
            Module = module;
            ThreadId = threadId;
            Message = message;
            Kind = kind;
            TaintKind = taintKind;
            Target = target;
            if (values != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in values)
                {
                    destination_0.Add(value_0);
                }

                Values = destination_0;
            }

            if (state != null)
            {
                State = new Dictionary<string, string>(state);
            }
            TargetKey = targetKey;
            Essential = essential;
            Importance = importance;
            Snippet = snippet;
            if (annotations != null)
            {
                var destination_1 = new List<AnnotationVersionOne>();
                foreach (var value_1 in annotations)
                {
                    if (value_1 == null)
                    {
                        destination_1.Add(null);
                    }
                    else
                    {
                        destination_1.Add(new AnnotationVersionOne(value_1));
                    }
                }

                Annotations = destination_1;
            }

            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}