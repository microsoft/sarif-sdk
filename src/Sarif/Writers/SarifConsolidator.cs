using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Writers
{
    /// <summary>
    ///  SarifConsolidator is used to reduce the size of Sarif objects and the serialized log.
    ///  It removes common redundant properties, uses Run-wide collections to avoid writing duplicate
    ///  values, and can remove portions of Results entirely.
    /// </summary>
    public class SarifConsolidator
    {
        private Run _run;
        private Dictionary<ThreadFlowLocation, int> _uniqueThreadFlows;

        public int TotalThreadFlowLocations { get; private set; }
        public int UniqueThreadFlowLocations => _uniqueThreadFlows.Count;

        public int TotalLocations { get; private set; }
        public int UniqueLocations { get; private set; }

        public int? MessageLengthLimitBytes { get; set; }
        public bool RemoveCodeFlows { get; set; }
        public bool RemoveRelatedLocations { get; set; }
        public bool RemoveGraphs { get; set; }
        public bool RemoveGraphTraversals { get; set; }
        public bool RemoveStacks { get; set; }
        public bool RemoveWebRequests { get; set; }
        public bool RemoveWebResponses { get; set; }

        public SarifConsolidator(Run run)
        {
            _run = run;
            _uniqueThreadFlows = new Dictionary<ThreadFlowLocation, int>(ThreadFlowLocation.ValueComparer);

            
            if (_run.Artifacts != null)
            {
                // Realize Deferred Artifact list
                _run.Artifacts = _run.Artifacts?.ToList();

                // Remove indices from Run.Artifact array (redundant)
                foreach (Artifact a in _run.Artifacts)
                {
                    if (a.Location != null)
                    {
                        a.Location.Index = -1;
                    }
                }
            }

            // Ensure Run TFL list is ready for consolidating TFLs
            _run.ThreadFlowLocations = _run.ThreadFlowLocations ?? new List<ThreadFlowLocation>();

            // Add existing TFLs to map (can't consolidate; there may be existing references to them)
            for (int i = 0; i < _run.ThreadFlowLocations.Count; ++i)
            {
                ThreadFlowLocation tfl = _run.ThreadFlowLocations[i];
                Trim(tfl.Location);
                _uniqueThreadFlows[tfl] = i;
            }
        }

        public void Trim(Result result)
        {
            if (result == null) { return; }

            // Remove Result components if configured to
            result.CodeFlows = (this.RemoveCodeFlows ? null : result.CodeFlows);
            result.RelatedLocations = (this.RemoveRelatedLocations ? null : result.RelatedLocations);
            result.Graphs = (this.RemoveGraphs ? null : result.Graphs);
            result.GraphTraversals = (this.RemoveGraphTraversals ? null : result.GraphTraversals);
            result.Stacks = (this.RemoveStacks ? null : result.Stacks);
            result.WebRequest = (this.RemoveWebRequests ? null : result.WebRequest);
            result.WebResponse = (this.RemoveWebResponses ? null : result.WebResponse);

            // Truncate long Messages if configured
            if (this.MessageLengthLimitBytes > 0)
            {
                if (result?.Message?.Text?.Length > this.MessageLengthLimitBytes)
                {
                    result.Message.Text = result.Message.Text.Substring(0, this.MessageLengthLimitBytes.Value);
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

        public void Consolidate(ThreadFlow tFlow)
        {
            if (tFlow.Locations != null)
            {
                TotalThreadFlowLocations += tFlow.Locations.Count;

                for (int i = 0; i < tFlow.Locations.Count; ++i)
                {
                    ThreadFlowLocation tfl = tFlow.Locations[i];
                    Trim(tfl.Location);

                    if (tfl.Index == -1)
                    {
                        if (!_uniqueThreadFlows.TryGetValue(tfl, out int tflIndex))
                        {
                            tflIndex = _run.ThreadFlowLocations.Count;
                            _run.ThreadFlowLocations.Add(tfl);
                            _uniqueThreadFlows[tfl] = tflIndex;
                        }

                        tFlow.Locations[i] = new ThreadFlowLocation() { Index = tflIndex };
                    }
                }
            }
        }

        public IList<Location> Trim(IList<Location> locations)
        {
            if (locations == null || locations.Count == 0) { return locations; }

            HashSet<Location> uniqueLocations = new HashSet<Location>(Location.ValueComparer);
            List<Location> newLocations = new List<Location>();

            this.TotalLocations += locations.Count;
            for (int i = 0; i < locations.Count; ++i)
            {
                Location loc = locations[i];

                // Trim Locations themselves
                Trim(loc);

                // Keep only one copy of each unique location
                if (uniqueLocations.Add(loc))
                {
                    this.UniqueLocations++;
                    newLocations.Add(loc);
                }
            }

            return newLocations;
        }

        public void Trim(Location loc)
        {
            if (loc != null)
            {
                // Remove Ids
                loc.Id = -1;

                Trim(loc.PhysicalLocation);
                Trim(loc.LogicalLocation);
                Trim(loc.LogicalLocations);
            }
        }

        public IList<LogicalLocation> Trim(IList<LogicalLocation> locations)
        {
            if (locations == null || locations.Count == 0) { return locations; }

            HashSet<LogicalLocation> uniqueLocations = new HashSet<LogicalLocation>(LogicalLocation.ValueComparer);
            List<LogicalLocation> newLocations = new List<LogicalLocation>();

            this.TotalLocations += locations.Count;
            for (int i = 0; i < locations.Count; ++i)
            {
                LogicalLocation loc = locations[i];

                // Trim Locations themselves
                Trim(loc);

                // Keep only one copy of each unique location
                if (uniqueLocations.Add(loc))
                {
                    this.UniqueLocations++;
                    newLocations.Add(loc);
                }
            }

            return newLocations;
        }

        public void Trim(LogicalLocation loc)
        {
            if (loc != null)
            {
                // Leave only index if index is present
                if (loc.Index != -1)
                {
                    loc.DecoratedName = null;
                    loc.FullyQualifiedName = null;
                    loc.Name = null;
                    loc.ParentIndex = -1;
                    loc.Properties?.Clear();
                    loc.Tags?.Clear();
                }
            }
        }

        public void Trim(PhysicalLocation loc)
        {
            if (loc != null)
            {
                Trim(loc.ArtifactLocation);
                Trim(loc.Region);
                Trim(loc.ContextRegion);
            }
        }

        public void Trim(ArtifactLocation a)
        {
            if (a != null)
            {
                // Leave only index if index is present
                if (a.Index != -1)
                {
                    a.Uri = null;
                    a.UriBaseId = null;
                    a.Description = null;
                    a.Tags?.Clear();
                    a.Properties?.Clear();
                }
            }
        }

        public void Trim(Region r)
        {
            if (r != null && r.StartLine > 0)
            {
                // Remove EndLine if the same as StartLine
                if (r.EndLine == r.StartLine)
                {
                    r.EndLine = 0;
                }

                // Remove Offset/Length if Line/Column available
                r.CharOffset = -1;
                r.CharLength = 0;
                r.ByteOffset = -1;
                r.ByteLength = 0;
            }
        }
    }
}
