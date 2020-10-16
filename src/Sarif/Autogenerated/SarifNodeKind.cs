// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A set of values for all the types that implement <see cref="ISarifNode" />.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "1.1.0.0")]
    public enum SarifNodeKind
    {
        /// <summary>
        /// An uninitialized kind.
        /// </summary>
        None,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="SarifLog" />.
        /// </summary>
        SarifLog,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Address" />.
        /// </summary>
        Address,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Artifact" />.
        /// </summary>
        Artifact,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ArtifactChange" />.
        /// </summary>
        ArtifactChange,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ArtifactContent" />.
        /// </summary>
        ArtifactContent,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ArtifactLocation" />.
        /// </summary>
        ArtifactLocation,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Attachment" />.
        /// </summary>
        Attachment,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="CodeFlow" />.
        /// </summary>
        CodeFlow,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ConfigurationOverride" />.
        /// </summary>
        ConfigurationOverride,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Conversion" />.
        /// </summary>
        Conversion,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Edge" />.
        /// </summary>
        Edge,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="EdgeTraversal" />.
        /// </summary>
        EdgeTraversal,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ExceptionData" />.
        /// </summary>
        ExceptionData,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ExternalProperties" />.
        /// </summary>
        ExternalProperties,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ExternalPropertyFileReference" />.
        /// </summary>
        ExternalPropertyFileReference,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ExternalPropertyFileReferences" />.
        /// </summary>
        ExternalPropertyFileReferences,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Fix" />.
        /// </summary>
        Fix,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Graph" />.
        /// </summary>
        Graph,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="GraphTraversal" />.
        /// </summary>
        GraphTraversal,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Invocation" />.
        /// </summary>
        Invocation,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Location" />.
        /// </summary>
        Location,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="LocationRelationship" />.
        /// </summary>
        LocationRelationship,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="LogicalLocation" />.
        /// </summary>
        LogicalLocation,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Message" />.
        /// </summary>
        Message,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="MultiformatMessageString" />.
        /// </summary>
        MultiformatMessageString,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Node" />.
        /// </summary>
        Node,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Notification" />.
        /// </summary>
        Notification,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="PhysicalLocation" />.
        /// </summary>
        PhysicalLocation,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="PropertyBag" />.
        /// </summary>
        PropertyBag,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Rectangle" />.
        /// </summary>
        Rectangle,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Region" />.
        /// </summary>
        Region,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Replacement" />.
        /// </summary>
        Replacement,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ReportingDescriptor" />.
        /// </summary>
        ReportingDescriptor,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ReportingConfiguration" />.
        /// </summary>
        ReportingConfiguration,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ReportingDescriptorReference" />.
        /// </summary>
        ReportingDescriptorReference,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ReportingDescriptorRelationship" />.
        /// </summary>
        ReportingDescriptorRelationship,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Result" />.
        /// </summary>
        Result,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ResultProvenance" />.
        /// </summary>
        ResultProvenance,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Run" />.
        /// </summary>
        Run,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="RunAutomationDetails" />.
        /// </summary>
        RunAutomationDetails,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="SpecialLocations" />.
        /// </summary>
        SpecialLocations,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Stack" />.
        /// </summary>
        Stack,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="StackFrame" />.
        /// </summary>
        StackFrame,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Suppression" />.
        /// </summary>
        Suppression,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ThreadFlow" />.
        /// </summary>
        ThreadFlow,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ThreadFlowLocation" />.
        /// </summary>
        ThreadFlowLocation,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Tool" />.
        /// </summary>
        Tool,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ToolComponent" />.
        /// </summary>
        ToolComponent,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ToolComponentReference" />.
        /// </summary>
        ToolComponentReference,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="TranslationMetadata" />.
        /// </summary>
        TranslationMetadata,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="VersionControlDetails" />.
        /// </summary>
        VersionControlDetails,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="WebRequest" />.
        /// </summary>
        WebRequest,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="WebResponse" />.
        /// </summary>
        WebResponse
    }
}