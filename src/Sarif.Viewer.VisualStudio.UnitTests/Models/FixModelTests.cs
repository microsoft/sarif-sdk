// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class FixModelTests
    {
        private DummyFixModel InitializeDummyFixModel()
        {
            var fileSystem = new DummyFileSystem();
            fileSystem.WriteAllBytes("index.html", File.ReadAllBytes(@"TestData\FixModel\index.html"));

            var fixModel = new DummyFixModel("Test dummy", fileSystem);

            var replacements = new List<ReplacementModel>();
            replacements.Add(new ReplacementModel() { Offset = 191, DeletedLength = 0, InsertedString = "\"" });
            replacements.Add(new ReplacementModel() { Offset = 199, DeletedLength = 0, InsertedString = "\"" });
            replacements.Add(new ReplacementModel() { Offset = 233, DeletedLength = 3, InsertedString = "img" });
            replacements.ForEach(rm => rm.InsertedBytes = Encoding.UTF8.GetBytes(rm.InsertedString));

            var changeModel = new FileChangeModel() { FilePath = @"C:\source\index.html" };
            replacements.ForEach(r => changeModel.Replacements.Add(r));
            fixModel.FileChanges.Add(changeModel);

            return fixModel;
        }

        [Fact]
        public void FixModel_ApplyFix()
        {
            // Arrange
            DummyFixModel dummy = InitializeDummyFixModel();

            // Act
            dummy.ApplyFix(dummy);

            // Assert
            byte[] actual = dummy.FileSystem.ReadAllBytes("index.html");
            byte[] expected = File.ReadAllBytes(@"TestData\FixModel\index_fixed.html");
            actual.Should().Equal(expected);

            dummy.FixLedger.Count.Should().Be(1);
            SortedList<int, int> offsets = dummy.FixLedger[dummy.FileChanges[0].FilePath.ToLower()].Offsets;
            offsets.Count.Should().Be(3);

            offsets.Keys[0].Should().Be(191);
            offsets.Keys[1].Should().Be(199);
            offsets.Keys[2].Should().Be(233);
            
            offsets[191].Should().Be(1);
            offsets[199].Should().Be(1);
            offsets[233].Should().Be(0);
        }

        private class DummyFixModel : FixModel
        {
            public IFileSystem FileSystem { get; private set; }

            public Dictionary<string, FixOffsetList> FixLedger
            {
                get { return s_sourceFileFixLedger; }
            }

            public DummyFixModel(string description, IFileSystem fileSystem) : base(description, fileSystem)
            {
                FileSystem = fileSystem;
            }

            internal override void LoadFixLedger()
            {
                s_sourceFileFixLedger = new Dictionary<string, FixOffsetList>();
            }

            internal override void SaveFixLedger() { }
        }

        private class DummyFileSystem : IFileSystem
        {
            private DateTime _lastWriteTime;

            public byte[] _testFileBytes { get; set; }

            public bool FileExists(string path)
            {
                return true;
            }

            public DateTime GetLastWriteTime(string path)
            {
                return _lastWriteTime;
            }

            public byte[] ReadAllBytes(string path)
            {
                return _testFileBytes;
            }

            public void WriteAllBytes(string path, byte[] bytes)
            {
                _testFileBytes = bytes;
            }

            public void SetLastWriteTime(string path, DateTime lastWriteTime)
            {
                _lastWriteTime = lastWriteTime;
            }

            #region Not implemented
            public string GetFullPath(string path)
            {
                throw new NotImplementedException();
            }

            public string[] ReadAllLines(string path)
            {
                throw new NotImplementedException();
            }

            public string ReadAllText(string path)
            {
                throw new NotImplementedException();
            }

            public string ReadAllText(string path, Encoding encoding)
            {
                throw new NotImplementedException();
            }

            public void SetAttributes(string path, FileAttributes fileAttributes)
            {
                throw new NotImplementedException();
            }

            public void WriteAllText(string path, string contents)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
    }
}
