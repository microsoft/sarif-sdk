// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.CodeAnalysis.Sarif
{
    public abstract class FileDiffingTests
    {
        protected string GenerateDiffCommand(string expected, string actual)
        {
            expected = Path.GetFullPath(expected).Replace(@"\", @"\\");
            actual = Path.GetFullPath(actual).Replace(@"\", @"\\");

            string beyondCompare = TryFindBeyondCompare();
            if (beyondCompare != null)
            {
                return String.Format(CultureInfo.InvariantCulture, "\"{0}\" \"{1}\" \"{2}\" /title1=Expected /title2=Actual", beyondCompare, expected, actual);
            }

            return String.Format(CultureInfo.InvariantCulture, "windiff \"{0}\" \"{1}\"", expected, actual);
        }

        protected static bool AreEquivalentSarifLogs(string actualSarif, string expectedSarif)
        {
            expectedSarif = expectedSarif ?? "{}";

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = SarifContractResolver.Instance,
                Formatting = Formatting.Indented
            };

            // Make sure we can successfully deserialize what was just generated
            SarifLog actualLog = JsonConvert.DeserializeObject<SarifLog>(actualSarif, settings);

            actualSarif = JsonConvert.SerializeObject(actualLog, settings);

            JToken generatedToken = JToken.Parse(actualSarif);
            JToken expectedToken = JToken.Parse(expectedSarif);

            return JToken.DeepEquals(generatedToken, expectedToken);
        }

        protected static string TryFindBeyondCompare()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            for (int idx = 4; idx >= 3; --idx)
            {
                string beyondComparePath = String.Format(CultureInfo.InvariantCulture, "{0}\\Beyond Compare {1}\\BComp.exe", programFiles, idx);
                if (File.Exists(beyondComparePath))
                {
                    return beyondComparePath;
                }
            }

            programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            for (int idx = 4; idx >= 3; --idx)
            {
                string beyondComparePath = String.Format(CultureInfo.InvariantCulture, "{0}\\Beyond Compare {1}\\BComp.exe", programFiles, idx);
                if (File.Exists(beyondComparePath))
                {
                    return beyondComparePath;
                }
            }

            string beyondCompare2Path = programFiles + "\\Beyond Compare 2\\BC2.exe";
            if (File.Exists(beyondCompare2Path))
            {
                return beyondCompare2Path;
            }

            return null;
        }
    }
}
