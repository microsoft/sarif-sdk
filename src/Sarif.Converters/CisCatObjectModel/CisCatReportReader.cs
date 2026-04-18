// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif.Converters.CisCatObjectModel
{
    public class CisCatReportReader : LogReader<CisCatReport>
    {
        public override CisCatReport ReadLog(Stream input)
        {
            string reportData;

            using (TextReader streamReader = new StreamReader(input))
            {
                reportData = streamReader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<CisCatReport>(reportData);
        }
    }
}
