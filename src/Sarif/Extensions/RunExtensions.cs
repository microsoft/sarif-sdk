// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Extensions
{
    public static class RunExtensions
    {
        public static bool HasResults(this Run run) => run.Results?.Count > 0;

        /// <summary>
        /// Returns a value indicating whether this run has any results whose baseline state
        /// is "absent".
        /// </summary>
        /// <param name="run">
        /// The <see cref="Run"/> whose results are to be examined.
        /// </param>
        /// <returns>
        /// <code>true</code> if <paramref name="run"/> has any absent results, otherwise
        /// <code>false</code>.
        /// </returns>
        /// <remarks>
        /// The SARIF spec states that the property <see cref="Result.BaselineState"/> must either
        /// be present on all results or on none of them. This requirement is intended to optimize
        /// performance of SARIF consumers such as results viewers, which (for example) need only
        /// examine the first result to decide whether to display a "Baseline state" column.
        /// Therefore if the first result has <see cref="BaselineState.None"/>, this method does
        /// not examine the rest of the results, and it returns <code>false</code>.
        /// </remarks>
        public static bool HasAbsentResults(this Run run) =>
            run.HasResults()
            && run.Results[0].BaselineState != BaselineState.None
            && run.Results.Any(r => r.BaselineState == BaselineState.Absent);

        /// <summary>
        /// Returns a value indicating whether this run has any suppressed results.
        /// </summary>
        /// <param name="run">
        /// The <see cref="Run"/> whose results are to be examined.
        /// </param>
        /// <returns>
        /// <code>true</code> if <paramref name="run"/> has any suppressed results, otherwise
        /// <code>false</code>.
        /// </returns>
        /// <remarks>
        /// The SARIF spec states that the property <see cref="Result.Suppressions"/> must either
        /// be present on all results or on none of them. This requirement is intended to optimize
        /// performance of SARIF consumers such as results viewers, which (for example) need only
        /// examine the first result to decide whether to display a "Suppressed" column. Therefore
        /// if the first result has a Suppressions value of null, this method does examine the rest
        /// of the results, and it returns <code>false</code>.
        /// </remarks>
        public static bool HasSuppressedResults(this Run run) =>
            run.HasResults()
            && run.Results[0].Suppressions != null
            && run.Results.Any(r => r.IsSuppressed());
    }
}
