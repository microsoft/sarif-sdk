// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Multitool.Rules
{
    /// <summary>
    /// A summary of the results expected from running a validation rule on
    /// a SARIF test file.
    /// </summary>
    /// <remarks>
    /// Each SARIF test file will include a property named expectedResults in the run's
    /// property bag. The test framework will compare the actual results with this property.
    /// </remarks>
    public class ExpectedValidationResults
    {
        /// <summary>
        /// The set of JSON pointers specifying the locations at which results were detected.
        /// </summary>
        [DataMember(Name = "resultLocationPointers", IsRequired = true)]
        public IEnumerable<string> ResultLocationPointers { get; set; }

        [JsonIgnore]
        public int ResultCount => ResultLocationPointers.Count();
    }
}
