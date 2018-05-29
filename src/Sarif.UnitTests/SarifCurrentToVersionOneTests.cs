// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.VersionOne;
using Microsoft.CodeAnalysis.Sarif.Visitors;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class SarifCurrentToVersionOneTests
    {
        private static SarifLog GetSarifLog(string logText)
        {
            return JsonConvert.DeserializeObject<SarifLog>(logText, SarifTransformerUtilities.JsonSettingsV2);
        }

        private static SarifLogVersionOne TransformCurrentToVersionOne(string v2LogText)
        {
            SarifLog v2Log = GetSarifLog(v2LogText);
            var transformer = new SarifCurrentToVersionOneVisitor();
            transformer.VisitSarifLog(v2Log);

            return transformer.SarifLogVersionOne;
        }

        private static void VerifyCurrentToVersionOneTransformation(string v2LogText, string v1LogExpectedText)
        {
            SarifLogVersionOne v1Log = TransformCurrentToVersionOne(v2LogText);
            string v1LogText = JsonConvert.SerializeObject(v1Log, SarifTransformerUtilities.JsonSettingsV1);
            v1LogText.Should().Be(v1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_Minimum()
        {
            const string V2LogText =
@"{
    ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
    ""version"": ""2.0.0"",
    ""runs"": [
      {
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0""
        },
        ""results"": []
      }
    ]
  }";

            const string V1LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""results\"": []\r\n}""
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_MinimumWithTwoRuns()
        {
            const string V2LogText =
@"{
    ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
    ""version"": ""2.0.0"",
    ""runs"": [
      {
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0""
        },
        ""results"": []
      },
      {
        ""tool"": {
          ""name"": ""AssetScanner"",
          ""semanticVersion"": ""1.7.2""
        },
        ""results"": []
      }
    ]
  }";

            const string V1LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""results\"": []\r\n}""
      }
    },
    {
      ""tool"": {
        ""name"": ""AssetScanner"",
        ""semanticVersion"": ""1.7.2""
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""AssetScanner\"",\r\n    \""semanticVersion\"": \""1.7.2\""\r\n  },\r\n  \""results\"": []\r\n}""
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithLogicalLocations()
        {
            const string V2LogText =
@"{
    ""version"": ""2.0.0"",
    ""runs"": [
      {
        ""tool"": {
          ""name"": ""CodeScanner"",
          ""semanticVersion"": ""2.1.0""
        },
        ""logicalLocations"": {
          ""collections::list::add"": {
            ""name"": ""add"",
            ""kind"": ""function"",
            ""parentKey"": ""collections::list""
          },
          ""collections::list"": {
            ""name"": ""list"",
            ""kind"": ""type"",
            ""parentKey"": ""collections""
          },
          ""collections"": {
            ""name"": ""collections"",
            ""kind"": ""namespace""
          }
        },
        ""results"": [
        ]
      }
    ]
  }";

            const string V1LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""logicalLocations"": {
        ""collections::list::add"": {
          ""name"": ""add"",
          ""parentKey"": ""collections::list"",
          ""kind"": ""function""
        },
        ""collections::list"": {
          ""name"": ""list"",
          ""parentKey"": ""collections"",
          ""kind"": ""type""
        },
        ""collections"": {
          ""name"": ""collections"",
          ""kind"": ""namespace""
        }
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""logicalLocations\"": {\r\n    \""collections::list::add\"": {\r\n      \""name\"": \""add\"",\r\n      \""parentKey\"": \""collections::list\"",\r\n      \""kind\"": \""function\""\r\n    },\r\n    \""collections::list\"": {\r\n      \""name\"": \""list\"",\r\n      \""parentKey\"": \""collections\"",\r\n      \""kind\"": \""type\""\r\n    },\r\n    \""collections\"": {\r\n      \""name\"": \""collections\"",\r\n      \""kind\"": \""namespace\""\r\n    }\r\n  },\r\n  \""results\"": []\r\n}""
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithFiles()
        {
            const string V2LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""files"": {
        ""file:///home/list.txt"": {
          ""length"": 43,
          ""mimeType"": ""text/plain"",
          ""contents"": {
            ""text"": ""The quick brown fox jumps over the lazy dog"",
            ""binary"": ""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw==""
          },
          ""hashes"": [
            {
              ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",
              ""algorithm"": ""sha-256""
            }
          ]
        },
        ""file:///home/buildAgent/bin/app.zip"": {
          ""mimeType"": ""application/zip"",
          ""properties"": {
            ""my_key"": ""some value""
          }
        },
        ""file:///home/buildAgent/bin/app.zip#/docs/intro.docx"": {
          ""fileLocation"": {
            ""uri"": ""file:///docs/intro.docx""
          },
          ""parentKey"": ""file:///home/buildAgent/bin/app.zip"",
          ""offset"": 17522,
          ""length"": 4050,
          ""mimeType"": ""application/vnd.openxmlformats-officedocument.wordprocessingml.document"",
          ""contents"": {}
        }
      },
      ""results"": []
    }
  ]
}";
            const string V1LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""files"": {
        ""file:///home/list.txt"": {
          ""length"": 43,
          ""mimeType"": ""text/plain"",
          ""contents"": ""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw=="",
          ""hashes"": [
            {
              ""value"": ""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592"",
              ""algorithm"": ""sha256""
            }
          ]
        },
        ""file:///home/buildAgent/bin/app.zip"": {
          ""mimeType"": ""application/zip"",
          ""properties"": {
            ""my_key"": ""some value""
          }
        },
        ""file:///home/buildAgent/bin/app.zip#/docs/intro.docx"": {
          ""uri"": ""file:///docs/intro.docx"",
          ""parentKey"": ""file:///home/buildAgent/bin/app.zip"",
          ""offset"": 17522,
          ""length"": 4050,
          ""mimeType"": ""application/vnd.openxmlformats-officedocument.wordprocessingml.document""
        }
      },
      ""results"": [],
      ""properties"": {
        ""sarifv2/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""files\"": {\r\n    \""file:///home/list.txt\"": {\r\n      \""length\"": 43,\r\n      \""mimeType\"": \""text/plain\"",\r\n      \""contents\"": {\r\n        \""text\"": \""The quick brown fox jumps over the lazy dog\"",\r\n        \""binary\"": \""VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw==\""\r\n      },\r\n      \""hashes\"": [\r\n        {\r\n          \""value\"": \""d7a8fbb307d7809469ca9abcb0082e4f8d5651e46d3cdb762d02d0bf37c9e592\"",\r\n          \""algorithm\"": \""sha-256\""\r\n        }\r\n      ]\r\n    },\r\n    \""file:///home/buildAgent/bin/app.zip\"": {\r\n      \""mimeType\"": \""application/zip\"",\r\n      \""properties\"": {\r\n        \""my_key\"": \""some value\""\r\n      }\r\n    },\r\n    \""file:///home/buildAgent/bin/app.zip#/docs/intro.docx\"": {\r\n      \""fileLocation\"": {\r\n        \""uri\"": \""file:///docs/intro.docx\""\r\n      },\r\n      \""parentKey\"": \""file:///home/buildAgent/bin/app.zip\"",\r\n      \""offset\"": 17522,\r\n      \""length\"": 4050,\r\n      \""mimeType\"": \""application/vnd.openxmlformats-officedocument.wordprocessingml.document\"",\r\n      \""contents\"": {}\r\n    }\r\n  },\r\n  \""results\"": []\r\n}""
      }
    }
  ]
}";

            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }

        [Fact]
        public void SarifTransformerTests_ToVersionOne_OneRunWithRules()
        {
            const string V2LogText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-2.0.0"",
  ""version"": ""2.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""results"": [],
      ""resources"": {
        ""rules"": {
          ""C2001"": {
            ""id"": ""C2001"",
            ""shortDescription"": {
              ""text"": ""A variable was used without being initialized.""
            },
            ""messageStrings"": {
              ""default"": ""Variable \""{0}\"" was used without being initialized.""
            },
            ""properties"": {
              ""some_key"": ""FoxForceFive""
            }
          },
          ""C2002"": {
            ""id"": ""C2002"",
            ""fullDescription"": {
              ""text"": ""Catfish season continuous hen lamb include dose copy grant.""
            },
            ""configuration"": {
              ""enabled"": true,
              ""defaultLevel"": ""error""
            },
            ""helpLocation"": {
              ""uri"": ""http://www.domain.com/rules/c2002.html""
            }
          },
          ""C2003"": {
            ""id"": ""C2003"",
            ""name"": {
              ""text"": ""Rule C2003""
            },
            ""shortDescription"": {
              ""text"": ""Rules were meant to be broken.""
            },
            ""fullDescription"": {
              ""text"": ""Rent internal rebellion competence biography photograph.""
            }
          }
        }
      }
    }
  ]
}";
            const string V1LogExpectedText =
@"{
  ""$schema"": ""http://json.schemastore.org/sarif-1.0.0"",
  ""version"": ""1.0.0"",
  ""runs"": [
    {
      ""tool"": {
        ""name"": ""CodeScanner"",
        ""semanticVersion"": ""2.1.0""
      },
      ""results"": [],
      ""rules"": {
        ""C2001"": {
          ""id"": ""C2001"",
          ""shortDescription"": ""A variable was used without being initialized."",
          ""messageFormats"": {
            ""default"": ""Variable \""{0}\"" was used without being initialized.""
          },
          ""properties"": {
            ""some_key"": ""FoxForceFive""
          }
        },
        ""C2002"": {
          ""id"": ""C2002"",
          ""fullDescription"": ""Catfish season continuous hen lamb include dose copy grant."",
          ""configuration"": ""enabled"",
          ""defaultLevel"": ""error"",
          ""helpUri"": ""http://www.domain.com/rules/c2002.html""
        },
        ""C2003"": {
          ""id"": ""C2003"",
          ""name"": ""Rule C2003"",
          ""shortDescription"": ""Rules were meant to be broken."",
          ""fullDescription"": ""Rent internal rebellion competence biography photograph.""
        }
      },
      ""properties"": {
        ""sarifv2/run"": ""{\r\n  \""tool\"": {\r\n    \""name\"": \""CodeScanner\"",\r\n    \""semanticVersion\"": \""2.1.0\""\r\n  },\r\n  \""results\"": [],\r\n  \""resources\"": {\r\n    \""rules\"": {\r\n      \""C2001\"": {\r\n        \""id\"": \""C2001\"",\r\n        \""shortDescription\"": {\r\n          \""text\"": \""A variable was used without being initialized.\""\r\n        },\r\n        \""messageStrings\"": {\r\n          \""default\"": \""Variable \\\""{0}\\\"" was used without being initialized.\""\r\n        },\r\n        \""properties\"": {\r\n          \""some_key\"": \""FoxForceFive\""\r\n        }\r\n      },\r\n      \""C2002\"": {\r\n        \""id\"": \""C2002\"",\r\n        \""fullDescription\"": {\r\n          \""text\"": \""Catfish season continuous hen lamb include dose copy grant.\""\r\n        },\r\n        \""configuration\"": {\r\n          \""enabled\"": true,\r\n          \""defaultLevel\"": \""error\""\r\n        },\r\n        \""helpLocation\"": {\r\n          \""uri\"": \""http://www.domain.com/rules/c2002.html\""\r\n        }\r\n      },\r\n      \""C2003\"": {\r\n        \""id\"": \""C2003\"",\r\n        \""name\"": {\r\n          \""text\"": \""Rule C2003\""\r\n        },\r\n        \""shortDescription\"": {\r\n          \""text\"": \""Rules were meant to be broken.\""\r\n        },\r\n        \""fullDescription\"": {\r\n          \""text\"": \""Rent internal rebellion competence biography photograph.\""\r\n        }\r\n      }\r\n    }\r\n  }\r\n}""
      }
    }
  ]
}";
            VerifyCurrentToVersionOneTransformation(V2LogText, V1LogExpectedText);
        }
    }
}