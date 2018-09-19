// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.IO;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    /// <summary>
    ///  JsonPositionedTextReader is a JsonTextReader which exposes TokenPosition(),
    ///  the byte offset in the stream where the current token begins. It is used to
    ///  enable deferred collections, which look like other collections but are deserialized
    ///  only when enumerated, saving memory.
    /// </summary>
    public class JsonPositionedTextReader : JsonTextReader
    {
        private LineMappingStreamReader StreamReader { get; set; }
        private int LastReadLineNumber { get; set; }
        private int LastReadLinePosition { get; set; }

        public Func<Stream> StreamProvider { get; private set; }

        public JsonPositionedTextReader(string filePath) : this(() => File.OpenRead(filePath))
        { }

        public JsonPositionedTextReader(Func<Stream> streamProvider) : this(streamProvider, new LineMappingStreamReader(streamProvider()))
        { }

        internal JsonPositionedTextReader(Func<Stream> streamProvider, LineMappingStreamReader reader) : base(reader)
        {
            this.StreamProvider = streamProvider;
            this.StreamReader = reader;
        }

        /// <summary>
        ///  Return the byte offset of the current token in the file.
        /// </summary>
        /// <remarks>
        ///  This must be derived by mapping the (Line, Position) the JsonTextReader returns to an absolute offset.
        ///  The offset isn't exposed, and StreamReader and JsonTextReader both buffer, so StreamReader.BaseStream.Position is not correct.
        ///  </remarks>
        public long TokenPosition => this.StreamReader.LineAndCharToOffset(this.LineNumber, this.LinePosition);
    }
}
