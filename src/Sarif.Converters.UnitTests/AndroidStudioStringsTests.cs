// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class AndroidStudioStringsTests
    {
        [Fact]
        public void AndroidStudioStrings_AddsToNameTable()
        {
            var nameTable = new NameTable();
            var uut = new AndroidStudioStrings(nameTable);

            Assert.Same(nameTable.Add("problems"), uut.Problems);
            Assert.Same(nameTable.Add("problem"), uut.Problem);
            Assert.Same(nameTable.Add("file"), uut.File);
            Assert.Same(nameTable.Add("line"), uut.Line);
            Assert.Same(nameTable.Add("module"), uut.Module);
            Assert.Same(nameTable.Add("package"), uut.Package);
            Assert.Same(nameTable.Add("entry_point"), uut.EntryPoint);
            Assert.Same(nameTable.Add("problem_class"), uut.ProblemClass);
            Assert.Same(nameTable.Add("hints"), uut.Hints);
            Assert.Same(nameTable.Add("hint"), uut.Hint);
            Assert.Same(nameTable.Add("description"), uut.Description);

            Assert.Same(nameTable.Add("TYPE"), uut.Type);
            Assert.Same(nameTable.Add("FQNAME"), uut.FQName);
            Assert.Same(nameTable.Add("severity"), uut.Severity);
            Assert.Same(nameTable.Add("attribute_key"), uut.AttributeKey);
            Assert.Same(nameTable.Add("value"), uut.Value);
        }
    }
}
