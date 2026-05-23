// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif
{
    public class HashUtilitiesTests
    {
        [Fact]
        public void HashUtilities_ShouldRespectQuietFlag()
        {
            int numberOfTestFiles = 10;

            var filePaths = new List<string>(numberOfTestFiles);

            for (int i = 0; i < numberOfTestFiles; i++)
            {
                string filePath = Path.GetTempFileName();
                string fileContents = Guid.NewGuid().ToString();

                File.WriteAllText(filePath, fileContents);

                filePaths.Add(filePath);
            }

            //  The easy path, simply calculate the hashes.
            //  We should write to the Console with the default TextWriter.
            try
            {
                IDictionary<string, HashData> hashes = HashUtilities.MultithreadedComputeTargetFileHashes(filePaths);
            }
            catch (Exception e)
            {
                //  If anything fails, delete the files and fail the test
                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath)) { File.Delete(filePath); }
                }

                Assert.True(false, e.Message);
            }

            //  An custom textwriter.S
            //  Using it should throw an InvalidOperationException when writing a string
            var testTextWriter = new TestTextWriter();
            TextWriter defaultOut = Console.Out;
            Console.SetOut(testTextWriter);

            try
            {
                IDictionary<string, HashData> hashes = HashUtilities.MultithreadedComputeTargetFileHashes(filePaths);

                //  If we got here, things went wrong, so cleanup
                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath)) { File.Delete(filePath); }
                }
                Console.SetOut(defaultOut);

                Assert.True(false, "Exception was expected but not thrown");
            }
            catch (InvalidOperationException)
            {
                //  Do nothing, this is the desired behavior.
            }

            //  Mimic the '--quiet true'.  We should not use the console this time.
            try
            {
                IDictionary<string, HashData> hashes = HashUtilities.MultithreadedComputeTargetFileHashes(filePaths, true);
            }
            catch
            {
                Assert.True(false, "Unexpected exception.");
            }
            finally
            {
                foreach (string filePath in filePaths)
                {
                    if (File.Exists(filePath)) { File.Delete(filePath); }
                }

                Console.SetOut(defaultOut);
            }
        }

        [Fact]
        public void RollingHash_EmptyString()
        {
            string testFileText = string.Empty;
            var expectedOutput = new Dictionary<int, string>();
            expectedOutput.Add(1, "c129715d7a2bc9a3:1");

            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);
            Assert.Equal(expectedOutput, actualOutput);
        }

        // The following set of unit tests, prefixed with "RollingHash_", are based on tests from
        // https://github.com/github/codeql-action/blob/main/src/fingerprints.test.ts
        [Fact]
        public void RollingHash_NewLineCombo1()
        {
            // Assume
            string testFileText = " a\nb\n  \t\tc\n d";
            var expectedOutput = new Dictionary<int, string>()
            {
                { 1, "271789c17abda88f:1" },
                { 2, "54703d4cd895b18:1" },
                { 3, "180aee12dab6264:1" },
                { 4, "a23a3dc5e078b07b:1" }
            };

            // Act
            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void RollingHash_NewLineCombo2()
        {
            // Assume
            string testFileText = " hello; \t\nworld!!!\n\n\n  \t\tGreetings\n End";
            var expectedOutput = new Dictionary<int, string>()
            {
                {1, "8b7cf3e952e7aeb2:1" },
                {2, "b1ae1287ec4718d9:1" },
                {3, "bff680108adb0fcc:1" },
                {4, "c6805c5e1288b612:1" },
                {5, "b86d3392aea1be30:1" },
                {6, "e6ceba753e1a442:1" },
            };

            // Act
            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void RollingHash_NewLineCombo3()
        {
            // Assume
            string testFileText = " hello; \t\nworld!!!\n\n\n  \t\tGreetings\n End\n";
            var expectedOutput = new Dictionary<int, string>()
            {
                {1, "e9496ae3ebfced30:1" },
                {2, "fb7c023a8b9ccb3f:1" },
                {3, "ce8ba1a563dcdaca:1" },
                {4, "e20e36e16fcb0cc8:1" },
                {5, "b3edc88f2938467e:1" },
                {6, "c8e28b0b4002a3a0:1" },
                {7, "c129715d7a2bc9a3:1" }
            };

            // Act
            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void RollingHash_NewLineCombo4()
        {
            // Assume
            string testFileText = "hello; \t\nworld!!!\r\r\r  \t\tGreetings\r End\r";
            var expectedOutput = new Dictionary<int, string>()
            {
                {1, "e9496ae3ebfced30:1" },
                {2, "fb7c023a8b9ccb3f:1" },
                {3, "ce8ba1a563dcdaca:1" },
                {4, "e20e36e16fcb0cc8:1" },
                {5, "b3edc88f2938467e:1" },
                {6, "c8e28b0b4002a3a0:1" },
                {7, "c129715d7a2bc9a3:1" }
            };

            // Act
            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void RollingHash_NewLineCombo5()
        {
            // Assume
            string testFileText = " hello; \t\r\nworld!!!\r\n\r\n\r\n  \t\tGreetings\r\n End\r\n";
            var expectedOutput = new Dictionary<int, string>()
            {
                {1, "e9496ae3ebfced30:1" },
                {2, "fb7c023a8b9ccb3f:1" },
                {3, "ce8ba1a563dcdaca:1" },
                {4, "e20e36e16fcb0cc8:1" },
                {5, "b3edc88f2938467e:1" },
                {6, "c8e28b0b4002a3a0:1" },
                {7, "c129715d7a2bc9a3:1" }
            };

            // Act
            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void RollingHash_NewLineCombo6()
        {
            // Assume
            string testFileText = " hello; \t\nworld!!!\r\n\n\r  \t\tGreetings\r End\r\n";
            var expectedOutput = new Dictionary<int, string>()
            {
                {1, "e9496ae3ebfced30:1" },
                {2, "fb7c023a8b9ccb3f:1" },
                {3, "ce8ba1a563dcdaca:1" },
                {4, "e20e36e16fcb0cc8:1" },
                {5, "b3edc88f2938467e:1" },
                {6, "c8e28b0b4002a3a0:1" },
                {7, "c129715d7a2bc9a3:1" }
            };

            // Act
            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void RollingHash_NewLineCombo7()
        {
            // Assume
            string test = "Lorem ipsum dolor sit amet.\n";
            string testFileText = "";

            for (int i = 0; i < 10; i++)
            {
                testFileText += test;
            }

            var expectedOutput = new Dictionary<int, string>()
            {
                {1, "a7f2ff13bc495cf2:1" },
                {2, "a7f2ff13bc495cf2:2" },
                {3, "a7f2ff13bc495cf2:3" },
                {4, "a7f2ff13bc495cf2:4" },
                {5, "a7f2ff13bc495cf2:5" },
                {6, "a7f2ff13bc495cf2:6" },
                {7, "a7f2ff1481e87703:1" },
                {8, "a9cf91f7bbf1862b:1" },
                {9, "55ec222b86bcae53:1" },
                {10, "cc97dc7b1d7d8f7b:1" },
                {11, "c129715d7a2bc9a3:1" }
            };

            // Act
            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void RollingHash_NewLineCombo8()
        {
            // Assume
            string testFileText = "x = 2\nx = 1\nprint(x)\nx = 3\nprint(x)\nx = 4\nprint(x)\n";

            var expectedOutput = new Dictionary<int, string>()
            {
                {1, "e54938cc54b302f1:1" },
                {2, "bb609acbe9138d60:1" },
                {3, "1131fd5871777f34:1" },
                {4, "5c482a0f8b35ea28:1" },
                {5, "54517377da7028d2:1" },
                {6, "2c644846cb18d53e:1" },
                {7, "f1b89f20de0d133:1" },
                {8, "c129715d7a2bc9a3:1" }
            };

            // Act
            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void RollingHash_UnicodeSeparatorsAreIgnored()
        {
            // Assume
            string testFileText = "x = 2\u2028x=1\u2029print(x)";

            var expectedOutput = new Dictionary<int, string>()
            {
                {1, "f0a8eee29e998ed7:1" },
            };

            // Act
            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);

            // Assert
            Assert.Equal(expectedOutput, actualOutput);
        }

        // ---------- HashAlgorithms / git-blob-sha-1 ----------

        // The well-known git blob SHA-1 of an empty file.
        // `git hash-object -t blob /dev/null` => e69de29bb2d1d6434b8b29ae775ad8c2e48c5391
        private const string EmptyFileGitBlobSha1 = "e69de29bb2d1d6434b8b29ae775ad8c2e48c5391";

        // SHA-1 of: ASCII "blob 14\0" + UTF-8 "Hello, world!\n" (14 bytes)
        // Verified to match `git hash-object` for the same byte sequence.
        private const string HelloWorldGitBlobSha1 = "af5626b4a114abcb82d63db7c8082c3c4756e51b";

        // SHA-512 of "Hello, world!\n" (14 ASCII bytes), uppercase hex.
        private const string HelloWorldSha512 =
            "09E1E2A84C92B56C8280F4A1203C7CFFD61B162CFE987278D4D6BE9AFBF38C0E" +
            "8934CDADF83751F4E99D111352BFFEFC958E5A4852C8A7A29C95742CE59288A8";

        [Fact]
        public void ComputeHashes_EmptyStream_GitBlobSha1_MatchesGitCanonical()
        {
            using var stream = new MemoryStream(Array.Empty<byte>());
            HashData hashes = HashUtilities.ComputeHashes(stream, HashAlgorithms.GitBlobSha1);

            hashes.GitBlobSha1.Should().Be(EmptyFileGitBlobSha1);
            hashes.Sha1.Should().BeNull();
            hashes.Sha256.Should().BeNull();
            hashes.Sha512.Should().BeNull();
        }

        [Fact]
        public void ComputeHashes_KnownContent_GitBlobSha1_MatchesGitHashObject()
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("Hello, world!\n");
            using var stream = new MemoryStream(bytes);

            HashData hashes = HashUtilities.ComputeHashes(stream, HashAlgorithms.GitBlobSha1);

            hashes.GitBlobSha1.Should().Be(HelloWorldGitBlobSha1);
        }

        [Fact]
        public void ComputeHashes_KnownContent_Sha512_MatchesCanonical()
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes("Hello, world!\n");
            using var stream = new MemoryStream(bytes);

            HashData hashes = HashUtilities.ComputeHashes(stream, HashAlgorithms.Sha512);

            hashes.Sha512.Should().Be(HelloWorldSha512);
            hashes.Sha1.Should().BeNull();
            hashes.Sha256.Should().BeNull();
            hashes.GitBlobSha1.Should().BeNull();
        }

        [Fact]
        public void ComputeHashes_GitBlobSha1_EmittedAsLowercaseHex()
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("Hello, world!\n");
            using var stream = new MemoryStream(bytes);

            HashData hashes = HashUtilities.ComputeHashes(stream, HashAlgorithms.GitBlobSha1);

            // Git canonically emits blob SHAs in lowercase hex; SARIF should match that
            // so values diff cleanly against `git hash-object` output.
            hashes.GitBlobSha1.Should().Be(hashes.GitBlobSha1.ToLowerInvariant());
        }

        [Fact]
        public void ComputeHashes_None_ReturnsAllNull()
        {
            using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            HashData hashes = HashUtilities.ComputeHashes(stream, HashAlgorithms.None);

            hashes.Sha1.Should().BeNull();
            hashes.Sha256.Should().BeNull();
            hashes.Sha512.Should().BeNull();
            hashes.GitBlobSha1.Should().BeNull();
        }

        [Fact]
        public void ComputeHashes_Sha256Only_ProducesOnlySha256()
        {
            using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            HashData hashes = HashUtilities.ComputeHashes(stream, HashAlgorithms.Sha256);

            hashes.Sha1.Should().BeNull();
            hashes.Sha256.Should().NotBeNullOrEmpty();
            hashes.Sha512.Should().BeNull();
            hashes.GitBlobSha1.Should().BeNull();
        }

        [Fact]
        public void ComputeHashes_AllAlgorithms_ProducesAll()
        {
            using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            HashAlgorithms all = HashAlgorithms.Sha1 | HashAlgorithms.Sha256 | HashAlgorithms.Sha512 | HashAlgorithms.GitBlobSha1;

            HashData hashes = HashUtilities.ComputeHashes(stream, all);

            hashes.Sha1.Should().NotBeNullOrEmpty();
            hashes.Sha256.Should().NotBeNullOrEmpty();
            hashes.Sha512.Should().NotBeNullOrEmpty();
            hashes.GitBlobSha1.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ComputeHashes_RestoresStreamPosition_ForSeekableStreams()
        {
            using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            stream.Position = 1;

            HashUtilities.ComputeHashes(stream, HashAlgorithms.Sha256 | HashAlgorithms.GitBlobSha1);

            stream.Position.Should().Be(1);
        }

        [Fact]
        public void ComputeHashes_DefaultParameterlessOverload_ProducesOnlySha256()
        {
            // With the v2 API, every overload defaults to HashAlgorithms.Default (SHA-256
            // only). Callers who want the legacy SHA-1 + SHA-256 combination must opt in
            // explicitly via the algorithms parameter. This test pins down that new
            // default contract.
            using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            HashData hashes = HashUtilities.ComputeHashes(stream);

            hashes.Sha1.Should().BeNull();
            hashes.Sha256.Should().NotBeNullOrEmpty();
            hashes.GitBlobSha1.Should().BeNull();
        }

        [Fact]
        public void ComputeHashes_GitBlobSha1FromFile_MatchesStreamComputation()
        {
            string filePath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(filePath, "abc");

                HashData fromFile = HashUtilities.ComputeHashes(filePath, fileSystem: null, HashAlgorithms.GitBlobSha1);

                using var stream = File.OpenRead(filePath);
                HashData fromStream = HashUtilities.ComputeHashes(stream, HashAlgorithms.GitBlobSha1);

                fromFile.GitBlobSha1.Should().Be(fromStream.GitBlobSha1);
                fromFile.GitBlobSha1.Should().NotBeNullOrEmpty();
            }
            finally
            {
                if (File.Exists(filePath)) { File.Delete(filePath); }
            }
        }

        // ---------- HashData ----------

        [Fact]
        public void HashData_ToDictionary_SkipsNullValues()
        {
            var hashData = new HashData { Sha256 = "ABC" };
            IDictionary<string, string> dict = hashData.ToDictionary();

            dict.Should().ContainKey("sha-256");
            dict.Should().NotContainKey("sha-1");
            dict.Should().NotContainKey("sha-512");
            dict.Should().NotContainKey("git-blob-sha-1");
        }

        [Fact]
        public void HashData_ToDictionary_EmitsGitBlobSha1Key()
        {
            var hashData = new HashData { GitBlobSha1 = "deadbeef" };
            IDictionary<string, string> dict = hashData.ToDictionary();

            dict.Should().ContainKey("git-blob-sha-1");
            dict["git-blob-sha-1"].Should().Be("deadbeef");
        }

        [Fact]
        public void HashData_ToDictionary_EmitsSha512Key()
        {
            var hashData = new HashData { Sha512 = "ABCDEF" };
            IDictionary<string, string> dict = hashData.ToDictionary();

            dict.Should().ContainKey("sha-512");
            dict["sha-512"].Should().Be("ABCDEF");
        }

        [Fact]
        public void ComputeHashes_FromNonZeroStreamPosition_HashesSuffixAsStandaloneBlob()
        {
            // The git-blob-sha-1 contract documented on ComputeHashes(Stream, HashAlgorithms):
            // hashing starts at the stream's current position, and the blob header length
            // reflects the bytes that will actually be hashed (Length - Position). The
            // resulting hash should therefore equal the canonical git blob SHA-1 of the
            // suffix viewed as a standalone blob.
            //
            // Suffix bytes = "Hello, world!\n" (14 bytes) -> known git blob SHA-1.
            const string expectedSuffixGitBlobSha1 = "af5626b4a114abcb82d63db7c8082c3c4756e51b";
            const string prefix = "IGNORED-PREFIX:";
            const string suffix = "Hello, world!\n";

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(prefix + suffix);

            using var stream = new MemoryStream(bytes);
            stream.Position = prefix.Length;

            HashData hashes = HashUtilities.ComputeHashes(stream, HashAlgorithms.GitBlobSha1);

            hashes.GitBlobSha1.Should().Be(expectedSuffixGitBlobSha1);

            // The implementation restores the starting position for seekable streams.
            stream.Position.Should().Be(prefix.Length);
        }

        [Fact]
        public void HashData_ToDictionary_WithAllNullValues_ReturnsEmptyDictionary()
        {
            var hashData = new HashData();
            IDictionary<string, string> dict = hashData.ToDictionary();

            dict.Should().NotBeNull();
            dict.Should().BeEmpty();
        }
    }
}
