// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Xml;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    [TestClass]
    public class CppCheckStringsTests
    {
        [TestMethod]
        public void CppCheckStrings_PutsStringsInNameTable()
        {
            var nameTable = new NameTable();
            var uut = new CppCheckStrings(nameTable);
            Assert.AreSame(nameTable.Add("results"), uut.Results);

            Assert.AreSame(nameTable.Add("cppcheck"), uut.CppCheck);
            Assert.AreSame(nameTable.Add("version"), uut.Version);
            Assert.AreSame(nameTable.Add("errors"), uut.Errors);
            Assert.AreSame(nameTable.Add("error"), uut.Error);

            Assert.AreSame(nameTable.Add("id"), uut.Id);
            Assert.AreSame(nameTable.Add("msg"), uut.Msg);
            Assert.AreSame(nameTable.Add("verbose"), uut.Verbose);
            Assert.AreSame(nameTable.Add("severity"), uut.Severity);

            Assert.AreSame(nameTable.Add("location"), uut.Location);
            Assert.AreSame(nameTable.Add("file"), uut.File);
            Assert.AreSame(nameTable.Add("line"), uut.Line);
        }
    }
}
