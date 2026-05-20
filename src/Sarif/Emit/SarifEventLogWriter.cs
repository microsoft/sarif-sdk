// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif.Emit
{
    /// <summary>
    /// Append-only writer for a SARIF event log (<c>*.sarif.wip.jsonl</c>).
    /// </summary>
    /// <remarks>
    /// <para>The writer opens the target file with <see cref="FileShare.Read"/> sharing so
    /// concurrent appends from other processes are rejected with an <see cref="IOException"/>.</para>
    /// <para>If the file exists and does not end with a newline, the prior writer was interrupted
    /// mid-line; the writer rejects the file with a <see cref="SarifEventLogException"/> rather
    /// than risk concatenating bytes to a torn line.</para>
    /// <para>Every event is serialized to a single UTF-8 line terminated with <c>\n</c>. The line
    /// is flushed to disk before the writer call returns to minimize the torn-line window on crash.</para>
    /// </remarks>
    public sealed class SarifEventLogWriter : IDisposable
    {
        private static readonly Encoding s_utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        private readonly FileStream _stream;
        private readonly JsonSerializer _serializer;

        public SarifEventLogWriter(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Event log path must be supplied.", nameof(path));
            }

            EnsureNoTornTrailingLine(path);

            _stream = new FileStream(
                path,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read);

            _serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = Newtonsoft.Json.Formatting.None,
            });
        }

        /// <summary>Appends an event with the given kind and payload.</summary>
        public void Append(string kind, JToken payload)
        {
            if (string.IsNullOrEmpty(kind))
            {
                throw new ArgumentException("Event kind must be supplied.", nameof(kind));
            }

            var sarifEvent = new SarifEvent
            {
                Version = SarifEventKinds.CurrentSchemaVersion,
                Kind = kind,
                Payload = payload ?? new JObject(),
            };

            // Serialize first, then write atomically as one buffer + newline. Avoids partial
            // line state if the serializer throws.
            var sb = new StringBuilder(256);
            using (var sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            using (var jw = new JsonTextWriter(sw) { Formatting = Newtonsoft.Json.Formatting.None })
            {
                _serializer.Serialize(jw, sarifEvent);
            }

            sb.Append('\n');
            byte[] bytes = s_utf8NoBom.GetBytes(sb.ToString());
            _stream.Write(bytes, 0, bytes.Length);
            _stream.Flush();
        }

        /// <summary>Appends an event whose payload is a strongly-typed SARIF object.</summary>
        public void Append(string kind, object payload)
        {
            JToken token;
            if (payload == null)
            {
                token = new JObject();
            }
            else if (payload is JToken existing)
            {
                token = existing;
            }
            else
            {
                // Route through text-based serialization so converters that call
                // JsonWriter.WriteRawValue (e.g. the SDK's EnumConverter) behave
                // correctly. Writing raw into a JTokenWriter would embed literal
                // quote characters inside JValue strings and break round-trip.
                string json;
                var sb = new StringBuilder(256);
                using (var sw = new StringWriter(sb, CultureInfo.InvariantCulture))
                using (var jw = new JsonTextWriter(sw) { Formatting = Newtonsoft.Json.Formatting.None })
                {
                    _serializer.Serialize(jw, payload);
                    json = sb.ToString();
                }

                token = JToken.Parse(json);
            }

            Append(kind, token);
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }

        /// <summary>
        /// If the file exists and is non-empty, verify its last byte is <c>\n</c>; otherwise the
        /// prior writer was interrupted mid-line and the file is in an unrecoverable state for
        /// safe append.
        /// </summary>
        private static void EnsureNoTornTrailingLine(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            using (var probe = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (probe.Length == 0)
                {
                    return;
                }

                probe.Seek(-1, SeekOrigin.End);
                int last = probe.ReadByte();
                if (last != '\n')
                {
                    throw new SarifEventLogException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Event log '{0}' does not end with a newline (torn line). The prior writer was interrupted mid-line; refusing to append. Inspect or discard the file before continuing.",
                            path));
                }
            }
        }
    }
}
