// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class SemmleConverterTests
    {
        [TestMethod]
        public void SemmleConverter_SimpleCsv()
        {
            var csv = SemmleCsvRecord.BuildDefaultRecord();
            SemmleQLCoverter


//            const string testCsv = "Equals on incomparable types,Finds calls of the form x.Equals(y) with incomparable types for x and y.,warning,"Call to Equals() comparing incomparable types[[""IComparable"" | ""file://C:/Windows/Company.NET/Framework/v2.0.50727/mscorlib.dll:0:0:0:0""]] and [[""ClientAttributeValue""|""relative://ClientClient/Company.ResourceManagement.ObjectModel/ClientAttributeValue.cs:7:152:16""],[""ClientAttributeValue""|""relative://ClientClient/Company.ResourceManagement.ObjectModel/ClientAttributeValue_ISerializable.cs:14:333:16""]]",ProjectOne/Microsoft.ResourceManagement.ObjectModel/ClientResource.cs,SuiteOne/SuiteOne_v1.0-servicing_1.0.1.10511.2/ProjectOneClient/Company.ResourceManagement.ObjectModel/ProjectOneResource.cs,1,2,3,4";
        }

        private class SemmleCsvRecord
        {
            public string QueryName;
            public string Description;
            public string Severity;
            public string Message;
            public string RelativePath;
            public string Path;
            public string StartLine;
            public string StartColumn;
            public string EndLine;
            public string EndColumn;

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
                    "\"" + QueryName    + "\"," +
                    "\"" + Description  + "\"," +
                    "\"" + Severity     + "\"," +
                    "\"" + Message      + "\"," +
                    "\"" + RelativePath + "\"," +
                    "\"" + Path         + "\"," +
                    "\"" + StartLine    + "\"," +
                    "\"" + StartColumn  + "\"," +
                    "\"" + EndLine      + "\"," +
                    "\"" + EndColumn    + "\"";
            }

            private const string QueryNameDefault = nameof(SemmleCsvRecord.QueryName);
            private const string DescriptionDefault = nameof(SemmleCsvRecord.Description);
            private const string SeverityDefault = nameof(SemmleCsvRecord.Severity);
            private const string MessageDefault = nameof(SemmleCsvRecord.Message);
            private const string RelativePathDefault = nameof(SemmleCsvRecord.RelativePath);
            private const string PathDefault = nameof(SemmleCsvRecord.Path);
            private const string StartLineDefault = nameof(SemmleCsvRecord.StartLine);
            private const string StartColumnDefault = nameof(SemmleCsvRecord.StartColumn);
            private const string EndLineDefault = nameof(SemmleCsvRecord.EndLine);
            private const string EndColumnDefault = nameof(SemmleCsvRecord.EndColumn);
        }
    }
}
