// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FortifyStringsTests
    {
        [Fact]
        public void FortifyStrings_ContainsCorrectStrings()
        {
            var nameTable = new NameTable();
            var uut = new FortifyStrings(nameTable);
            Assert.Same(nameTable.Add("Issue"), uut.Issue);
            Assert.Same(nameTable.Add("iid"), uut.Iid);
            Assert.Same(nameTable.Add("ruleID"), uut.RuleId);
            Assert.Same(nameTable.Add("Category"), uut.Category);
            Assert.Same(nameTable.Add("Folder"), uut.Folder);
            Assert.Same(nameTable.Add("Kingdom"), uut.Kingdom);
            Assert.Same(nameTable.Add("Abstract"), uut.Abstract);
            Assert.Same(nameTable.Add("AbstractCustom"), uut.AbstractCustom);
            Assert.Same(nameTable.Add("Friority"), uut.Friority);
            Assert.Same(nameTable.Add("Tag"), uut.Tag);
            Assert.Same(nameTable.Add("Comment"), uut.Comment);
            Assert.Same(nameTable.Add("Primary"), uut.Primary);
            Assert.Same(nameTable.Add("Source"), uut.Source);
            Assert.Same(nameTable.Add("TraceDiagramPath"), uut.TraceDiagramPath);
            Assert.Same(nameTable.Add("ExternalCategory"), uut.ExternalCategory);
            Assert.Same(nameTable.Add("type"), uut.Type);
            Assert.Same(nameTable.Add("FileName"), uut.FileName);
            Assert.Same(nameTable.Add("FilePath"), uut.FilePath);
            Assert.Same(nameTable.Add("LineStart"), uut.LineStart);
            Assert.Same(nameTable.Add("Snippet"), uut.Snippet);
            Assert.Same(nameTable.Add("SnippetLine"), uut.SnippetLine);
            Assert.Same(nameTable.Add("TargetFunction"), uut.TargetFunction);
        }
    }
}
