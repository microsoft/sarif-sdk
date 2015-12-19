// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Sdk
{
    public interface IRuleDescriptor
    {
        /// <summary>
        /// A string that contains a stable, opaque identifier for a rule.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// An optional string that contains a rule identifier that is understandable to an end user.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A URI where the primary documentation for the rule can be found. 
        /// </summary>
        Uri HelpUri { get; }

        /// <summary>
        /// A string that contains a concise description of the rule. The short description property
        /// should be a single sentence that is understandable when displayed in user interface contexts
        /// where the available space is limited to a single line of text.
        /// </summary>
        string ShortDescription { get; }

        /// <summary>
        /// A string whose value is a string that describes the rule. The fullDescription property should,
        /// as far as possible, provide details sufficient to enable resolution of any problem indicated
        /// by the result.
        /// </summary>
        string FullDescription { get; }

        /// <summary>
        /// A dictionary consisting of a set of name/value pairs with arbitrary names. The options
        /// objects shall describe the set of configurable options supported by the rule. The value
        /// within each name/value pair shall be a string, which may be the empty string. The value
        /// shall not be a dictionary or sub-object.
        /// </summary>
        global::System.Collections.Generic.Dictionary<string, string> Options { get; }

        /// <summary>
        /// A dictionary consisting of a set of name/value pairs with arbitrary names. The value
        /// within each name/value pair shall be a string that can be passed to a string formatting
        /// function (e.g., the C language printf function) to construct a formatted message in
        /// combination with an arbitrary number of additional function arguments.
        /// </summary>
        global::System.Collections.Generic.Dictionary<string, string> FormatSpecifiers { get; }

        /// <summary>
        /// A dictionary consisting of a set of name/value pairs with arbitrary names. This
        /// allows tools to include information about the rule that is not explicitly specified
        /// in the SARIF format. The value within each name/value pair shall be a string,
        /// which may be the empty string. The value shall not be a dictionary or sub-object.
        /// </summary>
        global::System.Collections.Generic.Dictionary<string, string> Properties { get; }
    }
}
