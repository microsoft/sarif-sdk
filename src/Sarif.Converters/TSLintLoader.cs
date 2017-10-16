// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

[assembly: InternalsVisibleTo("Sarif.Driver.UnitTests.dll,PublicKey=0024000004800000940000000602000000240000525341310004000001000100433fbf156abe971" +
    "8142bdbd48a440e779a1b708fd21486ee0ae536f4c548edf8a7185c1e3ac89ceef76c15b8cc2497906798779a59402f9b9e27281fb15e7111566cdc9a9f8326301d45320623c52" +
    "22089cf4d0013f365ae729fb0a9c9d15138042825cd511a0f3d4887a7b92f4c2749f81b410813d297b73244cf64995effb1")]
namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintLoader : ITSLintLoader
    {
        private readonly XmlObjectSerializer Serializer;

        public TSLintLoader()
        {
            Serializer = new DataContractJsonSerializer(typeof(TSLintLog));
        }

        public TSLintLoader(XmlObjectSerializer serializer)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }
        
        public TSLintLog ReadLog(Stream input)
        {
            input = input ?? throw new ArgumentNullException(nameof(input));

            return (TSLintLog)Serializer.ReadObject(NormalizeTSLintFixFormat(input));
        }

        /// <summary>
        /// Necessary because the TSLint json contains multiple patterns for fix, i.e.
        /// "fix":{"innerStart":4429,"innerLength":0,"innerText":"\r\n"},
        /// "fix":[{"innerStart":4429,"innerLength":0,"innerText":"\r\n"}],
        /// and "fix":[{"innerStart":4429,"innerLength":0,"innerText":"\r\n"},{"innerStart":4429,"innerLength":0,"innerText":"\r\n"}],
        /// 
        /// We need to normalize it so that every instance of fix is a list.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal Stream NormalizeTSLintFixFormat(Stream stream)
        {
            stream = stream ?? throw new ArgumentNullException(nameof(stream));

            string contents;
            using (StreamReader reader = new StreamReader(stream))
            {
                contents = reader.ReadToEnd();
            }

            Regex regex = new Regex("(\"fix\":{[^}]+},)");
            
            string[] tokens = regex.Split(contents);

            for (int i = 0; i < tokens.Length; i++) 
            {
                if (tokens[i].Contains("\"fix\":{"))
                {
                    tokens[i] = tokens[i].Replace("\"fix\":{", "\"fix\":[{").Replace("},", "}],");
                }
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(string.Concat(tokens));
            return new MemoryStream(byteArray);
        }

    }
}
