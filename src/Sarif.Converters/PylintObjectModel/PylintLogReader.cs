// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.PylintObjectModel
{
    public class PylintLogReader : LogReader<PylintLog>
    {
        public override PylintLog ReadLog(Stream input)
        {
            string pylintText;

            using (TextReader streamReader = new StreamReader(input))
            {
                pylintText = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<PylintLog>(pylintText);
        }
    }
}