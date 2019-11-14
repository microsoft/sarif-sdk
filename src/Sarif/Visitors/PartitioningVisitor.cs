// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    /// <summary>
    /// A visitor that partitions a specified SARIF log into a set of "partitioned logs."
    /// </summary>
    /// <remarks>
    /// Two results are placed in the same partitioned log if and only if a specified
    /// "partition function" returns the same value for both results, except that results for
    /// which the partition function returns null are discarded (not placed in any of the
    /// partitioned logs). Each partitioned log contains only those elements of run-level
    /// collections such as Run.Artifacts that are relevant to the subset of results in that log.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of the object returned from the partition function with which the visitor is
    /// constructed. It must be a reference type so that null is a valid value. It must override
    /// bool Equals(T other) so that two Ts can compare equal even if they are not reference equal.
    /// </typeparam>
    public class PartitioningVisitor<T> : SarifRewritingVisitor where T : class
    {
        /// <summary>
        /// A delegate for a function that returns a value specifying which partitioned log
        /// a specified result belongs in, or null if the result should be discarded (not
        /// placed in any of the partitioned logs).
        /// </summary>
        /// <param name="result">
        /// The result to be assigned to a partitioned log.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object returned from the partition function. It must be a reference
        /// type so that null is a valid value. It must override bool Equals(T other) so that
        /// two Ts can compare equal even if they are not reference equal.
        /// </typeparam>
        public delegate T PartitionFunction(Result result);

        private readonly PartitionFunction partitionFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitioningVisitor"/> class.
        /// </summary>
        /// <param name="partitionFunction">
        /// A delegate for a function that returns a value specifying which partitioned
        /// log each result belongs to.
        /// </param>
        public PartitioningVisitor(PartitionFunction partitionFunction)
        {
            this.partitionFunction = partitionFunction;
        }
    }
}
