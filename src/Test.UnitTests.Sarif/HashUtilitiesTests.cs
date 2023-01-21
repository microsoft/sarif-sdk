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

            //  Mimic the --quiet flag.  We should not use the console this time.
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

        [Fact]
        public void RollingHash_NewLineCombo1()
        {
            string testFileText = " a\nb\n  \t\tc\n d";
            Dictionary<int, string> expectedOutput = new Dictionary<int, string>();
            expectedOutput.Add(1, "271789c17abda88f:1");
            expectedOutput.Add(2, "54703d4cd895b18:1");
            expectedOutput.Add(3, "180aee12dab6264:1");
            expectedOutput.Add(4, "a23a3dc5e078b07b:1");

            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);
            Assert.Equal(expectedOutput, actualOutput);
        }

        [Fact]
        public void RollingHash_NewLineCombo2()
        {
            string testFileText = " hello; \t\nworld!!!\n\n\n  \t\tGreetings\n End";
            Dictionary<int, string> expectedOutput = new Dictionary<int, string>();
            expectedOutput.Add(1, "8b7cf3e952e7aeb2:1");
            expectedOutput.Add(2, "b1ae1287ec4718d9:1");
            expectedOutput.Add(3, "bff680108adb0fcc:1");
            expectedOutput.Add(4, "c6805c5e1288b612:1");
            expectedOutput.Add(5, "b86d3392aea1be30:1");
            expectedOutput.Add(6, "e6ceba753e1a442:1");

            Dictionary<int, string> actualOutput = HashUtilities.RollingHash(testFileText);
            Assert.Equal(expectedOutput, actualOutput);
        }
    }
}
