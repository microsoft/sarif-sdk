// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Readers;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// Interface exposed by objects that provide information about analysis rules.
    /// </summary>
    [GeneratedCode("Microsoft.Json.Schema.ToDotNet", "0.61.0.0")]
    public partial interface IRule
    {
        /// <summary>
        /// A stable, opaque identifier for the rule.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// An array of stable, opaque identifiers by which this rule was known in some previous version of the analysis tool.
        /// </summary>
        IList<string> DeprecatedIds { get; }

        /// <summary>
        /// A rule identifier that is understandable to an end user.
        /// </summary>
        Message Name { get; }

        /// <summary>
        /// A concise description of the rule. Should be a single sentence that is understandable when visible space is limited to a single line of text.
        /// </summary>
        Message ShortDescription { get; }

        /// <summary>
        /// A description of the rule. Should, as far as possible, provide details sufficient to enable resolution of any problem indicated by the result.
        /// </summary>
        Message FullDescription { get; }

        /// <summary>
        /// A set of name/value pairs with arbitrary names. The value within each name/value pair consists of plain text interspersed with placeholders, which can be used to construct a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        IDictionary<string, string> MessageStrings { get; }

        /// <summary>
        /// A set of name/value pairs with arbitrary names. The value within each name/value pair consists of rich text interspersed with placeholders, which can be used to construct a message in combination with an arbitrary number of additional string arguments.
        /// </summary>
        IDictionary<string, string> RichMessageStrings { get; }

        /// <summary>
        /// Information about the rule that can be configured at runtime.
        /// </summary>
        RuleConfiguration Configuration { get; }

        /// <summary>
        /// A URI where the primary documentation for the rule can be found.
        /// </summary>
        Uri HelpUri { get; }

        /// <summary>
        /// Provides the primary documentation for the rule, useful when there is no online documentation.
        /// </summary>
        Message Help { get; }
    }
}