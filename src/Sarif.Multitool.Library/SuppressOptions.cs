// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using CommandLine;

using Microsoft.CodeAnalysis.Sarif.Driver;

namespace Microsoft.CodeAnalysis.Sarif.Multitool
{
    [Verb("suppress", HelpText = "Enrich a SARIF file with additional data.")]
    public class SuppressOptions : SingleFileOptionsBase
    {
        [Option(
            "justification",
            HelpText = "A string that provides the rationale for the suppressions",
            Required = true)]
        public string Justification { get; set; }

        [Option(
            "alias",
            HelpText = "The account name associated with the suppression.")]
        public string Alias { get; set; }

        [Option(
            "guids",
            HelpText = "A UUID that will be associated with a suppression.")]
        public bool Guids { get; set; }

        [Option(            
            "results-guids",
            HelpText = "Guid(s) to SARIF log result(s) comprising the current result guid, without result matching information",
            Default = null)]
        public IEnumerable<string> ResultsGuids { get; set; }

        [Option(
            'e',
            "expression",
            HelpText = "Result Expression to Evaluate (ex: (BaselineState != 'Unchanged'))")]
        public string Expression { get; set; }

        [Option(
            "timestamps",
            HelpText = "The property 'timeUtc' that will be associated with a suppression.")]
        public bool Timestamps { get; set; }

        [Option(
            "expiryInDays",
            HelpText = "The property 'expiryUtc' that will be associated with a suppression from the 'timeUtc'.")]
        public int ExpiryInDays { get; set; }

        [Option(
            "status",
            HelpText = "The status that will be used in the suppression. Valid values include Accepted and UnderReview.")]
        public SuppressionStatus Status { get; set; }
    }
}
