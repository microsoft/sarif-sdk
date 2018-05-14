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
    /// A location visited by an analysis tool in the course of simulating or monitoring the execution of a program.
    /// </summary>
    [DataContract]
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
    public partial class CodeFlowLocation : PropertyBagHolder, ISarifNode
    {
        public static IEqualityComparer<CodeFlowLocation> ValueComparer => CodeFlowLocationEqualityComparer.Instance;

        public bool ValueEquals(CodeFlowLocation other) => ValueComparer.Equals(this, other);
        public int ValueGetHashCode() => ValueComparer.GetHashCode(this);

        /// <summary>
        /// Gets a value indicating the type of object implementing <see cref="ISarifNode" />.
        /// </summary>
        public SarifNodeKind SarifNodeKind
        {
            get
            {
                return SarifNodeKind.CodeFlowLocation;
            }
        }

        /// <summary>
        /// The 0-based sequence number of the location in the code flow within which it occurs.
        /// </summary>
        [DataMember(Name = "step", IsRequired = false, EmitDefaultValue = false)]
        public int Step { get; set; }

        /// <summary>
        /// The code location.
        /// </summary>
        [DataMember(Name = "location", IsRequired = false, EmitDefaultValue = false)]
        public Location Location { get; set; }

        /// <summary>
        /// The name of the module that contains the code that is executing.
        /// </summary>
        [DataMember(Name = "module", IsRequired = false, EmitDefaultValue = false)]
        public string Module { get; set; }

        /// <summary>
        /// A dictionary, each of whose keys specifies a variable or expression, the associated value of which represents the variable or expression value. For an annotation of kind 'continuation', for example, this dictionary might hold the current assumed values of a set of global variables.
        /// </summary>
        [DataMember(Name = "state", IsRequired = false, EmitDefaultValue = false)]
        public object State { get; set; }

        /// <summary>
        /// An integer representing a containment hierarchy within the thread flow
        /// </summary>
        [DataMember(Name = "nestingLevel", IsRequired = false, EmitDefaultValue = false)]
        public int NestingLevel { get; set; }

        /// <summary>
        /// An integer representing the temporal order in which execution reached this location.
        /// </summary>
        [DataMember(Name = "executionOrder", IsRequired = false, EmitDefaultValue = false)]
        public int ExecutionOrder { get; set; }

        /// <summary>
        /// Specifies the importance of this location in understanding the code flow in which it occurs. The order from most to least important is "essential", "important", "unimportant". Default: "important".
        /// </summary>
        [DataMember(Name = "importance", IsRequired = false, EmitDefaultValue = false)]
        public CodeFlowLocationImportance Importance { get; set; }

        /// <summary>
        /// Key/value pairs that provide additional information about the code location.
        /// </summary>
        [DataMember(Name = "properties", IsRequired = false, EmitDefaultValue = false)]
        internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlowLocation" /> class.
        /// </summary>
        public CodeFlowLocation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlowLocation" /> class from the supplied values.
        /// </summary>
        /// <param name="step">
        /// An initialization value for the <see cref="P: Step" /> property.
        /// </param>
        /// <param name="location">
        /// An initialization value for the <see cref="P: Location" /> property.
        /// </param>
        /// <param name="module">
        /// An initialization value for the <see cref="P: Module" /> property.
        /// </param>
        /// <param name="state">
        /// An initialization value for the <see cref="P: State" /> property.
        /// </param>
        /// <param name="nestingLevel">
        /// An initialization value for the <see cref="P: NestingLevel" /> property.
        /// </param>
        /// <param name="executionOrder">
        /// An initialization value for the <see cref="P: ExecutionOrder" /> property.
        /// </param>
        /// <param name="importance">
        /// An initialization value for the <see cref="P: Importance" /> property.
        /// </param>
        /// <param name="properties">
        /// An initialization value for the <see cref="P: Properties" /> property.
        /// </param>
        public CodeFlowLocation(int step, Location location, string module, object state, int nestingLevel, int executionOrder, CodeFlowLocationImportance importance, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Init(step, location, module, state, nestingLevel, executionOrder, importance, properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeFlowLocation" /> class from the specified instance.
        /// </summary>
        /// <param name="other">
        /// The instance from which the new instance is to be initialized.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="other" /> is null.
        /// </exception>
        public CodeFlowLocation(CodeFlowLocation other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Init(other.Step, other.Location, other.Module, other.State, other.NestingLevel, other.ExecutionOrder, other.Importance, other.Properties);
        }

        ISarifNode ISarifNode.DeepClone()
        {
            return DeepCloneCore();
        }

        /// <summary>
        /// Creates a deep copy of this instance.
        /// </summary>
        public CodeFlowLocation DeepClone()
        {
            return (CodeFlowLocation)DeepCloneCore();
        }

        private ISarifNode DeepCloneCore()
        {
            return new CodeFlowLocation(this);
        }

        private void Init(int step, Location location, string module, object state, int nestingLevel, int executionOrder, CodeFlowLocationImportance importance, IDictionary<string, SerializedPropertyInfo> properties)
        {
            Step = step;
            if (location != null)
            {
                Location = new Location(location);
            }

            Module = module;
            State = state;
            NestingLevel = nestingLevel;
            ExecutionOrder = executionOrder;
            Importance = importance;
            if (properties != null)
            {
                Properties = new Dictionary<string, SerializedPropertyInfo>(properties);
            }
        }
    }
}