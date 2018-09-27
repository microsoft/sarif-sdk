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
        private LineMappingStreamReader _streamReader;

        /// <summary>
        ///  StreamProvider is a function which knows how to re-open the stream containing the JSON
        ///  content to map. It's opened once here for the base JSON.NET parsing, and the
        ///  Deferred collections grab this function so they know how to re-open the Stream when
        ///  you enumerate or index into them.
        /// </summary>
        public Func<Stream> StreamProvider { get; private set; }

        public JsonPositionedTextReader(string filePath) : this(() => File.OpenRead(filePath))
        { }

        /// <summary>
        ///  Create a JsonPositionedTextReader. Takes a function to open the Stream so that deferred
        ///  collections created know how to open separate copies to seek to the collections when
        ///  they are enumerated.
        /// </summary>
        /// <param name="streamProvider"></param>
        public JsonPositionedTextReader(Func<Stream> streamProvider) : this(streamProvider, new LineMappingStreamReader(streamProvider()))
        { }

        internal JsonPositionedTextReader(Func<Stream> streamProvider, LineMappingStreamReader reader) : base(reader)
        {
            this.StreamProvider = streamProvider;
            this._streamReader = reader;
        }

        /// <summary>
        ///  Return the byte offset of the current token in the file.
        /// </summary>
        /// <remarks>
        ///  This must be derived by mapping the (Line, Position) the JsonTextReader returns to an absolute offset.
        ///  The offset isn't exposed, and StreamReader and JsonTextReader both buffer, so StreamReader.BaseStream.Position is not correct.
        ///  </remarks>
        public long TokenPosition => this._streamReader.LineAndCharToOffset(this.LineNumber, this.LinePosition);
    }
}
