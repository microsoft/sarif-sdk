// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

using FluentAssertions;

using Newtonsoft.Json.Linq;

namespace Test.UnitTests.Sarif.Mcp.Server.Fixtures
{
    /// <summary>
    /// Minimal JsonPath-style accessor for wire-format invariants typed
    /// equality cannot catch. Examples: <c>$.version</c> must be the string
    /// literal <c>"2.1.0"</c>; <c>$.runs[0].columnKind</c> must serialize as
    /// the lowercase string <c>"utf16CodeUnits"</c> (not an integer); a GUID
    /// must round-trip as a hyphenated 36-char string.
    /// </summary>
    public sealed class RawJson
    {
        private readonly JToken _root;

        private RawJson(JToken root) => this._root = root;

        public static RawJson OfFile(string path) =>
            OfText(File.ReadAllText(path));

        public static RawJson OfText(string json) =>
            new(JToken.Parse(json));

        /// <summary>
        /// Selects a token by JsonPath. Throws via FluentAssertions if not found.
        /// </summary>
        public JToken At(string jsonPath)
        {
            JToken? token = this._root.SelectToken(jsonPath);
            token.Should().NotBeNull($"JsonPath '{jsonPath}' must resolve");
            return token!;
        }

        public string StringAt(string jsonPath)
        {
            JToken token = this.At(jsonPath);
            token.Type.Should().Be(
                JTokenType.String,
                $"JsonPath '{jsonPath}' must be a JSON string (got {token.Type})");
            return token.Value<string>()!;
        }

        public bool HasPath(string jsonPath) =>
            this._root.SelectToken(jsonPath) != null;

        public JTokenType TypeAt(string jsonPath) => this.At(jsonPath).Type;
    }
}
