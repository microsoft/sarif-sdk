// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CodeAnalysis.Sarif.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class SemmleQLConverterTests : ConverterTestsBase<SemmleQLConverter>
    {
        [TestMethod]
        public void SemmleQLConverter_SimpleCsv()
        {
            var semmleCsvInput = SemmleCsvRecord.BuildDefaultRecord().ToCsv();

            string expected = @"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""Semmle QL""
      },
      ""results"": [
        {
          ""level"": ""error"",
          ""message"": ""Message"",
          ""locations"": [
            {
              ""resultFile"": {
                ""uri"": ""RelativePath"",
                ""uriBaseId"": ""$srcroot"",
                ""region"": {
                  ""startLine"": 1,
                  ""startColumn"": 2,
                  ""endLine"": 3,
                  ""endColumn"": 4
                }
              }
            }
          ]
        }
      ]
    }
  ]
}";
            RunTestCase(semmleCsvInput, expected, prettyPrint: true);
        }

        private class SemmleCsvRecord
        {
            public string QueryName;
            public string Description;
            public string Severity;
            public string Message;
            public string RelativePath;
            public string Path;
            public int StartLine;
            public int StartColumn;
            public int EndLine;
            public int EndColumn;

            public static SemmleCsvRecord BuildDefaultRecord()
            {
                return new SemmleCsvRecord()
                {
                    QueryName = QueryNameDefault,
                    Description = DescriptionDefault,
                    Severity = SeverityDefault,
                    Message = MessageDefault,
                    RelativePath = RelativePathDefault,
                    Path = PathDefault,
                    StartLine = StartLineDefault,
                    StartColumn = StartColumnDefault,
                    EndLine = EndLineDefault,
                    EndColumn = EndColumnDefault
                };
            }

            public string ToCsv()
            {
                return
                    "\"" + QueryName              + "\"," +
                    "\"" + Description            + "\"," +
                    "\"" + Severity               + "\"," +
                    "\"" + Message                + "\"," +
                    "\"" + RelativePath           + "\"," +
                    "\"" + Path                   + "\"," +
                    "\"" + StartLine.ToString()   + "\"," +
                    "\"" + StartColumn.ToString() + "\"," +
                    "\"" + EndLine.ToString()     + "\"," +
                    "\"" + EndColumn.ToString()   + "\"";
            }

            private const string QueryNameDefault = nameof(SemmleCsvRecord.QueryName);
            private const string DescriptionDefault = nameof(SemmleCsvRecord.Description);
            private const string SeverityDefault = SemmleQLConverter.SemmleError;
            private const string MessageDefault = nameof(SemmleCsvRecord.Message);
            private const string RelativePathDefault = nameof(SemmleCsvRecord.RelativePath);
            private const string PathDefault = nameof(SemmleCsvRecord.Path);
            private const int StartLineDefault = 1;
            private const int StartColumnDefault = 2;
            private const int EndLineDefault = 3;
            private const int EndColumnDefault = 4;
        }
    }
}
