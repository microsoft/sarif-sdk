// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    ///  SarifConsolidator is used to reduce the size of SARIF objects and the serialized log.
    ///  It removes common redundant properties, uses Run-wide collections to avoid writing duplicate
    ///  values, and can remove portions of Results entirely.
    /// </summary>
    /// <remarks>
    ///  NOTE: Removing some log portions may leave abandoned references elsewhere.
    ///   For example, removing RelatedLocations may break references from Result Locations or Message.
    /// </remarks>
    public class SarifConsolidator
    {
        private readonly Run _run;
        private readonly Dictionary<ThreadFlowLocation, int> _uniqueThreadFlowLocations;

        public int TotalThreadFlowLocations { get; private set; }
        public int UniqueThreadFlowLocations => _uniqueThreadFlowLocations.Count;

        public int TotalLocations { get; private set; }
        public int UniqueLocations { get; private set; }

        public RegionComponents RegionComponentsToKeep { get; set; } = RegionComponents.LineAndColumn;
        public int? MessageLengthLimitChars { get; set; }
        public bool RemoveUriBaseIds { get; set; }
        public bool RemoveCodeFlows { get; set; }
        public bool RemoveRelatedLocations { get; set; }
        public bool RemoveGraphs { get; set; }
        public bool RemoveStacks { get; set; }
        public bool RemoveWebRequests { get; set; }
        public bool RemoveWebResponses { get; set; }

        public SarifConsolidator(Run run)
        {
            _run = run;
            _uniqueThreadFlowLocations = new Dictionary<ThreadFlowLocation, int>(ThreadFlowLocation.ValueComparer);

            if (this.RemoveUriBaseIds)
            {
                run?.OriginalUriBaseIds.Clear();
            }

            if (_run.Artifacts != null)
            {
                // Realize Deferred Artifact list
                _run.Artifacts = _run.Artifacts?.ToList();

                foreach (Artifact a in _run.Artifacts)
                {
                    // Remove indices from Run.Artifact array (redundant)
                    // Remove UriBaseIds if requested
                    if (a.Location != null)
                    {
                        a.Location.Index = -1;
                        a.Location.UriBaseId = (this.RemoveUriBaseIds ? null : a.Location.UriBaseId);
                    }
                }
            }

            // Ensure Run TFL list is ready for consolidating TFLs
            _run.ThreadFlowLocations = _run.ThreadFlowLocations ?? new List<ThreadFlowLocation>();

            // Realize ThreadFlowLocations for consolidation
            _run.ThreadFlowLocations = _run.ThreadFlowLocations.ToList();

            // Add existing TFLs to map (can't consolidate; there may be existing references to them)
            for (int i = 0; i < _run.ThreadFlowLocations.Count; ++i)
            {
                ThreadFlowLocation tfl = _run.ThreadFlowLocations[i];
                Trim(tfl.Location);
                _uniqueThreadFlowLocations[tfl] = i;
            }
        }

        public void Trim(Result result)
        {
            if (result == null) { return; }

            // Remove Result components if configured to
            result.CodeFlows = (this.RemoveCodeFlows ? null : result.CodeFlows);
            result.RelatedLocations = (this.RemoveRelatedLocations ? null : result.RelatedLocations);
            result.Graphs = (this.RemoveGraphs ? null : result.Graphs);
            result.GraphTraversals = (this.RemoveGraphs ? null : result.GraphTraversals);
            result.Stacks = (this.RemoveStacks ? null : result.Stacks);
            result.WebRequest = (this.RemoveWebRequests ? null : result.WebRequest);
            result.WebResponse = (this.RemoveWebResponses ? null : result.WebResponse);

            // Truncate long Messages if configured
            if (this.MessageLengthLimitChars > 0)
            {
                if (result.Message?.Text?.Length > this.MessageLengthLimitChars)
                {
                    result.Message.Text = result.Message.Text.Substring(0, this.MessageLengthLimitChars.Value) + "...";
                }

                if (result.Message?.Markdown?.Length > this.MessageLengthLimitChars)
                {
                    result.Message.Markdown = result.Message.Markdown.Substring(0, this.MessageLengthLimitChars.Value) + "...";
                }
            }

            // Trim Location and RelatedLocation properties
            result.Locations = Trim(result.Locations);
            result.RelatedLocations = Trim(result.RelatedLocations);

            // Trim and Consolidate CodeFlows and ThreadFlows
            ConsolidateCodeFlows(result);
        }

        public void ConsolidateCodeFlows(Result result)
        {
            if (result.CodeFlows != null)
            {
                foreach (CodeFlow cFlow in result.CodeFlows)
                {
                    foreach (ThreadFlow tFlow in cFlow.ThreadFlows)
                    {
                        Consolidate(tFlow);
                    }
                }
            }
        }

        public void Consolidate(ThreadFlow threadFlow)
        {
            if (threadFlow.Locations != null)
            {
                TotalThreadFlowLocations += threadFlow.Locations.Count;

                for (int i = 0; i < threadFlow.Locations.Count; ++i)
                {
                    ThreadFlowLocation tfl = threadFlow.Locations[i];
                    Trim(tfl.Location);

                    if (tfl != null && tfl.Index == -1)
                    {
                        if (!_uniqueThreadFlowLocations.TryGetValue(tfl, out int tflIndex))
                        {
                            tflIndex = _run.ThreadFlowLocations.Count;
                            _run.ThreadFlowLocations.Add(tfl);
                            _uniqueThreadFlowLocations[tfl] = tflIndex;
                        }

                        threadFlow.Locations[i] = new ThreadFlowLocation() { Index = tflIndex };
                    }
                }
            }
        }

        public IList<Location> Trim(IList<Location> locations)
        {
            if (locations == null || locations.Count == 0) { return locations; }

            HashSet<Location> uniqueLocations = new HashSet<Location>(Location.ValueComparer);
            List<Location> newLocations = new List<Location>();

            foreach (Location location in locations)
            {
                Trim(location);

                // Keep only one copy of each unique location
                if (location != null && uniqueLocations.Add(location))
                {
                    newLocations.Add(location);
                }
            }

            this.TotalLocations += locations.Count;
            this.UniqueLocations += uniqueLocations.Count;

            return newLocations;
        }

        public void Trim(Location location)
        {
            if (location != null)
            {
                Trim(location.PhysicalLocation);
                Trim(location.LogicalLocations);
            }
        }

        public IList<LogicalLocation> Trim(IList<LogicalLocation> logicalLocations)
        {
            if (logicalLocations == null || logicalLocations.Count == 0) { return logicalLocations; }

            HashSet<LogicalLocation> uniqueLocations = new HashSet<LogicalLocation>(LogicalLocation.ValueComparer);
            List<LogicalLocation> newLocations = new List<LogicalLocation>();

            foreach (LogicalLocation logicalLocation in logicalLocations)
            {
                Trim(logicalLocation);

                // Keep only one copy of each unique location
                if (logicalLocation != null && uniqueLocations.Add(logicalLocation))
                {
                    newLocations.Add(logicalLocation);
                }
            }

            this.TotalLocations += logicalLocations.Count;
            this.UniqueLocations += uniqueLocations.Count;

            return newLocations;
        }

        public void Trim(LogicalLocation logicalLocation)
        {
            if (logicalLocation != null)
            {
                // Leave only index if index is present
                if (logicalLocation.Index != -1)
                {
                    logicalLocation.DecoratedName = null;
                    logicalLocation.FullyQualifiedName = null;
                    logicalLocation.Name = null;
                    logicalLocation.ParentIndex = -1;
                    logicalLocation.Properties?.Clear();
                    logicalLocation.Tags?.Clear();
                }
            }
        }

        public void Trim(PhysicalLocation physicalLocation)
        {
            if (physicalLocation != null)
            {
                Trim(physicalLocation.ArtifactLocation);
                Trim(physicalLocation.Region);
                Trim(physicalLocation.ContextRegion);
            }
        }

        public void Trim(ArtifactLocation artifactLocation)
        {
            if (artifactLocation != null)
            {
                // Leave only index if index is present
                if (artifactLocation.Index != -1)
                {
                    artifactLocation.Uri = null;
                    artifactLocation.UriBaseId = null;
                    artifactLocation.Description = null;
                    artifactLocation.Tags?.Clear();
                    artifactLocation.Properties?.Clear();
                }
            }
        }

        public void Trim(Region region)
        {
            if (region == null) { return; }

            if (region.StartLine > 0)
            {
                // Remove EndLine if the same as StartLine
                if (region.EndLine == region.StartLine)
                {
                    region.EndLine = 0;
                }

                // Remove StartColumn if it's the default (1)
                if (region.StartColumn == 1)
                {
                    region.StartColumn = 0;
                }
            }

            // Remove Region components if requested and if other components are also present
            if (!this.RegionComponentsToKeep.HasFlag(RegionComponents.LineAndColumn)
                && (region.CharOffset >= 0 || region.ByteOffset >= 0))
            {
                region.StartLine = 0;
                region.StartColumn = 0;
                region.EndLine = 0;
                region.EndColumn = 0;
            }

            if (!this.RegionComponentsToKeep.HasFlag(RegionComponents.ByteOffsetAndLength)
                && (region.CharOffset >= 0 || region.StartLine > 0))
            {
                region.ByteOffset = -1;
                region.ByteLength = 0;
            }

            if (!this.RegionComponentsToKeep.HasFlag(RegionComponents.CharOffsetAndLength)
                && (region.ByteOffset >= 0 || region.StartLine > 0))
            {
                region.CharOffset = -1;
                region.CharLength = 0;
            }
        }
    }
}
