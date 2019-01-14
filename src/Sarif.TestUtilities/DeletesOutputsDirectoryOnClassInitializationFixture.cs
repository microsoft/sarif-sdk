// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.TestUtilities
{
    /// <summary>
    /// This class is invoked a single time on initializing an xunit class for testing. It deletes
    /// any existing test output files that may have been produced by the previous run. This fixture
    /// is required because individual tests in test classes result in an object instantiation for
    /// each test case. The FileDiffingTests pattern, by design outputs a bundle of outputs to 
    /// a common directory, to allow diffing an entire directory of failing tests. If each test
    /// case deleted this directory, the results would be that only the last test failure would
    /// exist in the common outputs location
    /// </summary>
    public abstract class DeletesOutputsDirectoryOnClassInitializationFixture
    {
        protected virtual string TypeUnderTest => this.GetType().Name.Substring(0, this.GetType().Name.Length - "TestsFixture".Length);

        protected virtual string OutputFolderPath => Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "UnitTestOutput." + TypeUnderTest);

        public DeletesOutputsDirectoryOnClassInitializationFixture()
        {
            if (Directory.Exists(OutputFolderPath))
            {
                Directory.Delete(OutputFolderPath, recursive: true);
            }
        }
    }
}
