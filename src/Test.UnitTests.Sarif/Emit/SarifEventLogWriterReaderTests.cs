// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif.Emit;

using Newtonsoft.Json.Linq;

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Test.UnitTests.Emit
{
    public class SarifEventLogWriterReaderTests : IDisposable
    {
        private readonly string _path;

        public SarifEventLogWriterReaderTests()
        {
            _path = Path.Combine(Path.GetTempPath(), $"sarif-eventlog-{Guid.NewGuid():N}.jsonl");
        }

        public void Dispose()
        {
            if (File.Exists(_path)) { File.Delete(_path); }
        }

        [Fact]
        public void RoundTrip_WriterPlusReader_PreservesAllEventsInOrder()
        {
            using (var writer = new SarifEventLogWriter(_path))
            {
                writer.Append(SarifEventKinds.RunHeader, new Run { Tool = new Tool { Driver = new ToolComponent { Name = "demo" } } });
                writer.Append(SarifEventKinds.Result, new Result { RuleId = "CWE-79", Message = new Message { Text = "xss" } });
                writer.Append(SarifEventKinds.Invocation, new Invocation { ExecutionSuccessful = true });
                writer.Append(SarifEventKinds.NotificationDescriptor, new ReportingDescriptor { Id = "progress" });
            }

            var events = new SarifEventLogReader().Read(_path).ToList();

            events.Select(e => e.Kind).Should().Equal(
                SarifEventKinds.RunHeader,
                SarifEventKinds.Result,
                SarifEventKinds.Invocation,
                SarifEventKinds.NotificationDescriptor);
            events.Should().OnlyContain(e => e.Version == SarifEventKinds.CurrentSchemaVersion);
            events[1].Payload["ruleId"].Value<string>().Should().Be("CWE-79");
        }

        [Fact]
        public void Writer_AlwaysTerminatesEachLineWithLineFeed()
        {
            using (var writer = new SarifEventLogWriter(_path))
            {
                writer.Append(SarifEventKinds.Result, new Result { RuleId = "X" });
            }

            byte[] bytes = File.ReadAllBytes(_path);
            bytes[bytes.Length - 1].Should().Be((byte)'\n');
        }

        [Fact]
        public void Writer_DoesNotEmitUtf8Bom()
        {
            using (var writer = new SarifEventLogWriter(_path))
            {
                writer.Append(SarifEventKinds.Result, new Result { RuleId = "X" });
            }

            byte[] bytes = File.ReadAllBytes(_path);
            (bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF).Should().BeFalse();
        }

        [Fact]
        public void Writer_RefusesToAppendToFileMissingTrailingNewline()
        {
            File.WriteAllBytes(_path, Encoding.UTF8.GetBytes("{\"v\":1,\"kind\":\"result\",\"payload\":{}}"));

            Action act = () =>
            {
                using var w = new SarifEventLogWriter(_path);
            };

            act.Should().Throw<SarifEventLogException>().WithMessage("*torn line*");
        }

        [Fact]
        public void Reader_ToleratesCrLfLineEndings()
        {
            File.WriteAllText(
                _path,
                "{\"v\":1,\"kind\":\"result\",\"payload\":{\"ruleId\":\"A\"}}\r\n" +
                "{\"v\":1,\"kind\":\"result\",\"payload\":{\"ruleId\":\"B\"}}\r\n",
                Encoding.UTF8);

            var events = new SarifEventLogReader().Read(_path).ToList();
            events.Select(e => e.Payload.Value<string>("ruleId")).Should().Equal("A", "B");
        }

        [Fact]
        public void Reader_SkipsBlankLines()
        {
            File.WriteAllText(
                _path,
                "\n" +
                "{\"v\":1,\"kind\":\"result\",\"payload\":{}}\n" +
                "\n" +
                "{\"v\":1,\"kind\":\"invocation\",\"payload\":{}}\n",
                Encoding.UTF8);

            var events = new SarifEventLogReader().Read(_path).ToList();
            events.Select(e => e.Kind).Should().Equal(SarifEventKinds.Result, SarifEventKinds.Invocation);
        }

        [Fact]
        public void Reader_SkipsUnknownKindAtKnownSchemaVersion()
        {
            File.WriteAllText(
                _path,
                "{\"v\":1,\"kind\":\"future-extension\",\"payload\":{}}\n" +
                "{\"v\":1,\"kind\":\"result\",\"payload\":{}}\n",
                Encoding.UTF8);

            var events = new SarifEventLogReader().Read(_path).ToList();
            events.Should().HaveCount(1);
            events[0].Kind.Should().Be(SarifEventKinds.Result);
        }

        [Fact]
        public void Reader_ThrowsOnUnknownVersionForKnownKind()
        {
            File.WriteAllText(
                _path,
                "{\"v\":99,\"kind\":\"result\",\"payload\":{}}\n",
                Encoding.UTF8);

            Action act = () => new SarifEventLogReader().Read(_path).ToList();
            act.Should().Throw<SarifEventLogException>().WithMessage("*schema version 99*");
        }

        [Fact]
        public void Reader_ThrowsOnMalformedJsonWithLineNumber()
        {
            File.WriteAllText(
                _path,
                "{\"v\":1,\"kind\":\"result\",\"payload\":{}}\n" +
                "this is not json\n" +
                "{\"v\":1,\"kind\":\"result\",\"payload\":{}}\n",
                Encoding.UTF8);

            Action act = () => new SarifEventLogReader().Read(_path).ToList();
            act.Should().Throw<SarifEventLogException>().WithMessage("*line 2*");
        }

        [Fact]
        public void Writer_RejectsConcurrentWriterByExclusiveLock()
        {
            // The exclusive-lock contract relies on Windows' mandatory file sharing
            // semantics; on POSIX, .NET only honors FileShare.None as a best-effort
            // advisory lock (FileShare.Read does not block a second writer). The
            // emit chain's canonical use is single-process JSONL append, so this
            // guarantee is documented and tested as Windows-only.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            using var first = new SarifEventLogWriter(_path);
            first.Append(SarifEventKinds.Result, new Result { RuleId = "A" });

            Action act = () =>
            {
                using var second = new SarifEventLogWriter(_path);
            };

            act.Should().Throw<IOException>();
        }

        [Fact]
        public void WireShape_OrdersFieldsAsVKindPayload()
        {
            using (var writer = new SarifEventLogWriter(_path))
            {
                writer.Append(SarifEventKinds.Result, new JObject { ["x"] = 1 });
            }

            string line = File.ReadAllText(_path).TrimEnd('\n');
            line.Should().StartWith("{\"v\":1,\"kind\":\"result\",\"payload\":");
        }
    }
}
