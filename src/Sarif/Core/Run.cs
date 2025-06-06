﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Visitors;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class Run
    {
        private Dictionary<string, FailureLevel> PoliciesCache;

        private Dictionary<ArtifactLocation, int> _artifactLocationToIndexMap;

        public Uri ExpandUrisWithUriBaseId(string key)
        {
            ArtifactLocation fileLocation = this.OriginalUriBaseIds[key];

            if (fileLocation.UriBaseId == null)
            {
                return fileLocation.Uri;
            }
            throw new InvalidOperationException("Author this code along with tests for originalUriBaseIds that are nested");
        }

        public int GetFileIndex(
            ArtifactLocation fileLocation,
            bool addToFilesTableIfNotPresent = true,
            OptionallyEmittedData dataToInsert = OptionallyEmittedData.None,
            Encoding encoding = null,
            HashData hashData = null)
        {
            if (fileLocation == null) { throw new ArgumentNullException(nameof(fileLocation)); }

            if (this.Artifacts == null || this.Artifacts.Count == 0)
            {
                if (!addToFilesTableIfNotPresent)
                {
                    return -1;
                }
            }

            if (_artifactLocationToIndexMap == null)
            {
                InitializeFileToIndexMap();
            }

            if (fileLocation.Uri == null)
            {
                // We only have a file index, so just return it.
                return fileLocation.Index;
            }

            // Strictly speaking, some elements that may contribute to a files table
            // key are case sensitive, e.g., everything but the scheme and protocol of a
            // web URI. We don't have a proper comparer implementation that can handle
            // all cases. For now, we cover the Windows happy path, which assumes that
            // most URIs in log files are file paths (which are case-insensitive).
            //
            // Tracking item for an improved comparer:
            // https://github.com/Microsoft/sarif-sdk/issues/973

            // When we perform a files table look-up, only the uri and uriBaseId
            // are relevant; these properties together comprise the unique identity
            // of the file object. The file index, of course, does not relate to the
            // file identity. We consciously exclude the properties bag as well.

            // We will normalize the input fileLocation.Uri to make URIs more consistent
            // throughout the emitted log.
            fileLocation.Uri = new Uri(UriHelper.MakeValidUri(fileLocation.Uri.OriginalString), UriKind.RelativeOrAbsolute);

            var artifactLocation = new ArtifactLocation
            {
                Uri = fileLocation.Uri,
                UriBaseId = fileLocation.UriBaseId
            };

            if (!_artifactLocationToIndexMap.TryGetValue(artifactLocation, out int artifactIndex))
            {
                if (addToFilesTableIfNotPresent)
                {
                    this.Artifacts ??= new List<Artifact>();
                    artifactIndex = this.Artifacts.Count;

                    Uri artifactUri = artifactLocation.TryReconstructAbsoluteUri(this.OriginalUriBaseIds, out Uri resolvedUri)
                        ? resolvedUri
                        : artifactLocation.Uri;

                    var artifact = Artifact.Create(
                        artifactUri,
                        dataToInsert,
                        hashData: hashData,
                        encoding: encoding);

                    // Copy ArtifactLocation to ensure changes to Result copy don't affect new Run.Artifacts copy
                    artifact.Location = new ArtifactLocation(fileLocation);

                    this.Artifacts.Add(artifact);

                    _artifactLocationToIndexMap[artifactLocation] = artifactIndex;
                }
                else
                {
                    // We did not find the item. The call was not configured to add the entry.
                    // Return the default value that indicates the item isn't present.
                    artifactIndex = -1;
                }
            }

            fileLocation.Index = artifactIndex;
            return artifactIndex;
        }

        private void InitializeFileToIndexMap()
        {
            _artifactLocationToIndexMap = new Dictionary<ArtifactLocation, int>(ArtifactLocation.ValueComparer);

            // First, we'll initialize our file object to index map
            // with any files that already exist in the table
            for (int i = 0; i < this.Artifacts?.Count; i++)
            {
                Artifact artifact = this.Artifacts[i];

                var artifactLocation = new ArtifactLocation
                {
                    Uri = artifact.Location?.Uri,
                    UriBaseId = artifact.Location?.UriBaseId,
                };

                _artifactLocationToIndexMap[artifactLocation] = i;
            }
        }

        /// <summary>
        ///  Find the ToolComponent corresponding to a ToolComponentReference.
        /// </summary>
        /// <param name="reference">ToolComponentReference to resolve</param>
        /// <returns>ToolComponent for reference</returns>
        public ToolComponent GetToolComponentFromReference(ToolComponentReference reference)
        {
            return this.Tool?.GetToolComponentFromReference(reference);
        }

        /// <summary>
        ///  Set the Run property on each Result to this Run, so that Result methods
        ///  and properties which may need to look up Run collections can do so.
        /// </summary>
        public void SetRunOnResults()
        {
            if (this.Results != null)
            {
                if (this.Results is DeferredList<Result> deferredResults)
                {
                    // On deferred object model, must change Results as they're read, since they are discarded after each enumeration
                    deferredResults.AddTransformer((result) =>
                    {
                        result.Run = this;
                        return result;
                    });
                }
                else
                {
                    // Otherwise, just set the Result.Run property on each Result now
                    foreach (Result result in this.Results)
                    {
                        result.Run = this;
                    }
                }
            }
        }

        public void MergeResultsFrom(Run additional)
        {
            // Merge Results from the two Runs, building shared collections of result-referenced things
            var visitor = new RunMergingVisitor();

            visitor.VisitRun(this);
            visitor.VisitRun(additional);

            visitor.PopulateWithMerged(this);
        }

        public bool ShouldSerializeColumnKind()
        {
            // This serialization helper does two things.
            //
            // First, if ColumnKind has not been
            // explicitly set, we will set it to the value that works for the Microsoft
            // platform (which is not the specified SARIF default). This makes sure that
            // the value is set appropriate for code running on the Microsoft platform,
            // even if the SARIF producer is not aware of this rather obscure value.
            if (this.ColumnKind == ColumnKind.None)
            {
                this.ColumnKind = ColumnKind.Utf16CodeUnits;
            }

            // Second, we will always explicitly serialize this value. Otherwise, we can't easily
            // distinguish between earlier versions of the format for which this property was typically absent.
            return true;
        }

        public bool ShouldSerializeArtifacts()
        {
            return this.Artifacts.HasAtLeastOneNonDefaultValue(Artifact.ValueComparer);
        }

        public bool ShouldSerializeGraphs()
        {
            return this.Graphs.HasAtLeastOneNonDefaultValue(Graph.ValueComparer);
        }

        public bool ShouldSerializeAutomationDetails()
        {
            return this.AutomationDetails?.Description != null ||
                !string.IsNullOrWhiteSpace(this.AutomationDetails?.Id) ||
                (this.AutomationDetails?.Guid != null && this.AutomationDetails.Guid.Value != Guid.Empty) ||
                (this.AutomationDetails?.CorrelationGuid != null && this.AutomationDetails.CorrelationGuid != Guid.Empty);
        }

        public bool ShouldSerializeInvocations()
        {
            return this.Invocations.HasAtLeastOneNonNullValue();
        }

        public bool ShouldSerializeLogicalLocations()
        {
            return this.LogicalLocations.HasAtLeastOneNonDefaultValue(LogicalLocation.ValueComparer);
        }

        public bool ShouldSerializeNewlineSequences()
        {
            return this.NewlineSequences.HasAtLeastOneNonNullValue();
        }

        internal static Dictionary<string, FailureLevel> ComputePolicies(IEnumerable<ToolComponent> policies)
        {
            var localCache = new Dictionary<string, FailureLevel>();

            // checking if we have have policies
            if (policies == null || !policies.Any())
            {
                return localCache;
            }

            foreach (ToolComponent policy in policies)
            {
                foreach (ReportingDescriptor rule in policy.Rules)
                {
                    localCache[rule.Id] = rule.DefaultConfiguration.Level;
                }
            }

            return localCache;
        }

        /// <summary>
        /// Applies the policies contained in this run, if any, to remap result failure levels.
        /// When multiple policies remap the same rule, the last policy in the policies
        /// collection has precedence.
        /// </summary>
        public void ApplyPolicies(Dictionary<string, FailureLevel> policiesCache = null)
        {
            // Use policies cache if exist
            if (policiesCache != null)
            {
                PoliciesCache = policiesCache;
            }

            // Compute policies
            if (PoliciesCache == null || PoliciesCache.Count == 0)
            {
                PoliciesCache = ComputePolicies(this.Policies);
            }

            if (PoliciesCache.Count == 0)
            {
                return;
            }

            foreach (Result result in this.Results)
            {
                string ruleId = result.ResolvedRuleId(this);

                if (PoliciesCache.TryGetValue(ruleId, out FailureLevel level))
                {
                    result.Level = level;
                }
            }
        }

        public bool HasResults() => this.Results?.Count > 0;

        /// <summary>
        /// Returns a value indicating whether this run has any results whose baseline state
        /// is "absent".
        /// </summary>
        /// <param name="run">
        /// The <see cref="Run"/> whose results are to be examined.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="run"/> has any absent results, otherwise
        /// <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The SARIF spec states that the property <see cref="Result.BaselineState"/> must either
        /// be present on all results or on none of them. This requirement is intended to optimize
        /// performance of SARIF consumers such as results viewers, which (for example) need only
        /// examine the first result to decide whether to display a "Baseline state" column.
        /// Therefore if the first result has <see cref="BaselineState.None"/>, this method does
        /// not examine the rest of the results, and it returns <c>false</c>.
        /// </remarks>
        public bool HasAbsentResults() =>
            this.HasResults()
            && this.Results[0].BaselineState != BaselineState.None
            && this.Results.Any(r => r.BaselineState == BaselineState.Absent);

        /// <summary>
        /// Returns a value indicating whether this run has any suppressed results.
        /// </summary>
        /// <param name="run">
        /// The <see cref="Run"/> whose results are to be examined.
        /// </param>
        /// <returns>
        /// <c>true</c> if <paramref name="run"/> has any suppressed results, otherwise
        /// <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The SARIF spec states that the property <see cref="Result.Suppressions"/> must either
        /// be present on all results or on none of them. This requirement is intended to optimize
        /// performance of SARIF consumers such as results viewers, which (for example) need only
        /// examine the first result to decide whether to display a "Suppressed" column. Therefore
        /// if the first result has a Suppressions value of null, this method does examine the rest
        /// of the results, and it returns <c>false</c>.
        /// </remarks>
        public bool HasSuppressedResults() =>
            this.HasResults()
            && this.Results[0].Suppressions != null
            && this.Results.Any(r => r.TryIsSuppressed(out bool isSuppressed) && isSuppressed);
    }
}
