// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.CodeDom.Compiler;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// A set of values for all the types that implement <see cref="ISarifNode" />.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
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
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="AnnotatedCodeLocation" />.
        /// </summary>
        AnnotatedCodeLocation,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="CodeFlow" />.
        /// </summary>
        CodeFlow,
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
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Fix" />.
        /// </summary>
        Fix,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="FormattedRuleMessage" />.
        /// </summary>
        FormattedRuleMessage,
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
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="LogicalLocationComponent" />.
        /// </summary>
        LogicalLocationComponent,
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
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Result" />.
        /// </summary>
        Result,
        /// <summary>
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Rule" />.
        /// </summary>
        Rule,
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
        /// A value indicating that the <see cref="ISarifNode" /> object is of type <see cref="Tool" />.
        /// </summary>
        Tool
    }
}