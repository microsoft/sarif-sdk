// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class HashUtilities
    {
        static HashUtilities() => FileSystem = new FileSystem();

        private static IFileSystem _fileSystem;
        internal static IFileSystem FileSystem
        {
            get
            {
                _fileSystem = _fileSystem ?? new FileSystem();
                return _fileSystem;
            }

            set
            {
                _fileSystem = value;
            }
        }

        public static IDictionary<string, HashData> MultithreadedComputeTargetFileHashes(IEnumerable<string> analysisTargets)
        {
            if (analysisTargets == null) { return null; }

            Console.WriteLine("Computing file hashes...");
            var fileToHashDataMap = new ConcurrentDictionary<string, HashData>();

            var queue = new ConcurrentQueue<string>(analysisTargets);

            var tasks = new List<Task>();

            while (queue.Count > 0 && tasks.Count < Environment.ProcessorCount)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    while (queue.Count > 0)
                    {
                        if (queue.TryDequeue(out string filePath))
                        {
                            fileToHashDataMap[filePath] = HashUtilities.ComputeHashes(filePath);
                        }
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Hash computation complete.");
            return fileToHashDataMap;
        }

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5354:SHA1CannotBeUsed")]
        [SuppressMessage("Microsoft.Security.Cryptography", "CA5350:MD5CannotBeUsed")]
        public static HashData ComputeHashes(string fileName)
        {
            try
            {
                using (Stream stream = FileSystem.OpenRead(fileName))
                {
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                    {
                        string md5, sha1, sha256;
                        byte[] checksum;

                        using (var md5Cng = MD5.Create())
                        {
                            checksum = md5Cng.ComputeHash(bufferedStream);
                            md5 = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }

                        stream.Seek(0, SeekOrigin.Begin);
                        bufferedStream.Seek(0, SeekOrigin.Begin);

                        using (var sha1Cng = SHA1.Create())
                        {
                            checksum = sha1Cng.ComputeHash(bufferedStream);
                            sha1 = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }

                        stream.Seek(0, SeekOrigin.Begin);
                        bufferedStream.Seek(0, SeekOrigin.Begin);

                        using (var sha256Cng = SHA256.Create())
                        {
                            checksum = sha256Cng.ComputeHash(bufferedStream);
                            sha256 = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }

                        return new HashData(md5, sha1, sha256);
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return null;
        }

        public static string ComputeSha256Hash(string fileName)
        {
            string sha256Hash = null;

            try
            {
                using (Stream stream = FileSystem.OpenRead(fileName))
                {
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                    {
                        using (var sha = SHA256.Create())
                        {
                            byte[] checksum = sha.ComputeHash(bufferedStream);
                            sha256Hash = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return sha256Hash;
        }

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5354:SHA1CannotBeUsed")]
        public static string ComputeSha1Hash(string fileName)
        {
            string sha1 = null;

            try
            {
                using (Stream stream = FileSystem.OpenRead(fileName))
                {
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                    {
                        using (var sha = SHA1.Create())
                        {
                            byte[] checksum = sha.ComputeHash(bufferedStream);
                            sha1 = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return sha1;
        }

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5350:MD5CannotBeUsed")]
        public static string ComputeMD5Hash(string fileName)
        {
            string md5 = null;

            try
            {
                using (Stream stream = FileSystem.OpenRead(fileName))
                {
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
                    {
                        using (var sha = MD5.Create())
                        {
                            byte[] checksum = sha.ComputeHash(bufferedStream);
                            md5 = BitConverter.ToString(checksum).Replace("-", string.Empty);
                        }
                    }
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return md5;
        }
    }
}
