// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;

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

            List<string> filePaths = new List<string>(numberOfTestFiles);

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
            TestTextWriter testTextWriter = new TestTextWriter();
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
            string testFileText = "";
            Dictionary<int, string> expectedOutput = new Dictionary<int, string>();
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
            Dictionary<int, string> expectedOutput = new Dictionary<int, string>()
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
            Dictionary<int, string> expectedOutput = new Dictionary<int, string>()
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
            Dictionary<int, string> expectedOutput = new Dictionary<int, string>()
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
            Dictionary<int, string> expectedOutput = new Dictionary<int, string>()
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
            Dictionary<int, string> expectedOutput = new Dictionary<int, string>()
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
            Dictionary<int, string> expectedOutput = new Dictionary<int, string>()
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

            Dictionary<int, string> expectedOutput = new Dictionary<int, string>()
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

            Dictionary<int, string> expectedOutput = new Dictionary<int, string>()
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
    }
}
