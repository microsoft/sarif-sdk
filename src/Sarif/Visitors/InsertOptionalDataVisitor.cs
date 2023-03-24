// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class InsertOptionalDataVisitor : SarifRewritingVisitor
    {
        private static readonly Regex versionControlPropertiesRegex =
            new Regex(@"(?i)runs\[(?<run>\d?)\]\.invocations\[(?<invocation>\d?)\].versionControlProvenance.properties.(?<property>[a-zA-Z]+)=(?<value>.*)", RegexOptions.Compiled);

        private readonly IFileSystem _fileSystem;
        private readonly GitHelper.ProcessRunner _processRunner;

        private Run _run;
        private HashSet<Uri> _repoRootUris;
        private GitHelper _gitHelper;

        private int _ruleIndex;
        private readonly OptionallyEmittedData _dataToInsert;
        private readonly IDictionary<string, ArtifactLocation> _originalUriBaseIds;
        private readonly IEnumerable<string> _insertProperties;

        private const string Name = nameof(Name);
        private const string Email = nameof(Email);
        private const string CommitSha = nameof(CommitSha);

        public InsertOptionalDataVisitor(OptionallyEmittedData dataToInsert,
                                         FileRegionsCache fileRegionsCache,
                                         Run run,
                                         IEnumerable<string> insertProperties)
            : this(dataToInsert,
                   originalUriBaseIds: run?.OriginalUriBaseIds,
                   insertProperties: insertProperties,
                   fileRegionsCache: fileRegionsCache)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
        }

        public InsertOptionalDataVisitor(
            OptionallyEmittedData dataToInsert,
            FileRegionsCache fileRegionsCache,
            IDictionary<string, ArtifactLocation> originalUriBaseIds = null,
            IFileSystem fileSystem = null,
            GitHelper.ProcessRunner processRunner = null,
            IEnumerable<string> insertProperties = null)
        {
            _fileSystem = fileSystem ?? FileSystem.Instance;
            _processRunner = processRunner;

            _ruleIndex = -1;
            _gitHelper = new GitHelper();
            _dataToInsert = dataToInsert;
            _originalUriBaseIds = originalUriBaseIds;
            _insertProperties = insertProperties ?? new List<string>();

            FileRegionsCache = fileRegionsCache;
        }

        public FileRegionsCache FileRegionsCache { get; set; }

        public override Run VisitRun(Run node)
        {
            _run = node;
            _gitHelper = new GitHelper(_fileSystem, _processRunner);
            _repoRootUris = new HashSet<Uri>();

            if (_originalUriBaseIds != null)
            {
                _run.OriginalUriBaseIds ??= new Dictionary<string, ArtifactLocation>();

                foreach (string key in _originalUriBaseIds.Keys)
                {
                    _run.OriginalUriBaseIds[key] = _originalUriBaseIds[key];
                }
            }

            if (node == null) { return null; }

            bool scrapeFileReferences = _dataToInsert.HasFlag(OptionallyEmittedData.Hashes) ||
                                        _dataToInsert.HasFlag(OptionallyEmittedData.TextFiles) ||
                                        _dataToInsert.HasFlag(OptionallyEmittedData.BinaryFiles);

            if (scrapeFileReferences)
            {
                var visitor = new AddFileReferencesVisitor();
                visitor.VisitRun(node);
            }

            Run visited = base.VisitRun(node);

            // After all the ArtifactLocations have been visited,
            if (_run.VersionControlProvenance == null && _dataToInsert.HasFlag(OptionallyEmittedData.VersionControlDetails))
            {
                _run.VersionControlProvenance = CreateVersionControlProvenance();
            }

            return visited;
        }

        public override PhysicalLocation VisitPhysicalLocation(PhysicalLocation node)
        {
            if (node.Region == null || node.Region.IsBinaryRegion)
            {
                goto Exit;
            }

            bool insertRegionSnippets = _dataToInsert.HasFlag(OptionallyEmittedData.RegionSnippets);
            bool overwriteExistingData = _dataToInsert.HasFlag(OptionallyEmittedData.OverwriteExistingData);
            bool insertContextCodeSnippets = _dataToInsert.HasFlag(OptionallyEmittedData.ContextRegionSnippets);
            bool populateRegionProperties = _dataToInsert.HasFlag(OptionallyEmittedData.ComprehensiveRegionProperties);

            if (insertRegionSnippets || populateRegionProperties || insertContextCodeSnippets)
            {
                Uri resolvedUri = GetResolvedArtifactLocationUri(node.ArtifactLocation);

                Region expandedRegion = FileRegionsCache.PopulateTextRegionProperties(node.Region, resolvedUri, populateSnippet: insertRegionSnippets);

                ArtifactContent originalSnippet = node.Region.Snippet;

                if (populateRegionProperties)
                {
                    node.Region = expandedRegion;
                }

                node.Region.Snippet = originalSnippet == null || overwriteExistingData
                    ? expandedRegion.Snippet
                    : originalSnippet;

                if (insertContextCodeSnippets && (node.ContextRegion == null || overwriteExistingData))
                {
                    node.ContextRegion = FileRegionsCache.ConstructMultilineContextSnippet(expandedRegion, resolvedUri);
                }
            }

        Exit:
            return base.VisitPhysicalLocation(node);
        }

        public override Artifact VisitArtifact(Artifact node)
        {
            ArtifactLocation fileLocation = node.Location;
            if (fileLocation != null)
            {
                bool workToDo = false;
                bool overwriteExistingData = _dataToInsert.HasFlag(OptionallyEmittedData.OverwriteExistingData);

                workToDo |= (node.Hashes == null || overwriteExistingData) && _dataToInsert.HasFlag(OptionallyEmittedData.Hashes);
                workToDo |= (node.Contents?.Text == null || overwriteExistingData) && _dataToInsert.HasFlag(OptionallyEmittedData.TextFiles);
                workToDo |= (node.Contents?.Binary == null || overwriteExistingData) && _dataToInsert.HasFlag(OptionallyEmittedData.BinaryFiles);

                if (workToDo)
                {
                    if (fileLocation.TryReconstructAbsoluteUri(_run.OriginalUriBaseIds, out Uri uri))
                    {
                        Encoding encoding = null;

                        string encodingText = node.Encoding ?? _run.DefaultEncoding;

                        if (!string.IsNullOrWhiteSpace(encodingText))
                        {
                            try
                            {
                                encoding = Encoding.GetEncoding(encodingText);
                            }
                            catch (ArgumentException) { }
                        }

                        int length = node.Length;
                        node = Artifact.Create(uri, _dataToInsert, encoding: encoding);
                        node.Length = length;
                        fileLocation.Index = -1;
                        node.Location = fileLocation;
                    }
                }
            }

            return base.VisitArtifact(node);
        }

        public override ArtifactLocation VisitArtifactLocation(ArtifactLocation node)
        {
            if (_dataToInsert.HasFlag(OptionallyEmittedData.VersionControlDetails))
            {
                node = ExpressRelativeToRepoRoot(node);
            }

            return base.VisitArtifactLocation(node);
        }

        public override Result VisitResult(Result node)
        {
            _ruleIndex = node.RuleIndex;
            node = base.VisitResult(node);
            _ruleIndex = -1;

            if (node.Guid == null && _dataToInsert.HasFlag(OptionallyEmittedData.Guids))
            {
                node.Guid = Guid.NewGuid();
            }

            if (_dataToInsert.HasFlag(OptionallyEmittedData.GitBlameInformation))
            {
                // add git blame information to the result
                string filePath = GetFilePath(node.Locations[0].PhysicalLocation.ArtifactLocation);

                if (filePath != null)
                {
                    IEnumerable<IBlameHunk> blameHunks = SarifTransformerUtilities.ParseBlameInformation(
                                                                                        _gitHelper.GetBlame(filePath));

                    Region region = node.Locations[0].PhysicalLocation.Region;
                    if (region != null)
                    {
                        foreach (IBlameHunk blameHunk in blameHunks)
                        {
                            if (!blameHunk.ContainsLine(region.StartLine ?? 0))
                            {
                                continue;
                            }
                            node.SetProperty(CommitSha, blameHunk.CommitSha);
                            node.SetProperty(Email, blameHunk.Email);
                            node.SetProperty(Name, blameHunk.Name);
                            break;
                        }
                    }
                }
            }

            if (_dataToInsert.HasFlag(OptionallyEmittedData.ContextRegionSnippetPartialFingerprints) &&
                (node.PartialFingerprints == null ||
                 !node.PartialFingerprints.ContainsKey(ContextRegionHash) ||
                 _dataToInsert.HasFlag(OptionallyEmittedData.OverwriteExistingData)))
            {
                Location primaryLocation = node.Locations?.FirstOrDefault();
                if (primaryLocation != null)
                {
                    PhysicalLocation physicalLocation = primaryLocation.PhysicalLocation;
                    Uri resolvedUri = GetResolvedArtifactLocationUri(physicalLocation.ArtifactLocation);

                    ArtifactContent contextRegionSnippet = physicalLocation.ContextRegion?.Snippet;

                    if (contextRegionSnippet == null)
                    {
                        Region expandedRegion =
                            FileRegionsCache.PopulateTextRegionProperties(physicalLocation.Region, resolvedUri, false);
                        contextRegionSnippet = FileRegionsCache.ConstructMultilineContextSnippet(expandedRegion, resolvedUri)?.Snippet;
                    }

                    if (contextRegionSnippet?.Text != null)
                    {
                        string contextRegionHash = HashUtilities.ComputeStringSha256Hash(contextRegionSnippet.Text);

                        node.PartialFingerprints ??= new Dictionary<string, string>();

                        SarifUtilities.AddOrUpdateDictionaryEntry(node.PartialFingerprints, ContextRegionHash, contextRegionHash);
                    }
                }
            }

            return node;
        }

        public override Message VisitMessage(Message message)
        {
            if ((message.Text == null || _dataToInsert.HasFlag(OptionallyEmittedData.OverwriteExistingData)) &&
                _dataToInsert.HasFlag(OptionallyEmittedData.FlattenedMessages))
            {
                message.Flatten(_ruleIndex, _run);
            }
            return base.VisitMessage(message);
        }

        private List<VersionControlDetails> CreateVersionControlProvenance()
        {
            var matches = new List<Match>();
            var versionControlProvenance = new List<VersionControlDetails>();

            foreach (string property in _insertProperties)
            {
                Match match = versionControlPropertiesRegex.Match(property);
                if (match.Success)
                {
                    matches.Add(match);
                }
            }

            foreach (Uri repoRootUri in _repoRootUris)
            {
                string repoRootPath = repoRootUri.LocalPath;
                Uri repoRemoteUri = _gitHelper.GetRemoteUri(repoRootPath);
                if (repoRemoteUri != null)
                {
                    var versionControlDetail = new VersionControlDetails
                    {
                        RepositoryUri = repoRemoteUri,
                        RevisionId = _gitHelper.GetCurrentCommit(repoRootPath),
                        Branch = _gitHelper.GetCurrentBranch(repoRootPath),
                        MappedTo = new ArtifactLocation { Uri = repoRootUri }
                    };

                    foreach (Match match in matches)
                    {
                        versionControlDetail.SetProperty(match.Groups["property"].Value, match.Groups["value"].Value);
                    }

                    versionControlProvenance.Add(versionControlDetail);
                }
            }

            return versionControlProvenance;
        }

        private ArtifactLocation ExpressRelativeToRepoRoot(ArtifactLocation node)
        {
            Uri uri = node.Uri;
            if (uri == null && node.Index >= 0 && _run.Artifacts?.Count > node.Index)
            {
                uri = _run.Artifacts[node.Index].Location.Uri;
            }

            if (uri != null && uri.IsAbsoluteUri && uri.IsFile)
            {
                string repoRootPath = _gitHelper.GetRepositoryRoot(uri.LocalPath);
                if (repoRootPath != null)
                {
                    var repoRootUri = new Uri(repoRootPath + @"\", UriKind.Absolute);

                    _repoRootUris ??= new HashSet<Uri>();
                    _repoRootUris.Add(repoRootUri);

                    Uri relativeUri = repoRootUri.MakeRelativeUri(uri);
                    if (!string.IsNullOrEmpty(relativeUri.OriginalString))
                    {
                        node.Uri = relativeUri;
                        node.UriBaseId = GetUriBaseIdForRepoRoot(repoRootUri);
                    }
                }
            }

            return node;
        }

        private string GetUriBaseIdForRepoRoot(Uri repoRootUri)
        {
            // Is there already an entry in OriginalUriBaseIds for this repo?
            if (_run.OriginalUriBaseIds != null)
            {
                foreach (KeyValuePair<string, ArtifactLocation> pair in _run.OriginalUriBaseIds)
                {
                    if (pair.Value.Uri == repoRootUri)
                    {
                        // Yes, so return it.
                        return pair.Key;
                    }
                }
            }

            // No, so add one.
            _run.OriginalUriBaseIds ??= new Dictionary<string, ArtifactLocation>();

            string uriBaseId = GetNextRepoRootUriBaseId();
            _run.OriginalUriBaseIds.Add(
                uriBaseId,
                new ArtifactLocation
                {
                    Uri = repoRootUri
                });

            return uriBaseId;
        }

        private string GetNextRepoRootUriBaseId()
        {
            ICollection<string> originalUriBaseIdSymbols = _run.OriginalUriBaseIds.Keys;

            for (int i = 0; ; i++)
            {
                string uriBaseId = GetUriBaseId(i);
                if (!originalUriBaseIdSymbols.Contains(uriBaseId))
                {
                    return uriBaseId;
                }
            }
        }

        private string GetFilePath(ArtifactLocation node)
        {
            Uri uri = node.Uri;
            if (uri == null)
            {
                uri = node.Resolve(_run).Uri;
            }
            else
            {
                node.TryReconstructAbsoluteUri(_run.OriginalUriBaseIds, out uri);
            }
            return uri.IsAbsoluteUri && uri.IsFile ? uri.GetFilePath() : null;
        }

        private Uri GetResolvedArtifactLocationUri(ArtifactLocation artifactLocation)
        {
            if (artifactLocation.Uri == null && artifactLocation.Index >= 0)
            {
                // Uri is not stored at result level, but we have an index to go look in run.Artifacts
                // we must pick the ArtifactLocation details from run.artifacts array
                Artifact artifactFromRun = _run.Artifacts[artifactLocation.Index];
                artifactLocation = artifactFromRun.Location;
            }

            // If we can resolve a file location to a newly constructed
            // absolute URI, we will prefer that
            if (!artifactLocation.TryReconstructAbsoluteUri(_run.OriginalUriBaseIds, out Uri resolvedUri))
            {
                resolvedUri = artifactLocation.Uri;
            }

            return resolvedUri;
        }

        private const string RepoRootUriBaseIdStem = "REPO_ROOT";
        public const string ContextRegionHash = "contextRegionHash/v1";

        // When there is only one repo root (the usual case), the uriBaseId is "REPO_ROOT" (unless
        // that symbol is already in use in originalUriBaseIds. The second and subsequent uriBaseIds
        // are REPO_ROOT_2, _3, etc. (again, skipping over any that are in use). We never assign
        // REPO_ROOT_1 (although of course it might exist in originalUriBaseIds).
        internal static string GetUriBaseId(int i)
            => i == 0
            ? RepoRootUriBaseIdStem
            : $"{RepoRootUriBaseIdStem}_{i + 1}";
    }
}
