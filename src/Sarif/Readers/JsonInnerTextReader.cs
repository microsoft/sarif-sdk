// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Readers
{
    /// <summary>
    ///  JsonInnerTextReader is used by deferred collections when deserializing components. 
    ///  It's just a type used as a marker that we're already inside a deferred thing.
    /// </summary>
    internal class JsonInnerTextReader : JsonTextReader
    {
        public JsonInnerTextReader(TextReader reader) : base(reader)
        { }
    }
}
