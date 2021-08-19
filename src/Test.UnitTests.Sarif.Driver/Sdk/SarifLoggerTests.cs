// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Writers;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Driver
{
    public class SarifLoggerTests
    {
        [Fact]
        public void CanWriteToStream()
        {
            var textWriter = new StringWriter();

            _ = new SarifLogger(
                textWriter,
                analysisTargets: Enumerable.Empty<string>(),
                logFilePersistenceOptions: LogFilePersistenceOptions.None,
                invocationTokensToRedact: null,
                invocationPropertiesToLog: null,
                kinds: new List<ResultKind> { ResultKind.Fail },
                levels: new List<FailureLevel> { FailureLevel.Warning, FailureLevel.Error });

            string result = textWriter.ToString();

            result.Should().NotBeNull();
        }
    }
}
