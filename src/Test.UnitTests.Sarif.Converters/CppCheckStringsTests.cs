// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Xml;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public class CppCheckStringsTests
    {
        [Fact]
        public void CppCheckStrings_PutsStringsInNameTable()
        {
            var nameTable = new NameTable();
            var uut = new CppCheckStrings(nameTable);
            Assert.Same(nameTable.Add("results"), uut.Results);

            Assert.Same(nameTable.Add("cppcheck"), uut.CppCheck);
            Assert.Same(nameTable.Add("version"), uut.Version);
            Assert.Same(nameTable.Add("errors"), uut.Errors);
            Assert.Same(nameTable.Add("error"), uut.Error);

            Assert.Same(nameTable.Add("id"), uut.Id);
            Assert.Same(nameTable.Add("msg"), uut.Msg);
            Assert.Same(nameTable.Add("verbose"), uut.Verbose);
            Assert.Same(nameTable.Add("severity"), uut.Severity);

            Assert.Same(nameTable.Add("location"), uut.Location);
            Assert.Same(nameTable.Add("file"), uut.File);
            Assert.Same(nameTable.Add("line"), uut.Line);
        }
    }
}
