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
    ///  
    ///  JsonPositionedTextReader requires a *streamProvider* function which can
    ///  open the desired stream multiple times. This is so that deferred collections
    ///  made from the reader pass know how to open another copy of the file to
    ///  read individual elements when the calling code enumerates them later.
    /// </summary>
    public class JsonPositionedTextReader : JsonTextReader
    {
        private readonly LineMappingStreamReader _streamReader;

        private long _cachedPosition;
        private int _cachedPositionLineNumber;
        private int _cachedPositionLinePosition;

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
        ///  Return the byte offset of the last byte of the current token in the file.
        ///  This corresponds to the LineNumber and LinePosition the JsonTextReader returns.
        /// </summary>
        /// <remarks>
        ///  This must be derived by mapping the (Line, Position) the JsonTextReader returns to an absolute offset.
        ///  The offset isn't exposed, and StreamReader and JsonTextReader both buffer, so StreamReader.BaseStream.Position is not correct.
        ///  </remarks>
        public long TokenPosition
        {
            get
            {
                if (this.LinePosition != _cachedPositionLinePosition || this.LineNumber != _cachedPositionLineNumber)
                {
                    _cachedPositionLineNumber = this.LineNumber;
                    _cachedPositionLinePosition = this.LinePosition;
                    _cachedPosition = this._streamReader.LineAndCharToOffset(this.LineNumber, this.LinePosition);
                }

                return _cachedPosition;
            }
        }

        /// <summary>
        ///  Return the character right after this token, if available.
        /// </summary>
        public char? CharAfterToken => this._streamReader.CharAt(this.LineNumber, this.LinePosition + 1);

        /// <summary>
        ///  If this reader is positioned at the start of an array, read to the item at the given index
        ///  and return the position of it.
        /// </summary>
        /// <remarks>
        ///  Used to find an array element when the position of every element was not saved, by walking
        ///  from the nearest previous element.
        /// </remarks>
        /// <param name="desiredIndex">Index of array element to find</param>
        /// <returns>Position of the beginning of the element</returns>
        public long ReadToArrayIndex(int desiredIndex)
        {
            // Read StartArray
            this.Read();
            if (this.TokenType != JsonToken.StartArray) { throw new ArgumentException($"ReadToArrayIndex must be given an array to search"); }

            // If first item desired, it's at ArrayStart + 1
            if (desiredIndex == 0) { return this.TokenPosition + 1; }

            // Read up to the end of the element before the desired one
            int index;
            for (index = 0; index < desiredIndex; ++index)
            {
                this.Read();
                if (this.TokenType == JsonToken.EndArray) { break; }

                this.Skip();
            }

            // Verify the array was long enough
            if (index < desiredIndex - 1) { throw new ArgumentOutOfRangeException($"ReadToArrayIndex requested for index {desiredIndex}, but only {index} elements found."); }

            // Return two after the last item end (past the last character and the comma)
            return this.TokenPosition + 2;
        }
    }
}
