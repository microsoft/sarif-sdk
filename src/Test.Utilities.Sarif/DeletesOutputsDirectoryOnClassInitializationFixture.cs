// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System.IO;

namespace Microsoft.CodeAnalysis.Sarif
{
    /// <summary>
    /// This class is an XUnit "class fixture." If a test class is marked with the interface
    /// IClassFixture&lt;T>, then before XUnit runs any tests from the class, it will instantiate
    /// T (which must have a parameterless constructor). If T implements IDisposable, then after
    /// xUnit runs the last test method from the class, it will dispose the fixture. This mechanism
    /// allows class-level setup and teardown (although I don't know why they don't just reflect
    /// for static methods like ClassSetup and ClassTeardown).
    ///
    /// See https://xunit.github.io/docs/shared-context for more information about xUnit class fixtures.
    ///
    /// This particular fixture deletes any existing test output files that may have been produced
    /// by a previous run. It is designed for use on test classes that derive from FileDiffingUnitTests.
    /// It is required because FileDiffingUnitTests emits the outputs from each test to a common directory,
    /// to allow diffing an entire directory of failing tests. If each test case deleted this directory,
    /// then at the end it would contain only the output from the last failing test.
    ///
    /// Each class that derives from FileDiffingUnitTests can declare its own derived fixture class if
    /// it wants to override the virtual TypeUnderTest or OutputFolderPath properties, but there seems
    /// no good reason to do this.
    /// </summary>
    public class DeletesOutputsDirectoryOnClassInitializationFixture
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
