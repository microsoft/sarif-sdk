// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Interface exposed by objects that provide information about analysis rules.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.22.0.0")]
    public interface IRule
    {
        /// <summary>
        /// A stable, opaque identifier for the rule.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// A rule identifier that is understandable to an end user.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A concise description of the rule. Should be a single sentence that is understandable when visible space is limited to a single line of text.
        /// </summary>
        string ShortDescription { get; }

        /// <summary>
        /// A string that describes the rule. Should, as far as possible, provide details sufficient to enable resolution of any problem indicated by the result.
        /// </summary>
        string FullDescription { get; }

        /// <summary>
        /// A set of name/value pairs with arbitrary names. The value within each name/value pair shall consist of plain text interspersed with placeholders, which can be used to format a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        IDictionary<string, string> MessageFormats { get; }

        /// <summary>
        /// A value specifying the default severity level of the notification.
        /// </summary>
        ResultLevel DefaultLevel { get; }

        /// <summary>
        /// A URI where the primary documentation for the rule can be found.
        /// </summary>
        Uri HelpUri { get; }

        /// <summary>
        /// Key/value pairs that provide additional information about the rule.
        /// </summary>
        IDictionary<string, string> Properties { get; }

        /// <summary>
        /// A set of distinct strings that provide additional information about the rule.
        /// </summary>
        IList<string> Tags { get; }
    }
}