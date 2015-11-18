// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Converters
{
    [TestClass]
    public class AndroidStudioStringsTests
    {
        [TestMethod]
        public void AndroidStudioStrings_AddsToNameTable()
        {
            var nameTable = new NameTable();
            var uut = new AndroidStudioStrings(nameTable);

            Assert.AreSame(nameTable.Add("problems"), uut.Problems);
            Assert.AreSame(nameTable.Add("problem"), uut.Problem);
            Assert.AreSame(nameTable.Add("file"), uut.File);
            Assert.AreSame(nameTable.Add("line"), uut.Line);
            Assert.AreSame(nameTable.Add("module"), uut.Module);
            Assert.AreSame(nameTable.Add("package"), uut.Package);
            Assert.AreSame(nameTable.Add("entry_point"), uut.EntryPoint);
            Assert.AreSame(nameTable.Add("problem_class"), uut.ProblemClass);
            Assert.AreSame(nameTable.Add("hints"), uut.Hints);
            Assert.AreSame(nameTable.Add("hint"), uut.Hint);
            Assert.AreSame(nameTable.Add("description"), uut.Description);

            Assert.AreSame(nameTable.Add("TYPE"), uut.Type);
            Assert.AreSame(nameTable.Add("FQNAME"), uut.FQName);
            Assert.AreSame(nameTable.Add("severity"), uut.Severity);
            Assert.AreSame(nameTable.Add("attribute_key"), uut.AttributeKey);
            Assert.AreSame(nameTable.Add("value"), uut.Value);
        }
    }
}
