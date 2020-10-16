// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    public class InSourceSuppressionConverterTests : JsonTests
    {
        [Fact]
        public void SuppressionStatus_SuppressedInSource()
        {
            string expected = CreateCurrentV2SarifLogText(
                resultCount: 1,
                (log) =>
                {
                    log.Runs[0].Results[0].Suppressions = new List<Suppression> { new Suppression { Kind = SuppressionKind.InSource } };
                });

            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };

                uut.Initialize(run);

                uut.WriteResults(new[] { new Result
                    {
                        Message = new Message { Text = "Some testing occurred."},
                        Suppressions = new List<Suppression> { new Suppression { Kind = SuppressionKind.InSource } }
                    }
                });
            });
            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);

            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Equal(SuppressionKind.InSource, sarifLog.Runs[0].Results[0].Suppressions[0].Kind);
        }

        [Fact]
        public void BaselineState_None()
        {
            string expected = CreateCurrentV2SarifLogText(resultCount: 1);

            string actual = GetJson(uut =>
            {
                var run = new Run();
                uut.Initialize(run);

                uut.WriteResults(new[] { new Result
                    {
                        Message = new Message { Text = "Some testing occurred."},
                        BaselineState = BaselineState.None
                    }
                });

                // The CloseResults call is not literally required, we provide it
                // for reasons of coverage, to ensure that both the explicit and
                // implicit closing mechanism works.
                uut.CloseResults();

                // Because we did not initialize the run with a Tool object, we
                // need to explicitly emit it via the API.
                uut.WriteTool(DefaultTool);
            });

            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);

            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Null(sarifLog.Runs[0].Results[0].Suppressions);
            Assert.Equal(BaselineState.None, sarifLog.Runs[0].Results[0].BaselineState);
        }

        [Fact]
        public void BaselineState_UnchangedAndUpdated()
        {
            string expected = CreateCurrentV2SarifLogText(
                resultCount: 2,
                (log) =>
                {
                    log.Runs[0].Results[0].BaselineState = BaselineState.Unchanged;
                    log.Runs[0].Results[1].BaselineState = BaselineState.Updated;
                });

            string actual = GetJson(uut =>
            {
                var run = new Run() { Tool = DefaultTool };
                uut.Initialize(run);

                uut.WriteResults(new[] {
                    new Result
                    {
                        Message = new Message { Text = "Some testing occurred."},
                        BaselineState = BaselineState.Unchanged
                    },
                    new Result {
                        Message = new Message { Text = "Some testing occurred."},
                        BaselineState = BaselineState.Updated
                    }
                });
            });
            actual.Should().BeCrossPlatformEquivalent<SarifLog>(expected);

            SarifLog sarifLog = JsonConvert.DeserializeObject<SarifLog>(actual);
            Assert.Null(sarifLog.Runs[0].Results[0].Suppressions);
            Assert.Equal(BaselineState.Unchanged, sarifLog.Runs[0].Results[0].BaselineState);
            Assert.Equal(BaselineState.Updated, sarifLog.Runs[0].Results[1].BaselineState);
        }
    }
}
