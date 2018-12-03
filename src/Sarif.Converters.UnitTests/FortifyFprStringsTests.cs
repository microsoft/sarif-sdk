// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class FortifyFprStringsTests
    {
        [Fact]
        public void FortifyFprStrings_ContainsCorrectStrings()
        {
            var nameTable = new NameTable();
            var uut = new FortifyFprStrings(nameTable);

            Assert.Same(nameTable.Add("CreatedTS"), uut.CreatedTimestamp);
            Assert.Same(nameTable.Add("date"), uut.DateAttribute);
            Assert.Same(nameTable.Add("time"), uut.TimeAttribute);
            Assert.Same(nameTable.Add("UUID"), uut.Uuid);
            Assert.Same(nameTable.Add("Build"), uut.Build);
            Assert.Same(nameTable.Add("BuildID"), uut.BuildId);
            Assert.Same(nameTable.Add("SourceBasePath"), uut.SourceBasePath);
            Assert.Same(nameTable.Add("SourceFiles"), uut.SourceFiles);
            Assert.Same(nameTable.Add("File"), uut.File);
            Assert.Same(nameTable.Add("size"), uut.SizeAttribute);
            Assert.Same(nameTable.Add("type"), uut.TypeAttribute);
            Assert.Same(nameTable.Add("encoding"), uut.EncodingAttribute);
            Assert.Same(nameTable.Add("Name"), uut.Name);
            Assert.Same(nameTable.Add("Vulnerabilities"), uut.Vulnerabilities);
            Assert.Same(nameTable.Add("Vulnerability"), uut.Vulnerability);
            Assert.Same(nameTable.Add("ClassID"), uut.ClassId);
            Assert.Same(nameTable.Add("AnalysisInfo"), uut.AnalysisInfo);
            Assert.Same(nameTable.Add("ReplacementDefinitions"), uut.ReplacementDefinitions);
            Assert.Same(nameTable.Add("Def"), uut.Def);
            Assert.Same(nameTable.Add("key"), uut.KeyAttribute);
            Assert.Same(nameTable.Add("value"), uut.ValueAttribute);
            Assert.Same(nameTable.Add("Unified"), uut.Unified);
            Assert.Same(nameTable.Add("Trace"), uut.Trace);
            Assert.Same(nameTable.Add("Entry"), uut.Entry);
            Assert.Same(nameTable.Add("NodeRef"), uut.NodeRef);
            Assert.Same(nameTable.Add("isDefault"), uut.IsDefaultAttribute);
            Assert.Same(nameTable.Add("label"), uut.LabelAttribute);
            Assert.Same(nameTable.Add("SourceLocation"), uut.SourceLocation);
            Assert.Same(nameTable.Add("snippet"), uut.SnippetAttribute);
            Assert.Same(nameTable.Add("path"), uut.PathAttribute);
            Assert.Same(nameTable.Add("line"), uut.LineAttribute);
            Assert.Same(nameTable.Add("lineEnd"), uut.LineEndAttribute);
            Assert.Same(nameTable.Add("colStart"), uut.ColStartAttribute);
            Assert.Same(nameTable.Add("colEnd"), uut.ColEndAttribute);
            Assert.Same(nameTable.Add("Description"), uut.Description);
            Assert.Same(nameTable.Add("CustomDescription"), uut.CustomDescription);
            Assert.Same(nameTable.Add("classID"), uut.ClassIdAttribute);
            Assert.Same(nameTable.Add("Abstract"), uut.Abstract);
            Assert.Same(nameTable.Add("Explanation"), uut.Explanation);
            Assert.Same(nameTable.Add("UnifiedNodePool"), uut.UnifiedNodePool);
            Assert.Same(nameTable.Add("Node"), uut.Node);
            Assert.Same(nameTable.Add("Action"), uut.Action);
            Assert.Same(nameTable.Add("Snippets"), uut.Snippets);
            Assert.Same(nameTable.Add("Snippet"), uut.Snippet);
            Assert.Same(nameTable.Add("id"), uut.IdAttribute);
            Assert.Same(nameTable.Add("StartLine"), uut.StartLine);
            Assert.Same(nameTable.Add("EndLine"), uut.EndLine);
            Assert.Same(nameTable.Add("Text"), uut.Text);
            Assert.Same(nameTable.Add("CommandLine"), uut.CommandLine);
            Assert.Same(nameTable.Add("Argument"), uut.Argument);
            Assert.Same(nameTable.Add("Errors"), uut.Errors);
            Assert.Same(nameTable.Add("Error"), uut.Error);
            Assert.Same(nameTable.Add("code"), uut.CodeAttribute);
            Assert.Same(nameTable.Add("MachineInfo"), uut.MachineInfo);
            Assert.Same(nameTable.Add("Hostname"), uut.Hostname);
            Assert.Same(nameTable.Add("Username"), uut.Username);
            Assert.Same(nameTable.Add("Platform"), uut.Platform);
        }
    }
}
