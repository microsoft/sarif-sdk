// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class GitHelper : IDisposable
    {
        public static readonly GitHelper Default = new GitHelper();

        public delegate string ProcessRunner(string workingDirectory, string exePath, string arguments);

        private static readonly ProcessRunner DefaultProcessRunner =
            (string workingDirectory, string exePath, string arguments)
                => new ExternalProcess(workingDirectory, exePath, arguments, stdOut: null, acceptableReturnCodes: null).StdOut.Text;

        private readonly IFileSystem fileSystem;
        private readonly ProcessRunner processRunner;
        private readonly ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();

        internal static readonly string s_expectedGitExePath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Git\cmd\git.exe");

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

        public GitHelper(IFileSystem fileSystem = null, ProcessRunner processRunner = null)
        {
            this.fileSystem = fileSystem ?? new FileSystem();
            this.processRunner = processRunner ?? DefaultProcessRunner;

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

        internal string GetGitExePath()
        {
            if (this.fileSystem.FileExists(s_expectedGitExePath))
            {
                return s_expectedGitExePath;
            }

            return FileSearcherHelper.SearchForFileInEnvironmentVariable("PATH", "git.exe");
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

            string stdOut = this.processRunner(
                workingDirectory: repositoryPath,
                exePath: gitPath,
                arguments: args);

            return TrimNewlines(stdOut);
        }

        private static string TrimNewlines(string text) => text
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

        public string GetRepositoryRoot(string path, bool useCache = true)
        {
            // The "default" instance won't let you use the cache, to prevent independent users
            // from interfering with each other.
            if (useCache && object.ReferenceEquals(this, Default))
            {
                throw new ArgumentException(SdkResources.GitHelperDefaultInstanceDoesNotPermitCaching, nameof(useCache));
            }

            if (this.fileSystem.FileExists(path))
            {
                path = Path.GetDirectoryName(path);
            }

            string repoRootPath;
            if (useCache)
            {
                cacheLock.EnterReadLock();
                try
                {
                    if (directoryToRepoRootPathDictionary.TryGetValue(path, out repoRootPath))
                    {
                        return repoRootPath;
                    }
                }
                finally
                {
                    cacheLock.ExitReadLock();
                }
            }

            repoRootPath = path;
            while (!string.IsNullOrEmpty(repoRootPath) && !IsRepositoryRoot(repoRootPath))
            {
                repoRootPath = Path.GetDirectoryName(repoRootPath);
            }

            // It's important to terminate with a slash because the value returned from this method
            // will be used to create an absolute URI on which MakeUriRelative will be called. For
            // example, suppose this method returns @"C:\\dev\sarif-sdk\". The caller will use it
            // to create an absolute URI (call it repoRootUri) "file:///C:/dev/sarif-sdk/". The
            // caller will use this URI to "rebase" another URI (call it artifactUri) such as
            // "file:///C:/dev/sarif-sdk/src/Sarif". The caller will do that by calling
            // repoRootUri.MakeRelativeUri(artifactUri). It turns out that unless repoRootUri
            // ends with a slash, this call will return "sarif-sdk/src/Sarif" rather than the
            // expected (at least for me) "src/Sarif".
            if (repoRootPath != null && !repoRootPath.EndsWith(@"\")) { repoRootPath += @"\"; }

            if (useCache)
            {
                cacheLock.EnterWriteLock();
                try
                {
                    // Add whatever we found to the cache, even if it was null (in which case we now know
                    // that this path isn't under source control).
                    directoryToRepoRootPathDictionary.Add(path, repoRootPath);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
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
