// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A set of values for all the types that implement <see cref="ISarifNode" />.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.49.0.0")]
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
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Attachment" />.
        /// </summary>
        Attachment,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="CodeFlow" />.
        /// </summary>
        CodeFlow,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="CodeFlowLocation" />.
        /// </summary>
        CodeFlowLocation,
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
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="FileChange" />.
        /// </summary>
        FileChange,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="FileData" />.
        /// </summary>
        FileData,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="FileContent" />.
        /// </summary>
        FileContent,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="FileLocation" />.
        /// </summary>
        FileLocation,
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
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Hash" />.
        /// </summary>
        Hash,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Invocation" />.
        /// </summary>
        Invocation,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Location" />.
        /// </summary>
        Location,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="LogicalLocation" />.
        /// </summary>
        LogicalLocation,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Message" />.
        /// </summary>
        Message,
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
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Region" />.
        /// </summary>
        Region,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Replacement" />.
        /// </summary>
        Replacement,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Resources" />.
        /// </summary>
        Resources,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Result" />.
        /// </summary>
        Result,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Rule" />.
        /// </summary>
        Rule,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="RuleConfiguration" />.
        /// </summary>
        RuleConfiguration,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Run" />.
        /// </summary>
        Run,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Stack" />.
        /// </summary>
        Stack,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="StackFrame" />.
        /// </summary>
        StackFrame,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="ThreadFlow" />.
        /// </summary>
        ThreadFlow,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Tool" />.
        /// </summary>
        Tool,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="VersionControlDetails" />.
        /// </summary>
        VersionControlDetails
    }
}