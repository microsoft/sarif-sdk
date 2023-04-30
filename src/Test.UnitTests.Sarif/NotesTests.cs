// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Driver;
using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class NotesTests
    {
        [Fact]
        public void NotesTests_LogEmptyFileSkipped()
        {
            using var logger =
                new MemoryStreamSarifLogger(levels: BaseLogger.ErrorWarningNote,
                                            kinds: BaseLogger.Fail);

            var oneCharacterFile = new EnumeratedArtifact(FileSystem.Instance)
            {
                Uri = new Uri(@"c:\\nonemptyfile.txt"),
            };

            var emptyFile = new EnumeratedArtifact(FileSystem.Instance)
            {
                Uri = new Uri(@"c:\\emptyfile.txt"),
                SizeInBytes = 0,
            };

            var context = new TestAnalysisContext()
            {
                Logger = logger,
                TargetsProvider = new ArtifactProvider(FileSystem.Instance)
                {
                    Artifacts = new EnumeratedArtifact[] { oneCharacterFile },
                    Skipped = new[] { emptyFile },
                },
                FailureLevels = BaseLogger.ErrorWarningNote,
                ResultKinds = BaseLogger.Fail,
            };

            var command = new TestMultithreadedAnalyzeCommand();
            int result = command.Run(options: null, ref context);
            context.ValidateCommandExecution(result);

            var log = logger.ToSarifLog();
            log.Runs.Should().HaveCount(1);

            Run run = log.Runs[0];
            run.Results.Should().HaveCount(0);

            run.Invocations.Should().HaveCount(1);
            Invocation invocation = run.Invocations[0];
            invocation.ToolExecutionNotifications.Should().BeNull();

            // Finally, what we're here for, verify that we received a
            // notification warning us about the skipped zero-byte file.
            invocation.ToolConfigurationNotifications.Should().HaveCount(1);
            Notification notification = invocation.ToolConfigurationNotifications[0];
            notification.Descriptor.Id.Should().Be(Notes.Msg002_EmptyFileSkipped);
        }
    }
}
