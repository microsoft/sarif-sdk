// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Mcp.Server
{
    /// <summary>
    /// Maintains the mutable state for an in-progress SARIF run: rule
    /// registration, artifact table, and notification descriptors. All
    /// mutation is serialized via an internal lock to support concurrent
    /// requests on the same run.
    /// </summary>
    public sealed class SarifRunContext
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, int> _ruleBaseIdToIndex = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _artifactUriToIndex = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, int> _notificationIdToIndex = new(StringComparer.Ordinal);

        // All-or-nothing property tracking: missing = no results yet,
        // true = first result had it, false = first result didn't.
        private readonly Dictionary<string, bool> _allOrNothingState = new(StringComparer.Ordinal);

        public Run Run { get; }

        public string OutputPath { get; }

        public string SourceRoot { get; }

        public bool IsFinalized { get; private set; }

        public DateTime CreatedUtc { get; } = DateTime.UtcNow;

        public CweNameResolver? CweResolver { get; set; }

        /// <summary>
        /// Per-run file cache. Provides newline indexing, hash computation,
        /// snippet population, and context-region construction for source
        /// files referenced by this run's results.
        /// </summary>
        public FileRegionsCache FileRegionsCache { get; } = new FileRegionsCache();

        public SarifRunContext(Run run, string outputPath, string sourceRoot)
        {
            this.Run = run ?? throw new ArgumentNullException(nameof(run));

            // Canonicalize paths so that relative traversal in agent-supplied
            // URIs resolves deterministically. This is not a sandbox: in stdio
            // mode the agent IS the process, and in HTTP mode the real security
            // boundary is the configured auth, not filesystem path checks.
            this.OutputPath = string.IsNullOrEmpty(outputPath) ? outputPath : Path.GetFullPath(outputPath);
            this.SourceRoot = string.IsNullOrEmpty(sourceRoot) ? sourceRoot : Path.GetFullPath(sourceRoot);

            this.Run.Results = new List<Result>();
            this.Run.Artifacts = new List<Artifact>();
            this.Run.Tool.Driver.Rules = new List<ReportingDescriptor>();
            this.Run.Tool.Driver.Notifications = new List<ReportingDescriptor>();
            this.Run.Invocations = new List<Invocation>();
        }

        /// <summary>
        /// Returns true if a rule with the given (base or NOVEL-qualified) ID is already registered.
        /// </summary>
        public bool HasRule(string ruleId)
        {
            string baseId = GetBaseRuleId(ruleId);
            string key = string.Equals(baseId, "NOVEL", StringComparison.OrdinalIgnoreCase) ? ruleId : baseId;
            lock (this._lock) { return this._ruleBaseIdToIndex.ContainsKey(key); }
        }

        /// <summary>
        /// Returns the index into <c>tool.driver.rules[]</c>. If the rule is already
        /// registered, returns the existing index. CWE-prefixed IDs are resolved
        /// against MITRE for a PascalCase name when a resolver is attached.
        /// </summary>
        public int EnsureRule(
            string ruleId,
            string? ruleName = null,
            string? shortDescription = null,
            string? helpUri = null,
            FailureLevel defaultLevel = FailureLevel.Warning)
        {
            string baseId = GetBaseRuleId(ruleId);
            string key = string.Equals(baseId, "NOVEL", StringComparison.OrdinalIgnoreCase) ? ruleId : baseId;

            // Fast path: already registered (no I/O needed).
            lock (this._lock)
            {
                if (this._ruleBaseIdToIndex.TryGetValue(key, out int existingIndex))
                {
                    return existingIndex;
                }
            }

            // Slow path: resolve CWE name OUTSIDE the lock (may do HTTP).
            string? resolvedName = ruleName ?? this.CweResolver?.Resolve(baseId);

            lock (this._lock)
            {
                ThrowIfFinalizedUnsafe();
                if (this._ruleBaseIdToIndex.TryGetValue(key, out int existingIndex))
                {
                    return existingIndex;
                }

                var descriptor = new ReportingDescriptor
                {
                    Id = baseId,
                    Name = resolvedName,
                    ShortDescription = shortDescription != null
                        ? new MultiformatMessageString { Text = shortDescription }
                        : null,
                    HelpUri = helpUri != null ? new Uri(helpUri) : null,
                    DefaultConfiguration = new ReportingConfiguration { Level = defaultLevel }
                };

                int index = this.Run.Tool.Driver.Rules!.Count;
                this.Run.Tool.Driver.Rules.Add(descriptor);
                this._ruleBaseIdToIndex[key] = index;
                return index;
            }
        }

        /// <summary>
        /// Gets or registers an artifact by URI, returning its index in <c>run.artifacts[]</c>.
        /// Computes SHA-1/SHA-256 hashes and length when the artifact resolves to an existing file.
        /// </summary>
        public int EnsureArtifact(string uri)
        {
            string normalizedUri = NormalizeUri(uri);

            lock (this._lock)
            {
                if (this._artifactUriToIndex.TryGetValue(normalizedUri, out int existingIndex))
                {
                    return existingIndex;
                }
            }

            // Slow path: compute hashes + length OUTSIDE the lock.
            string? absolutePath = ResolveToAbsolutePath(normalizedUri);
            IDictionary<string, string>? hashes = null;
            int fileLength = -1;

            if (absolutePath != null && File.Exists(absolutePath))
            {
                HashData hashData = HashUtilities.ComputeHashes(absolutePath);
                hashes = hashData?.ToDictionary();

                // SARIF Artifact.Length is int (max ~2 GB). Source files
                // analysed in MCP scenarios are far below that boundary.
                long rawLength = new FileInfo(absolutePath).Length;
                fileLength = rawLength > int.MaxValue ? int.MaxValue : (int)rawLength;
            }

            lock (this._lock)
            {
                ThrowIfFinalizedUnsafe();
                if (this._artifactUriToIndex.TryGetValue(normalizedUri, out int existingIndex))
                {
                    return existingIndex;
                }

                var artifact = new Artifact
                {
                    Location = new ArtifactLocation { Uri = new Uri(normalizedUri, UriKind.RelativeOrAbsolute) },
                    Roles = ArtifactRoles.AnalysisTarget,
                    Hashes = hashes,
                    Length = fileLength
                };

                int index = this.Run.Artifacts!.Count;
                this.Run.Artifacts.Add(artifact);
                this._artifactUriToIndex[normalizedUri] = index;
                return index;
            }
        }

        /// <summary>Registers a notification descriptor if not already present.</summary>
        public int EnsureNotificationDescriptor(string descriptorId, FailureLevel level)
        {
            lock (this._lock)
            {
                ThrowIfFinalizedUnsafe();
                if (this._notificationIdToIndex.TryGetValue(descriptorId, out int existingIndex))
                {
                    return existingIndex;
                }

                var descriptor = new ReportingDescriptor
                {
                    Id = descriptorId,
                    DefaultConfiguration = new ReportingConfiguration { Level = level }
                };

                int index = this.Run.Tool.Driver.Notifications!.Count;
                this.Run.Tool.Driver.Notifications.Add(descriptor);
                this._notificationIdToIndex[descriptorId] = index;
                return index;
            }
        }

        /// <summary>
        /// Checks all-or-nothing consistency for a property key.
        /// Returns a warning message if inconsistent, null otherwise.
        /// </summary>
        public string? CheckAllOrNothing(string propertyKey, bool newResultHasProperty)
        {
            lock (this._lock)
            {
                if (!this._allOrNothingState.TryGetValue(propertyKey, out bool firstHad))
                {
                    this._allOrNothingState[propertyKey] = newResultHasProperty;
                    return null;
                }

                if (firstHad && !newResultHasProperty)
                {
                    return $"All-or-nothing violation: prior results have '{propertyKey}' but this result does not. " +
                           $"If any result carries '{propertyKey}', all must.";
                }

                if (!firstHad && newResultHasProperty)
                {
                    return $"All-or-nothing: prior results lack '{propertyKey}' but this result has it. " +
                           $"Back-fill prior results or remove from this one.";
                }

                return null;
            }
        }

        /// <summary>Adds a result to the run's results list. Thread-safe.</summary>
        public int AddResult(Result result)
        {
            lock (this._lock)
            {
                ThrowIfFinalizedUnsafe();
                this.Run.Results!.Add(result);
                return this.Run.Results.Count - 1;
            }
        }

        /// <summary>Adds a notification to the specified invocation. Thread-safe.</summary>
        public void AddNotification(Notification notification, bool isConfig, int? invocationIndex)
        {
            lock (this._lock)
            {
                ThrowIfFinalizedUnsafe();

                if (this.Run.Invocations!.Count == 0)
                {
                    throw new InvalidOperationException(
                        "No invocations exist. Call sarif_start_invocation first.");
                }

                Invocation invocation = this.Run.Invocations[0];
                if (invocationIndex != null)
                {
                    if (invocationIndex < 0 || invocationIndex >= this.Run.Invocations.Count)
                    {
                        throw new ArgumentException(
                            $"Invalid invocation_index {invocationIndex}. " +
                            $"Run has {this.Run.Invocations.Count} invocations (0-based).");
                    }

                    invocation = this.Run.Invocations[invocationIndex.Value];
                }
                else if (this.Run.Invocations.Count > 1)
                {
                    invocation = this.Run.Invocations[^1];
                }

                if (isConfig)
                {
                    invocation.ToolConfigurationNotifications ??= new List<Notification>();
                    invocation.ToolConfigurationNotifications.Add(notification);
                }
                else
                {
                    invocation.ToolExecutionNotifications ??= new List<Notification>();
                    invocation.ToolExecutionNotifications.Add(notification);
                }
            }
        }

        /// <summary>Adds a new invocation to the run. Thread-safe.</summary>
        public int AddInvocation(Invocation invocation)
        {
            lock (this._lock)
            {
                ThrowIfFinalizedUnsafe();
                int index = this.Run.Invocations!.Count;
                this.Run.Invocations.Add(invocation);
                return index;
            }
        }

        /// <summary>Closes an invocation by index. Thread-safe.</summary>
        public void CloseInvocation(int invocationIndex, bool success, int? exitCode, string? summary)
        {
            lock (this._lock)
            {
                ThrowIfFinalizedUnsafe();

                if (invocationIndex < 0 || invocationIndex >= this.Run.Invocations!.Count)
                {
                    throw new ArgumentException(
                        $"Invalid invocation_index {invocationIndex}. " +
                        $"Run has {this.Run.Invocations!.Count} invocations (0-based).");
                }

                Invocation inv = this.Run.Invocations[invocationIndex];
                if (inv.EndTimeUtc != default)
                {
                    throw new InvalidOperationException(
                        $"Invocation {invocationIndex} is already closed (ended at {inv.EndTimeUtc:O}).");
                }

                inv.EndTimeUtc = DateTime.UtcNow;
                inv.ExecutionSuccessful = success;
                if (exitCode.HasValue)
                {
                    inv.ExitCode = exitCode.Value;
                }

                if (summary != null)
                {
                    inv.SetProperty("ai/summary", summary);
                }
            }
        }

        /// <summary>
        /// Atomically applies final metadata, closes open invocations, and marks
        /// the run as finalized. After this call, AddResult/AddNotification throw,
        /// making the Run effectively immutable.
        /// </summary>
        public (int ResultCount, int RuleCount) FinalizeWithMetadata(
            string? handoffNotes,
            DateTime? endTimeUtc = null)
        {
            lock (this._lock)
            {
                ThrowIfFinalizedUnsafe();

                DateTime endTime = endTimeUtc ?? DateTime.UtcNow;

                if (handoffNotes != null)
                {
                    this.Run.SetProperty("ai/handoff", handoffNotes);
                }

                foreach (Invocation inv in this.Run.Invocations!)
                {
                    if (inv.EndTimeUtc == default)
                    {
                        inv.EndTimeUtc = endTime;
                    }
                }

                int resultCount = this.Run.Results?.Count ?? 0;
                int ruleCount = this.Run.Tool.Driver.Rules?.Count ?? 0;

                this.IsFinalized = true;

                return (resultCount, ruleCount);
            }
        }

        public void ThrowIfFinalized()
        {
            lock (this._lock) { ThrowIfFinalizedUnsafe(); }
        }

        private void ThrowIfFinalizedUnsafe()
        {
            if (this.IsFinalized)
            {
                throw new InvalidOperationException("Run is finalized. Create a new run.");
            }
        }

        /// <summary>
        /// Extracts the base rule ID: "CWE-502/binaryformatter-no-binder" → "CWE-502".
        /// "NOVEL/something" → "NOVEL".
        /// </summary>
        internal static string GetBaseRuleId(string ruleId)
        {
            int slash = ruleId.IndexOf('/');
            return slash > 0 ? ruleId[..slash] : ruleId;
        }

        /// <summary>
        /// Resolves a URI (relative or absolute) to an absolute filesystem path,
        /// constrained to live under <see cref="SourceRoot"/>. Returns null if
        /// the file doesn't exist or escapes the source root.
        /// </summary>
        internal string? ResolveToAbsolutePath(string uri)
        {
            if (string.IsNullOrEmpty(this.SourceRoot))
            {
                return null;
            }

            string candidate;
            if (Path.IsPathRooted(uri))
            {
                candidate = Path.GetFullPath(uri);
            }
            else
            {
                candidate = Path.GetFullPath(
                    Path.Combine(this.SourceRoot, uri.Replace('/', Path.DirectorySeparatorChar)));
            }

            string canonicalRoot = this.SourceRoot.TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            if (!candidate.StartsWith(canonicalRoot, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return File.Exists(candidate) ? candidate : null;
        }

        /// <summary>
        /// Normalizes a file URI: if absolute and under SourceRoot, converts to
        /// relative with forward slashes. External/non-file URIs are left untouched.
        /// </summary>
        private string NormalizeUri(string uri)
        {
            if (string.IsNullOrEmpty(this.SourceRoot) || string.IsNullOrEmpty(uri))
            {
                return uri;
            }

            string normalized = uri.Replace('\\', '/');

            if (Path.IsPathRooted(uri))
            {
                string rootNormalized = this.SourceRoot.Replace('\\', '/').TrimEnd('/') + "/";
                if (normalized.StartsWith(rootNormalized, StringComparison.OrdinalIgnoreCase))
                {
                    normalized = normalized[rootNormalized.Length..];
                }
            }

            return normalized;
        }
    }
}
