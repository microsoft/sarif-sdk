// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;
using Newtonsoft.Json;
using FluentAssertions;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class RunTests
    {
        [Fact]
        public void Run_ColumnKindSerializesProperly()
        {
            // In our Windows-specific SDK, if no one has explicitly set ColumnKind, we
            // will set it to the windows-specific value of Utf16CodeUnits. Otherwise,
            // the SARIF file will pick up the ColumnKind default value of 
            // UnicodeCodePoints, which is not appropriate for Windows frameworks.
            RoundTripColumnKind(persistedValue: ColumnKind.None, expectedRoundTrippedValue: ColumnKind.Utf16CodeUnits);

            // When explicitly set, we should always preserve that setting
            RoundTripColumnKind(persistedValue: ColumnKind.Utf16CodeUnits, expectedRoundTrippedValue: ColumnKind.Utf16CodeUnits);
            RoundTripColumnKind(persistedValue: ColumnKind.UnicodeCodePoints, expectedRoundTrippedValue: ColumnKind.UnicodeCodePoints);

        }

        private void RoundTripColumnKind(ColumnKind persistedValue, ColumnKind expectedRoundTrippedValue)
        {
            var sarifLog = new SarifLog
            {
                Runs = new[]
                {
                    new Run
                    {
                        Tool = new Tool { Name = "Test tool"},
                        ColumnKind = persistedValue
                    }
                }
            };

            string json = JsonConvert.SerializeObject(sarifLog);

            // We should never see the default value persisted to JSON
            json.Contains("unicodeCodePoints").Should().BeFalse();

            sarifLog = JsonConvert.DeserializeObject<SarifLog>(json);
            sarifLog.Runs[0].ColumnKind.Should().Be(expectedRoundTrippedValue);
        }
    }
}
