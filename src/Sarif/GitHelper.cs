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

        private static string DefaultProcessRunnerImpl(string workingDirectory, string exePath, string arguments)
        {
            return new ExternalProcess(workingDirectory, exePath, arguments, stdOut: null, acceptableReturnCodes: null).StdOut.Text;
        }

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

        public bool IsRepositoryRoot(string repoPath)
        {
            repoPath = repoPath.EndsWith(@"\") ?
                repoPath.Substring(0, repoPath.Length - 1) :
                repoPath;

            return repoPath.Equals(GetTopLevel(repoPath), StringComparison.OrdinalIgnoreCase);
        }

        public GitHelper(IFileSystem fileSystem = null, ProcessRunner processRunner = null)
        {
            this.fileSystem = fileSystem ?? FileSystem.Instance;

            this.processRunner = processRunner ?? DefaultProcessRunnerImpl;

            GitExePath = GetGitExePath(this.fileSystem);
        }

        public string GitExePath { get; set; }

        public Uri GetRemoteUri(string repoPath)
        {
            string uriText = GetSimpleGitCommandOutput(
                    repoPath,
                    args: "remote get-url origin");

            // Sometimes, the uri contains the following format:
            // user:password@repositoryurl. With that, we are removing
            // the sensitive information from it.
            if (!string.IsNullOrEmpty(uriText) && uriText.Contains("@"))
            {
                int index = uriText.IndexOf('@');
                uriText = $"https://{uriText.Substring(index + 1)}";
            }

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

        public string GetBlame(string filePath)
        {
            filePath = Path.GetFullPath(filePath);
            return GetSimpleGitCommandOutput(
                GetTopLevel(filePath),
                args: $"blame -f --porcelain {filePath}",
                trimLines: false);
        }

        public void Checkout(string repoPath, string commitSha)
        {
            GetSimpleGitCommandOutput(
                repoPath,
                args: $"checkout {commitSha}");
        }

        internal static string GetGitExePath(IFileSystem fileSystem)
        {
            if (fileSystem.FileExists(s_expectedGitExePath))
            {
                return s_expectedGitExePath;
            }

            return FileSearcherHelper.SearchForFileInEnvironmentVariable("PATH", "git.exe", fileSystem);
        }

        public string GetCurrentBranch(string repoPath)
        {
            return GetSimpleGitCommandOutput(
                repoPath,
                args: "rev-parse --abbrev-ref HEAD");
        }

        public string GetTopLevel(string repoPath)
        {
            const string args = "rev-parse --show-toplevel";
            string currentDirectory = this.fileSystem.EnvironmentCurrentDirectory;

            try
            {
                string gitPath = this.GitExePath;

                if (gitPath == null)
                {
                    return null;
                }

                if (!this.fileSystem.DirectoryExists(repoPath) &&
                    !this.fileSystem.FileExists(repoPath))
                {
                    return null;
                }

                if (this.fileSystem.FileExists(repoPath))
                {
                    repoPath = Path.GetDirectoryName(repoPath);
                }

                this.fileSystem.EnvironmentCurrentDirectory = Path.GetDirectoryName(repoPath);

                string stdOut = this.processRunner(
                    workingDirectory: repoPath,
                    exePath: gitPath,
                    arguments: args);

                // git always emits POSIX-style forward slashes regardless of host OS, but callers
                // on Windows expect platform-native separators (Path.Combine, file existence
                // checks, etc. round-trip cleanly only against the native separator). Normalize
                // by piping through DirectoryInfo.FullName.
                return stdOut != null ?
                    new DirectoryInfo(TrimNewlines(stdOut)).FullName :
                    null;
            }
            finally
            {
                this.fileSystem.EnvironmentCurrentDirectory = currentDirectory;
            }
        }

        private string GetSimpleGitCommandOutput(string repoPath, string args, bool trimLines = true)
        {
            string currentDirectory = this.fileSystem.EnvironmentCurrentDirectory;

            // Accept either separator from callers — git itself, on every platform, emits forward
            // slashes — but normalize for the file-system operations below.
            if (repoPath != null)
            {
                repoPath = NormalizeDirectorySeparators(repoPath);
            }

            try
            {
                string gitPath = this.GitExePath;

                if (gitPath == null || !IsRepositoryRoot(repoPath))
                {
                    return null;
                }

                this.fileSystem.EnvironmentCurrentDirectory = repoPath;

                string stdOut = this.processRunner(
                    workingDirectory: repoPath,
                    exePath: gitPath,
                    arguments: args);

                return stdOut != null ?
                    (trimLines ? TrimNewlines(stdOut) : stdOut) :
                    null;
            }
            finally
            {
                this.fileSystem.EnvironmentCurrentDirectory = currentDirectory;
            }
        }

        private static string TrimNewlines(string text) => text
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

        /// <summary>
        /// Normalizes a path so that all directory-separator characters match
        /// <see cref="Path.DirectorySeparatorChar"/>. <c>git</c> always emits POSIX-style forward
        /// slashes regardless of host OS, but downstream code on Windows
        /// (<c>Path.Combine</c>, <see cref="IFileSystem.FileExists"/>, dictionary key comparisons
        /// against earlier outputs of <see cref="System.IO.DirectoryInfo.FullName"/>) only
        /// round-trips cleanly against the platform-native separator. Without this normalization,
        /// paths returned by <see cref="GetRepositoryRoot"/> and other path-returning helpers
        /// carry forward-slash segments through to callers, which silently break later path
        /// arithmetic on Windows.
        /// </summary>
        internal static string NormalizeDirectorySeparators(string path)
        {
            if (string.IsNullOrEmpty(path)) { return path; }

            if (Path.DirectorySeparatorChar == '/')
            {
                // POSIX: forward slashes are native; just collapse any stray backslashes.
                return path.Replace('\\', '/');
            }

            // Windows: collapse forward slashes to backslashes.
            return path.Replace('/', '\\');
        }

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

            string topLevelPath = GetTopLevel(path);

            // Normalize directory separators to match Path.DirectorySeparatorChar so callers on
            // Windows can reliably compare/combine the returned path with other native paths.
            repoRootPath = topLevelPath != null ?
                NormalizeDirectorySeparators(new DirectoryInfo(GetTopLevel(path)).FullName) :
                null;

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
