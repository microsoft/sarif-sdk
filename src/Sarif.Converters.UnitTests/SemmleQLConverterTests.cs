// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis.Sarif.Converters;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SemmleQLConverterTests : ConverterTestsBase<SemmleQLConverter>
    {
        [Fact]
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

        [Fact]
        public void SemmleQLConvert_EmbeddedLocations()
        {
            string semmleCsvInput = @"Equals on incomparable types,Finds calls of the form x.Equals(y) with incomparable types for x and y.,warning,""Call to Equals() comparing incomparable types[[""""IComparable"""" | """"file://C:/Windows/Company.NET/Framework/v2.0.50727/mscorlib.dll:0:0:0:0""""]] and [[""""ClientAttributeValue""""|""""relative://ClientClient/Company.ResourceManagement.ObjectModel/ClientAttributeValue.cs:7:152:16""""],[""""ClientAttributeValue""""|""""relative://ClientClient/Company.ResourceManagement.ObjectModel/ClientAttributeValue_ISerializable.cs:14:333:16""""]]"",ProjectOne/Microsoft.ResourceManagement.ObjectModel/ClientResource.cs,SuiteOne/SuiteOne_v1.0-servicing_1.0.1.10511.2/ProjectOneClient/Company.ResourceManagement.ObjectModel/ProjectOneResource.cs,865,15,900,100";

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
          ""message"": ""Call to Equals() comparing incomparable types\""IComparable\"" and \""ClientAttributeValue\"""",
          ""locations"": [
            {
              ""resultFile"": {
                ""uri"": ""ProjectOne/Microsoft.ResourceManagement.ObjectModel/ClientResource.cs"",
                ""uriBaseId"": ""$srcroot"",
                ""region"": {
                  ""startLine"": 865,
                  ""startColumn"": 15,
                  ""endLine"": 900,
                  ""endColumn"": 100
                }
              }
            }
          ],
          ""relatedLocations"": [
            {
              ""physicalLocation"": {
                ""uri"": ""file:///C:/Windows/Company.NET/Framework/v2.0.50727/mscorlib.dll"",
                ""region"": {}
              }
            },
            {
              ""physicalLocation"": {
                ""uri"": ""/ClientClient/Company.ResourceManagement.ObjectModel/ClientAttributeValue.cs"",
                ""uriBaseId"": ""$srcroot"",
                ""region"": {
                  ""startLine"": 7,
                  ""offset"": 152,
                  ""length"": 16
                }
              }
            },
            {
              ""physicalLocation"": {
                ""uri"": ""/ClientClient/Company.ResourceManagement.ObjectModel/ClientAttributeValue_ISerializable.cs"",
                ""uriBaseId"": ""$srcroot"",
                ""region"": {
                  ""startLine"": 14,
                  ""offset"": 333,
                  ""length"": 16
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
