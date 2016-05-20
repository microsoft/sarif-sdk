// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.CodeAnalysis.Sarif.Writers;
using Newtonsoft.Json;

namespace Microsoft.CodeAnalysis.Sarif
{
    public abstract class JsonTests
    {
        private const SarifVersion CurrentVersion = SarifVersion.OneZeroZero;

        protected static readonly string SarifSchemaUri =
            SarifUtilities.ConvertToSchemaUri(CurrentVersion).OriginalString;

        protected static readonly string SarifFormatVersion =
            SarifUtilities.ConvertToText(CurrentVersion);

        protected static readonly Run DefaultRun = new Run();
        protected static readonly Tool DefaultTool = new Tool();
        protected static readonly Result DefaultResult = new Result();

        protected JsonTests()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = SarifContractResolver.Instance
            };
        }

        protected static string GetJson(Action<ResultLogJsonWriter> testContent)
        {
            StringBuilder result = new StringBuilder();
            using (var str = new StringWriter(result))
            using (var json = new JsonTextWriter(str) { Formatting = Formatting.Indented })
            using (var uut = new ResultLogJsonWriter(json))
            {
                testContent(uut);
            }

            return result.ToString();
        }
    }
}