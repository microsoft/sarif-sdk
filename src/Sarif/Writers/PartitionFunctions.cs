// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    /// Provides ready-made <see cref="PartitionFunction{T}"/> implementations for the
    /// well-known <see cref="SplittingStrategy"/> values, plus an explicit address-list
    /// strategy ("PerIndexList") that lets a caller dictate exactly which results land
    /// in each output bucket.
    /// </summary>
    /// <remarks>
    /// All factory methods perform a single O(N) pre-walk of the log to compute a
    /// per-result partition key, then return an O(1) closure that the
    /// <see cref="PartitioningVisitor{T}"/> can call during its own traversal.
    /// Results are looked up by reference equality, which is safe because the visitor
    /// hands the same <see cref="Result"/> instances it walked to the partition function.
    /// </remarks>
    public static class PartitionFunctions
    {
        /// <summary>
        /// Auto-generated bucket name used when an explicit address-list spec does not
        /// itself name buckets. Buckets are numbered in spec order, starting at zero.
        /// </summary>
        public const string IndexListBucketPrefix = "bucket";

        /// <summary>
        /// Returns a <see cref="PartitionFunction{T}"/> implementing the specified
        /// <see cref="SplittingStrategy"/> against the given log.
        /// </summary>
        /// <param name="log">The SARIF log to be partitioned.</param>
        /// <param name="strategy">The splitting strategy to apply.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="strategy"/> is not supported by this helper.
        /// Use <see cref="ForIndexList"/> for <see cref="SplittingStrategy.PerIndexList"/>.
        /// </exception>
        public static PartitionFunction<string> ForStrategy(SarifLog log, SplittingStrategy strategy)
        {
            if (log == null) { throw new ArgumentNullException(nameof(log)); }

            var keyMap = new Dictionary<Result, string>(ResultReferenceComparer.Instance);

            if (log.Runs == null)
            {
                return _ => null;
            }

            int globalResultIndex = 0;

            for (int runIndex = 0; runIndex < log.Runs.Count; runIndex++)
            {
                Run run = log.Runs[runIndex];
                if (run?.Results == null) { continue; }

                for (int resultIndex = 0; resultIndex < run.Results.Count; resultIndex++)
                {
                    Result result = run.Results[resultIndex];
                    if (result == null) { continue; }

                    keyMap[result] = ComputeStrategyKey(strategy, run, result, runIndex, globalResultIndex);
                    globalResultIndex++;
                }
            }

            return result => (result != null && keyMap.TryGetValue(result, out string key)) ? key : null;
        }

        /// <summary>
        /// Returns a <see cref="PartitionFunction{T}"/> that places each result into a
        /// caller-specified bucket according to its address in the input log.
        /// </summary>
        /// <param name="log">The SARIF log to be partitioned.</param>
        /// <param name="indexSpec">
        /// A spec in the compact mini-language understood by <see cref="ParseIndexSpec"/>.
        /// </param>
        /// <param name="spilloverBucket">
        /// Optional bucket name to receive any result not addressed by
        /// <paramref name="indexSpec"/>. When null, unmatched results are discarded.
        /// </param>
        /// <param name="strictCoverage">
        /// When true, throws if any result in the log is not addressed by
        /// <paramref name="indexSpec"/> (and no <paramref name="spilloverBucket"/> is set).
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="log"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="indexSpec"/> is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when the spec cannot be parsed.</exception>
        public static PartitionFunction<string> ForIndexList(
            SarifLog log,
            string indexSpec,
            string spilloverBucket = null,
            bool strictCoverage = false)
        {
            if (log == null) { throw new ArgumentNullException(nameof(log)); }
            if (string.IsNullOrWhiteSpace(indexSpec))
            {
                throw new ArgumentException("Index spec must not be null or empty.", nameof(indexSpec));
            }

            IDictionary<ResultAddress, string> addressToBucket = ParseIndexSpec(indexSpec);

            ValidateAddressesAgainstLog(addressToBucket, log);

            var keyMap = new Dictionary<Result, string>(ResultReferenceComparer.Instance);
            int unmatchedCount = 0;

            if (log.Runs != null)
            {
                for (int runIndex = 0; runIndex < log.Runs.Count; runIndex++)
                {
                    Run run = log.Runs[runIndex];
                    if (run?.Results == null) { continue; }

                    for (int resultIndex = 0; resultIndex < run.Results.Count; resultIndex++)
                    {
                        Result result = run.Results[resultIndex];
                        if (result == null) { continue; }

                        var address = new ResultAddress(runIndex, resultIndex);
                        if (addressToBucket.TryGetValue(address, out string bucket))
                        {
                            keyMap[result] = bucket;
                        }
                        else if (spilloverBucket != null)
                        {
                            keyMap[result] = spilloverBucket;
                        }
                        else
                        {
                            unmatchedCount++;
                        }
                    }
                }
            }

            if (strictCoverage && unmatchedCount > 0)
            {
                throw new InvalidOperationException(
                    $"Strict coverage failed: {unmatchedCount} result(s) in the log were not addressed by the index spec and no spillover bucket was specified.");
            }

            return result => (result != null && keyMap.TryGetValue(result, out string key)) ? key : null;
        }

        /// <summary>
        /// Parses a partition spec written in the compact mini-language.
        /// </summary>
        /// <remarks>
        /// Grammar:
        /// <code>
        /// spec    ::= bucket ( "|" bucket )*
        /// bucket  ::= segment ( ";" segment )*
        /// segment ::= compactSeg | sarifUri
        /// compactSeg ::= [ runId ":" ] resultIdx ( "," resultIdx )*
        /// sarifUri   ::= "sarif:/runs/" runId "/results/" resultIdx
        /// </code>
        /// Buckets are auto-named "bucket0", "bucket1", ... in spec order. The
        /// <c>runId</c> prefix on a compact segment is optional; if omitted, run 0 is
        /// assumed (handy for the very common single-run AI workflow). Whitespace
        /// around any token is ignored.
        /// </remarks>
        /// <param name="spec">The spec string to parse.</param>
        /// <returns>
        /// A dictionary mapping each addressed <see cref="ResultAddress"/> to the
        /// auto-generated bucket name it belongs to.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="spec"/> is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when the spec is malformed or contains a duplicate address.</exception>
        public static IDictionary<ResultAddress, string> ParseIndexSpec(string spec)
        {
            if (string.IsNullOrWhiteSpace(spec))
            {
                throw new ArgumentException("Spec must not be null or empty.", nameof(spec));
            }

            var addressToBucket = new Dictionary<ResultAddress, string>();

            string[] bucketTexts = spec.Split('|');
            for (int b = 0; b < bucketTexts.Length; b++)
            {
                string bucketName = IndexListBucketPrefix + b.ToString(CultureInfo.InvariantCulture);
                string bucketText = bucketTexts[b].Trim();
                if (bucketText.Length == 0)
                {
                    throw new FormatException($"Empty bucket at position {b} in spec '{spec}'.");
                }

                foreach (string segmentRaw in bucketText.Split(';'))
                {
                    string segment = segmentRaw.Trim();
                    if (segment.Length == 0)
                    {
                        throw new FormatException($"Empty segment in bucket {b} of spec '{spec}'.");
                    }

                    foreach (ResultAddress address in ParseSegment(segment))
                    {
                        if (addressToBucket.ContainsKey(address))
                        {
                            throw new FormatException(
                                $"Duplicate address {address} in spec '{spec}'. Each result address may appear in at most one bucket.");
                        }

                        addressToBucket[address] = bucketName;
                    }
                }
            }

            return addressToBucket;
        }

        /// <summary>
        /// An address into a SARIF log: a (run index, result index) pair.
        /// </summary>
        public readonly struct ResultAddress : IEquatable<ResultAddress>
        {
            public ResultAddress(int runIndex, int resultIndex)
            {
                RunIndex = runIndex;
                ResultIndex = resultIndex;
            }

            public int RunIndex { get; }

            public int ResultIndex { get; }

            public bool Equals(ResultAddress other)
                => RunIndex == other.RunIndex && ResultIndex == other.ResultIndex;

            public override bool Equals(object obj) => obj is ResultAddress other && Equals(other);

            public override int GetHashCode()
                => unchecked((RunIndex * 397) ^ ResultIndex);

            public override string ToString()
                => $"sarif:/runs/{RunIndex.ToString(CultureInfo.InvariantCulture)}/results/{ResultIndex.ToString(CultureInfo.InvariantCulture)}";
        }

        private static string ComputeStrategyKey(
            SplittingStrategy strategy,
            Run run,
            Result result,
            int runIndex,
            int globalResultIndex)
        {
            string ruleId = result.RuleId ?? string.Empty;
            string runIndexText = runIndex.ToString(CultureInfo.InvariantCulture);

            switch (strategy)
            {
                case SplittingStrategy.PerRule:
                    return ruleId;

                case SplittingStrategy.PerRunPerRule:
                    return $"run{runIndexText}_{ruleId}";

                case SplittingStrategy.PerRunPerTarget:
                    return $"run{runIndexText}_{GetTargetKey(run, result)}";

                case SplittingStrategy.PerRunPerTargetPerRule:
                    return $"run{runIndexText}_{GetTargetKey(run, result)}_{ruleId}";

                case SplittingStrategy.PerRun:
                    return $"run{runIndexText}";

                case SplittingStrategy.PerResult:
                    return $"result{globalResultIndex.ToString(CultureInfo.InvariantCulture)}";

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(strategy),
                        strategy,
                        $"Strategy '{strategy}' is not supported by {nameof(PartitionFunctions)}.{nameof(ForStrategy)}. " +
                        $"Use {nameof(ForIndexList)} for {nameof(SplittingStrategy.PerIndexList)}.");
            }
        }

        private static string GetTargetKey(Run run, Result result)
        {
            if (result?.Locations == null || result.Locations.Count == 0) { return "noTarget"; }

            Location loc = result.Locations[0];
            ArtifactLocation artifactLocation = loc?.PhysicalLocation?.ArtifactLocation;
            if (artifactLocation == null) { return "noTarget"; }

            if (artifactLocation.Uri != null)
            {
                return artifactLocation.Uri.OriginalString;
            }

            // Fall back to the run-level artifact entry referenced by Index, if any.
            if (artifactLocation.Index >= 0
                && run?.Artifacts != null
                && artifactLocation.Index < run.Artifacts.Count)
            {
                Artifact artifact = run.Artifacts[artifactLocation.Index];
                Uri uri = artifact?.Location?.Uri;
                if (uri != null) { return uri.OriginalString; }
            }

            return "noTarget";
        }

        private static IEnumerable<ResultAddress> ParseSegment(string segment)
        {
            // Two shapes are accepted within a segment:
            //   1) A compact form:    "[<runId>:]<r1>[,<r2>...]"
            //   2) One or more SARIF URLs: "sarif:/runs/<r>/results/<m>[,sarif:/...]"
            //
            // SARIF URLs and bare integers can be intermixed within a comma list. The optional
            // "<runId>:" prefix only applies to the bare-integer tokens that follow it within
            // the segment; SARIF URLs always carry their own run index.
            int runIndex = 0;
            string body = segment;

            int colonIndex = IndexOfRunPrefixColon(segment);
            if (colonIndex >= 0)
            {
                string runText = segment.Substring(0, colonIndex).Trim();
                body = segment.Substring(colonIndex + 1).Trim();

                if (!int.TryParse(runText, NumberStyles.Integer, CultureInfo.InvariantCulture, out runIndex)
                    || runIndex < 0)
                {
                    throw new FormatException($"Invalid run index '{runText}' in segment '{segment}'.");
                }
            }

            if (body.Length == 0)
            {
                throw new FormatException($"Segment '{segment}' has no result addresses.");
            }

            foreach (string token in body.Split(','))
            {
                string trimmed = token.Trim();
                if (trimmed.Length == 0)
                {
                    throw new FormatException($"Empty address in segment '{segment}'.");
                }

                if (trimmed.StartsWith("sarif:", StringComparison.OrdinalIgnoreCase))
                {
                    yield return ParseSarifUri(trimmed);
                    continue;
                }

                if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int resultIndex)
                    || resultIndex < 0)
                {
                    throw new FormatException($"Invalid result index '{trimmed}' in segment '{segment}'.");
                }

                yield return new ResultAddress(runIndex, resultIndex);
            }
        }

        private static int IndexOfRunPrefixColon(string segment)
        {
            // The "<runId>:" prefix is only valid for the compact form. A leading "sarif:" is part
            // of the URI scheme, not a run prefix.
            if (segment.StartsWith("sarif:", StringComparison.OrdinalIgnoreCase))
            {
                return -1;
            }

            int colonIndex = segment.IndexOf(':');
            if (colonIndex < 0)
            {
                return -1;
            }

            // Everything before the colon must be digits for it to be a run prefix; otherwise the
            // colon belongs to a later token (e.g., an embedded sarif: URL).
            for (int i = 0; i < colonIndex; i++)
            {
                if (!char.IsDigit(segment[i]) && !char.IsWhiteSpace(segment[i]))
                {
                    return -1;
                }
            }

            return colonIndex;
        }

        private static ResultAddress ParseSarifUri(string uri)
        {
            // Per the SARIF 2.1.0 spec section 3.10.3, a SARIF URL has the form
            // "sarif:" + JSON-Pointer. For result addresses the pointer is
            // "/runs/<n>/results/<m>", so the full URI looks like
            // "sarif:/runs/<n>/results/<m>" with a single '/' after the scheme.
            const string expected = "sarif:/runs/{N}/results/{M}";
            const string scheme = "sarif:";

            if (!uri.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException($"SARIF URI '{uri}' does not match the expected form '{expected}'.");
            }

            string pointer = uri.Substring(scheme.Length);
            // pointer should be "/runs/<n>/results/<m>"; Split('/') yields
            // ["", "runs", "<n>", "results", "<m>"].
            string[] parts = pointer.Split('/');
            if (parts.Length != 5
                || parts[0].Length != 0
                || !string.Equals(parts[1], "runs", StringComparison.Ordinal)
                || !string.Equals(parts[3], "results", StringComparison.Ordinal))
            {
                throw new FormatException($"SARIF URI '{uri}' does not match the expected form '{expected}'.");
            }

            if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int runIndex)
                || runIndex < 0)
            {
                throw new FormatException($"Invalid run index '{parts[2]}' in SARIF URI '{uri}'.");
            }

            if (!int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int resultIndex)
                || resultIndex < 0)
            {
                throw new FormatException($"Invalid result index '{parts[4]}' in SARIF URI '{uri}'.");
            }

            return new ResultAddress(runIndex, resultIndex);
        }

        private static void ValidateAddressesAgainstLog(IDictionary<ResultAddress, string> addresses, SarifLog log)
        {
            int runCount = log.Runs?.Count ?? 0;
            foreach (ResultAddress address in addresses.Keys)
            {
                if (address.RunIndex >= runCount)
                {
                    throw new InvalidOperationException(
                        $"Address {address} references run {address.RunIndex}, but the log contains only {runCount} run(s).");
                }

                Run run = log.Runs[address.RunIndex];
                int resultCount = run?.Results?.Count ?? 0;
                if (address.ResultIndex >= resultCount)
                {
                    throw new InvalidOperationException(
                        $"Address {address} references result {address.ResultIndex}, but run {address.RunIndex} contains only {resultCount} result(s).");
                }
            }
        }

        private sealed class ResultReferenceComparer : IEqualityComparer<Result>
        {
            public static readonly ResultReferenceComparer Instance = new ResultReferenceComparer();

            public bool Equals(Result x, Result y) => ReferenceEquals(x, y);

            public int GetHashCode(Result obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
