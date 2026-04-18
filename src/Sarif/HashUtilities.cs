// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Sarif.Numeric;

namespace Microsoft.CodeAnalysis.Sarif
{
    public static class HashUtilities
    {
        private static readonly int TAB = '\t';
        private static readonly int SPACE = ' ';
        private static readonly int LF = '\n';
        private static readonly int CR = '\r';
        private static readonly int EOF = 65535;
        private static readonly int BLOCK_SIZE = 100;
        private static readonly Long MOD = new Long(37, 0, false);

        public static IDictionary<string, HashData> MultithreadedComputeTargetFileHashes(IEnumerable<string> analysisTargets, bool suppressConsoleOutput = false)
        {
            if (analysisTargets == null) { return null; }

            if (!suppressConsoleOutput)
            {
                Console.WriteLine("Computing file hashes...");
            }

            var fileToHashDataMap = new ConcurrentDictionary<string, HashData>();

            var queue = new ConcurrentQueue<string>(analysisTargets);

            var tasks = new List<Task>();

            while (!queue.IsEmpty && tasks.Count < Environment.ProcessorCount)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    while (!queue.IsEmpty)
                    {
                        if (queue.TryDequeue(out string filePath))
                        {
                            fileToHashDataMap[filePath] = HashUtilities.ComputeHashes(filePath);
                        }
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());

            if (!suppressConsoleOutput)
            {
                Console.WriteLine("Hash computation complete.");
            }

            return fileToHashDataMap;
        }

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5354:SHA1CannotBeUsed")]
        [SuppressMessage("Microsoft.Security.Cryptography", "CA5350:MD5CannotBeUsed")]
        public static HashData ComputeHashes(string fileName, IFileSystem fileSystem = null)
        {
            fileSystem ??= FileSystem.Instance;

            try
            {
                using (Stream stream = fileSystem.FileOpenRead(fileName))
                {
                    // This condition is actually only feasible in testing, as a null
                    // value will only be returned by a mock object that doesn't 
                    // recognize the current specified file argument. In production,
                    // an exception will always be raised for a missing file. If
                    // we enter the code below, that indicates that a test 
                    // encountered an adverse condition and is attempting to produce
                    // a file hash for some source file in a notification stack. We
                    // return null here, as the actual source hash isn't interesting
                    // for this scenario, and we want to reliably finish test execution
                    // and record exception details.
                    if (stream == null) { return null; }

                    return ComputeHashes(stream);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return null;
        }

        public static HashData ComputeHashes(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
            {
                string sha1, sha256;
                byte[] checksum;

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

                return new HashData(sha1, sha256);
            }
        }

        public static HashData ComputeHashesForText(string text)
        {
            text ??= string.Empty;
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
            return ComputeHashes(stream);
        }

        public static string ComputeSha256Hash(string fileName, IFileSystem fileSystem = null)
        {
            fileSystem ??= FileSystem.Instance;

            string sha256Hash = null;

            try
            {
                using (Stream stream = fileSystem.FileOpenRead(fileName))
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

        public static string ComputeStringSha256Hash(string text)
        {
            using var sha = SHA256.Create();
            byte[] byteHash = Encoding.UTF8.GetBytes(text);
            byte[] checksum = sha.ComputeHash(byteHash);
            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }

        [SuppressMessage("Microsoft.Security.Cryptography", "CA5354:SHA1CannotBeUsed")]
        public static string ComputeSha1Hash(string fileName, IFileSystem fileSystem = null)
        {
            fileSystem ??= FileSystem.Instance;
            string sha1 = null;

            try
            {
                using (Stream stream = fileSystem.FileOpenRead(fileName))
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

        public static Dictionary<int, string> RollingHash(string fileText)
        {
            var rollingHashes = new Dictionary<int, string>();

            // A rolling view into the input
            int[] window = new int[BLOCK_SIZE];

            int[] lineNumbers = new int[BLOCK_SIZE];
            for (int i = 0; i < lineNumbers.Length; i++)
            {
                lineNumbers[i] = -1;
            }

            var hashRaw = new Long(0, 0, false);
            Long firstMod = ComputeFirstMod();

            // The current index in the window, will wrap around to zero when we reach BLOCK_SIZE
            int index = 0;
            // The line number of the character we are currently processing from the input
            int lineNumber = 0;
            // Is the next character to be read the start of a new line
            bool lineStart = true;
            // Was the previous character a CR (carriage return)
            bool prevCR = false;

            var hashCounts = new Dictionary<string, int>();

            // Output the current hash and line number to the cache
            Action outputHash = () =>
            {
                string hashValue = hashRaw.ToUnsigned().ToString(16);

                if (!hashCounts.ContainsKey(hashValue))
                {
                    hashCounts[hashValue] = 0;
                }

                hashCounts[hashValue]++;
                rollingHashes[lineNumbers[index]] = $"{hashValue}:{hashCounts[hashValue]}";
                lineNumbers[index] = -1;
            };

            // Update the current hash value and increment the index in the window
            Action<int> updateHash = (current) =>
            {
                int begin = window[index];
                window[index] = current;

                hashRaw = MOD.Multiply(hashRaw)
                            .Add(Long.FromInt(current))
                            .Subtract(firstMod.Multiply(Long.FromInt(begin)));

                index = (index + 1) % BLOCK_SIZE;
            };

            // First process every character in the input, updating the hash and lineNumbers
            // as we go. Once we reach a point in the window again then we've processed
            // BLOCK_SIZE characters and if the last character at this point in the window
            // was the start of a line then we should output the hash for that line.
            Action<int> processCharacter = (current) =>
            {
                // Skip tabs, spaces, and line feeds that come directly after a carriage return.
                if (current == SPACE || current == TAB || (prevCR && current == LF))
                {
                    prevCR = false;
                    return;
                }
                // Replace CR with LF.
                // Note that we do not handle /u2028 (Unicode linefeed)
                // or /u2029 (Unicode paragraph feed) characters.
                if (current == CR)
                {
                    current = LF;
                    prevCR = true;
                }
                else
                {
                    prevCR = false;
                }
                if (lineNumbers[index] != -1)
                {
                    outputHash();
                }
                if (lineStart)
                {
                    lineStart = false;
                    lineNumber++;
                    lineNumbers[index] = lineNumber;
                }
                if (current == LF)
                {
                    lineStart = true;
                }
                updateHash(current);
            };

            if (fileText != null)
            {
                for (int i = 0; i < fileText.Length; i++)
                {
                    processCharacter(fileText[i]);
                }

                processCharacter(EOF);

                // Flush the remaining lines
                for (int i = 0; i < BLOCK_SIZE; i++)
                {
                    if (lineNumbers[index] != -1)
                    {
                        outputHash();
                    }
                    updateHash(0);
                }
            }

            return rollingHashes;
        }

        private static Long ComputeFirstMod()
        {
            var firstMod = new Long(1, 0, false);

            for (int i = 0; i < 100; i++)
            {
                firstMod = firstMod.Multiply(MOD);
            }

            return firstMod;
        }


        public static string ComputeSha256HashValue(string value)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
            }
        }
    }
}
