// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
                logFilePersistenceOptions: FilePersistenceOptions.None,
                invocationTokensToRedact: null,
                invocationPropertiesToLog: null,
                kinds: BaseLogger.Fail,
                levels: BaseLogger.ErrorWarning);

            string result = textWriter.ToString();

            result.Should().NotBeNull();
        }
    }
}
