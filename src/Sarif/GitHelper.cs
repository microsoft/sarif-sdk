// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif
{
    internal class GitHelper : IDisposable
    {
        private readonly IFileSystem fileSystem;
        private readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        // TODO: This will go away because GitHelper shouldn't know anything about SARIF, such as
        // how to create a VersionControlDetails object.
        private readonly Dictionary<string, VersionControlDetails> repoRoots = new Dictionary<string, VersionControlDetails>(StringComparer.OrdinalIgnoreCase);

        // A cache that maps directory names to the root directory of the repository, if any, that
        // contains them.
        //
        // The case-insensitive key comparison is correct on Windows systems and wrong on Linux/MacOS.
        // This is a general problem in the SDK. See https://github.com/microsoft/sarif-sdk/issues/1736,
        // "File path comparisons are not file-system-appropriate".
        //
        // The cache is internal rather than private so that tests can verify that the cache is
        // being populated appropriately. It's not so easy to verify that it's being _used_
        // appropriately.
        internal readonly Dictionary<string, string> directoryToRepoRootPathDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public bool IsRepositoryRoot(string repositoryPath) => this.fileSystem.DirectoryExists(Path.Combine(repositoryPath, ".git"));

        public GitHelper(IFileSystem fileSystem = null)
        {
            this.fileSystem = fileSystem ?? new FileSystem();
            GitExePath = GetGitExePath();
        }

        public string GitExePath { get; }

        public Uri GetRemoteUri(string repoPath)
        {
            string uriText = GetSimpleGitCommandOutput(
                    repoPath,
                    args: $"remote get-url origin");

            return uriText == null
                ? null
                : new Uri(uriText, UriKind.Absolute);
        }

        public string GetCurrentCommit(string repoPath)
        {
            return GetSimpleGitCommandOutput(
                repoPath,
                args: "rev-parse --verify HEAD");
        }

        public void Checkout(string repoPath, string commitSha)
        {
            GetSimpleGitCommandOutput(
                repoPath,
                args: $"checkout {commitSha}");
        }

        private string GetGitExePath()
        {
            string gitPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Git\cmd\git.exe");
            return (!string.IsNullOrEmpty(gitPath) && this.fileSystem.FileExists(gitPath)) ? gitPath : null;
        }

        public string GetCurrentBranch(string repoPath)
        {
            return GetSimpleGitCommandOutput(
                repoPath,
                args: "rev-parse --abbrev-ref HEAD");
        }

        private string GetSimpleGitCommandOutput(string repositoryPath, string args)
        {
            string gitPath = this.GitExePath;

            if (gitPath == null || !IsRepositoryRoot(repositoryPath))
            {
                return null;
            }

            var gitCommand = new ExternalProcess(
                workingDirectory: repositoryPath,
                fileName: gitPath,
                arguments: args);

            return TrimNewlines(gitCommand.StdOut.Text);
        }

        private static string TrimNewlines(string text) => text
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

        public VersionControlDetails GetVersionControlDetails(string repositoryPath, bool crawlParentDirectories)
        {
            VersionControlDetails value;

            cacheLock.EnterReadLock();
            try
            {
                if (!crawlParentDirectories)
                {
                    if (repoRoots.TryGetValue(repositoryPath, out value))
                    {
                        return value;
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, VersionControlDetails> kp in repoRoots)
                    {
                        if (repositoryPath.StartsWith(kp.Key, StringComparison.OrdinalIgnoreCase) && ((repositoryPath.Length == kp.Key.Length) || (repositoryPath[kp.Key.Length] == '\\')))
                        {
                            return kp.Value;
                        }
                    }
                }
            }
            finally
            {
                cacheLock.ExitReadLock();
            }

            repositoryPath = GetRepositoryRoot(repositoryPath);

            if (string.IsNullOrEmpty(repositoryPath))
            {
                return null;
            }

            Uri repoRemoteUri = GetRemoteUri(repositoryPath);
            value = (repoRemoteUri is null)
                ? null
                : new VersionControlDetails
                {
                    RepositoryUri = repoRemoteUri,
                    RevisionId = GetCurrentCommit(repositoryPath),
                    Branch = GetCurrentBranch(repositoryPath),
                    MappedTo = new ArtifactLocation { Uri = new Uri(repositoryPath, UriKind.Absolute) },
                };

            cacheLock.EnterWriteLock();
            try
            {
                if (!repoRoots.ContainsKey(repositoryPath))
                {
                    repoRoots.Add(repositoryPath, value);
                }
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }

            return value;
        }

        public string GetRepositoryRoot(string path, bool useCache = true)
        {
            string repoRootPath;
            if (useCache)
            {
                if (directoryToRepoRootPathDictionary.TryGetValue(path, out repoRootPath))
                {
                    return repoRootPath;
                }
            }

            repoRootPath = path;
            while (!string.IsNullOrEmpty(repoRootPath) && !IsRepositoryRoot(repoRootPath))
            {
                repoRootPath = Path.GetDirectoryName(repoRootPath);
            }

            if (useCache)
            {
                // Add whatever we found to the cache, even if it was null (in which case we now know
                // that this path isn't under source control).
                directoryToRepoRootPathDictionary.Add(path, repoRootPath);
            }

            return repoRootPath;
        }

        /// <summary>
        /// Dispose pattern as required by style
        /// </summary>
        /// <param name="disposing">Set to true to actually dispose</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                cacheLock.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
