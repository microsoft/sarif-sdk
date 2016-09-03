// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// An annotation used to express code flows through a method or other locations that are related to a result.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class AnnotatedCodeLocation : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<AnnotatedCodeLocation> ValueComparer => AnnotatedCodeLocationEqualityComparer.Instance;

        public bool ValueEquals(AnnotatedCodeLocation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.AnnotatedCodeLocation;
            }
        }

        /// <summary>
        /// OBSOLETE (use "step" instead): An identifier for the location, unique within the scope of the code flow within which it occurs.
        /// </summary>
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        [JsonConverter(typeof(Microsoft.CodeAnalysis.Sarif.Readers.AnnotatedCodeLocationIdConverter))]
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
        public PhysicalLocation PhysicalLocation { get; set; }

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
        public AnnotatedCodeLocationKind Kind { get; set; }

        /// <summary>
        /// Classifies state transitions in code locations relevant to a taint analysis.
        /// </summary>
        [DataMember(Name = "taintKind", IsRequired = false, EmitDefaultValue = false)]
        public TaintKind TaintKind { get; set; }

        /// <summary>
        /// The fully qualified name of the target on which this location operates. For an annotation of kind 'call', for example, the target refers to the fully qualified logical name of the function called from this location.
        /// </summary>
        [DataMember(Name = "target", IsRequired = false, EmitDefaultValue = false)]
        public string Target { get; set; }

        /// <summary>
        /// An ordered set of strings that parameterize the operation for this location. For an annotation of kind 'call', for example, this property may hold the ordered list of arguments passed to the callee.
        /// </summary>
        [DataMember(Name = "parameters", IsRequired = false, EmitDefaultValue = false)]
        public IList<string> Parameters { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys specifies a variable name, the associated value of which represents the variable value. For an annotation of kind 'continuation', for example, this dictionary might hold the current assumed values of a set of global variables.
        /// </summary>
        [DataMember(Name = "variables", IsRequired = false, EmitDefaultValue = false)]
        public object Variables { get; set; }

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
        public AnnotatedCodeLocationImportance Importance { get; set; }

        /// <summary>
        /// The source code at the specified location.
        /// </summary>
        [DataMember(Name = "snippet", IsRequired = false, EmitDefaultValue = false)]
        public string Snippet { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the code location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocation" /> class.
        /// </summary>
        public AnnotatedCodeLocation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocation" /> class from the supplied values.
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
        /// <param name="parameters">
        /// An initialization value for the <see cref="P: Parameters" /> property.
        /// </param>
        /// <param name="variables">
        /// An initialization value for the <see cref="P: Variables" /> property.
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
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public AnnotatedCodeLocation(int id, int step, PhysicalLocation physicalLocation, string fullyQualifiedLogicalName, string logicalLocationKey, string module, int threadId, string message, AnnotatedCodeLocationKind kind, TaintKind taintKind, string target, IEnumerable<string> parameters, object variables, string targetKey, bool essential, AnnotatedCodeLocationImportance importance, string snippet, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(id, step, physicalLocation, fullyQualifiedLogicalName, logicalLocationKey, module, threadId, message, kind, taintKind, target, parameters, variables, targetKey, essential, importance, snippet, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotatedCodeLocation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public AnnotatedCodeLocation(AnnotatedCodeLocation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Id, other.Step, other.PhysicalLocation, other.FullyQualifiedLogicalName, other.LogicalLocationKey, other.Module, other.ThreadId, other.Message, other.Kind, other.TaintKind, other.Target, other.Parameters, other.Variables, other.TargetKey, other.Essential, other.Importance, other.Snippet, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public AnnotatedCodeLocation DeepClone()
        {
            return (AnnotatedCodeLocation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new AnnotatedCodeLocation(this);
        }

        private void Init(int id, int step, PhysicalLocation physicalLocation, string fullyQualifiedLogicalName, string logicalLocationKey, string module, int threadId, string message, AnnotatedCodeLocationKind kind, TaintKind taintKind, string target, IEnumerable<string> parameters, object variables, string targetKey, bool essential, AnnotatedCodeLocationImportance importance, string snippet, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Id = id;
            Step = step;
            if (physicalLocation != null)
            {
                PhysicalLocation = new PhysicalLocation(physicalLocation);
            }

            FullyQualifiedLogicalName = fullyQualifiedLogicalName;
            LogicalLocationKey = logicalLocationKey;
            Module = module;
            ThreadId = threadId;
            Message = message;
            Kind = kind;
            TaintKind = taintKind;
            Target = target;
            if (parameters != null)
            {
                var destination_0 = new List<string>();
                foreach (var value_0 in parameters)
                {
                    destination_0.Add(value_0);
                }

                Parameters = destination_0;
            }

            Variables = variables;
            TargetKey = targetKey;
            Essential = essential;
            Importance = importance;
            Snippet = snippet;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}