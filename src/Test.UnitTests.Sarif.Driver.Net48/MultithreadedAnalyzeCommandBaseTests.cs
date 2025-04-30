// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Test.UnitTests.Sarif.Driver.Net48
{
    [TestClass]
    public sealed class MultithreadedAnalyzeCommandBaseTests
    {
        [TestMethod]
        public void AnalyzeCommand_IllegalPathCharInURL()
        {
            var sarifOutput = new StringBuilder();
            var command = new AnalyzeTestCommand();
            using var writer = new StringWriter(sarifOutput);
            var logger = new SarifLogger(writer,
                                            run: new Run { Tool = command.Tool },
                                            levels: BaseLogger.ErrorWarningNote,
                                            kinds: BaseLogger.Fail);

            var target = new EnumeratedArtifact(FileSystem.Instance) { Uri = new Uri("http://example.com/some<character>test/bad\"characters\"path.txt"), Contents = "fake content" };

            var context = new AnalyzeTestContext
            {
                TargetsProvider = new ArtifactProvider(new[] { target }),
                FailureLevels = BaseLogger.ErrorWarningNote,
                ResultKinds = BaseLogger.Fail,
                Logger = logger,
            };

            int result = new AnalyzeTestCommand().Run(options: new AnalyzeTestOptions(), ref context);
            Assert.AreEqual(0, result);
        }
    }
}
