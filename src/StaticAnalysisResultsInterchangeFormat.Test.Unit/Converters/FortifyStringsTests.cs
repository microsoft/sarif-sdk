// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.StaticAnalysisResultsInterchangeFormat.Converters
{
    [TestClass]
    public class FortifyStringsTests
    {
        [TestMethod]
        public void FortifyStrings_ContainsCorrectStrings()
        {
            var nameTable = new NameTable();
            var uut = new FortifyStrings(nameTable);
            Assert.AreSame(nameTable.Add("Result"), uut.Result);
            Assert.AreSame(nameTable.Add("iid"), uut.Iid);
            Assert.AreSame(nameTable.Add("ruleID"), uut.RuleId);
            Assert.AreSame(nameTable.Add("Category"), uut.Category);
            Assert.AreSame(nameTable.Add("Folder"), uut.Folder);
            Assert.AreSame(nameTable.Add("Kingdom"), uut.Kingdom);
            Assert.AreSame(nameTable.Add("Abstract"), uut.Abstract);
            Assert.AreSame(nameTable.Add("AbstractCustom"), uut.AbstractCustom);
            Assert.AreSame(nameTable.Add("Friority"), uut.Friority);
            Assert.AreSame(nameTable.Add("Tag"), uut.Tag);
            Assert.AreSame(nameTable.Add("Comment"), uut.Comment);
            Assert.AreSame(nameTable.Add("Primary"), uut.Primary);
            Assert.AreSame(nameTable.Add("Source"), uut.Source);
            Assert.AreSame(nameTable.Add("TraceDiagramPath"), uut.TraceDiagramPath);
            Assert.AreSame(nameTable.Add("ExternalCategory"), uut.ExternalCategory);
            Assert.AreSame(nameTable.Add("type"), uut.Type);
            Assert.AreSame(nameTable.Add("FileName"), uut.FileName);
            Assert.AreSame(nameTable.Add("FilePath"), uut.FilePath);
            Assert.AreSame(nameTable.Add("LineStart"), uut.LineStart);
            Assert.AreSame(nameTable.Add("Snippet"), uut.Snippet);
            Assert.AreSame(nameTable.Add("SnippetLine"), uut.SnippetLine);
            Assert.AreSame(nameTable.Add("TargetFunction"), uut.TargetFunction);
        }
    }
}
