// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class TSLintLoader : ITSLintLoader
    {
        private readonly XmlObjectSerializer Serializer;

        public TSLintLoader()
        {
            Serializer = new DataContractJsonSerializer(typeof(TSLintLog));
        }

        /// <summary>
        /// A constructor used for test purposes (to allow mocking the serializer)
        /// </summary>
        /// <param name="serializer"></param>
        internal TSLintLoader(XmlObjectSerializer serializer)
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

            // NOTE: The outer capturing parentheses in this regex are required.
            // They take advantage of a useful (but not easily discoverable)
            // feature of Regex.Split:
            //
            //     If capturing parentheses are used in a Regex.Split expression,
            //     any captured text is included in the resulting string array.
            //     For example, if you split the string "plum-pear" on a hyphen
            //     placed within capturing parentheses, the returned array includes
            //     a string element that contains the hyphen.
            //
            // See https://msdn.microsoft.com/en-us/library/8yttk7sy(v=vs.110).aspx
            Regex regex = new Regex("(\"fix\":\\s*{[^}]+},)");

            string[] tokens = regex.Split(contents);

            for (int i = 0; i < tokens.Length; i++) 
            {
                if (Regex.Replace(tokens[i], @"\s+", "").Contains("\"fix\":{"))
                {
                    tokens[i] = tokens[i].Replace("{", "[{").Replace("},", "}],");
                }
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(string.Concat(tokens));
            return new MemoryStream(byteArray);
        }

    }
}
