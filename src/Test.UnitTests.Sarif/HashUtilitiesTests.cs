// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Collections.Generic;
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
                foreach(string filePath in filePaths)
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
    }
}
