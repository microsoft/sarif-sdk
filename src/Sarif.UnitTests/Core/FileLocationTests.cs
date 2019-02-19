// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests.Core
{
    public class ArtifactLocationTests
    {
        [Fact]
        public void ArtifactLocation_CreateFromFilesDictionaryKey()
        {
            var sb = new StringBuilder();

            var testCases = new Tuple<string, string, ArtifactLocation>[]
            {
                new Tuple<string, string, ArtifactLocation>
                    ("file.txt", null, new ArtifactLocation { Uri = new Uri("file.txt", UriKind.Relative)}),
                new Tuple<string, string, ArtifactLocation>
                    (@"c:\file.txt", null, new ArtifactLocation { Uri = new Uri("file:///c:/file.txt", UriKind.Absolute)}),
                new Tuple<string, string, ArtifactLocation> // this test normalizes the uri to contain three slases
                    ("file://c:/file.txt", null, new ArtifactLocation { Uri = new Uri("file:///c:/file.txt", UriKind.Absolute)}),
                new Tuple<string, string, ArtifactLocation>
                    ("file:///c:/file.txt", null, new ArtifactLocation { Uri = new Uri("file:///c:/file.txt", UriKind.Absolute)}),
                new Tuple<string, string, ArtifactLocation>
                    ("file://c:/archive.zip#file.txt", null, new ArtifactLocation { Uri = new Uri("file.txt", UriKind.Relative)}),
                new Tuple<string, string, ArtifactLocation>
                    ("#SRC#file.txt", null, new ArtifactLocation { Uri = new Uri("file.txt", UriKind.Relative), UriBaseId = "SRC"}),
                new Tuple<string, string, ArtifactLocation>
                    ("#SRC#archive.zip#archive_two.zip/file.txt", "#SRC#archive.zip#archive_two.zip/", new ArtifactLocation { Uri = new Uri("file.txt", UriKind.Relative)}),
            };

            foreach (Tuple<string, string, ArtifactLocation> testCase in testCases)
            {
                var fileLocation = ArtifactLocation.CreateFromFilesDictionaryKey(key: testCase.Item1, parentKey: testCase.Item2);
                if (!fileLocation.ValueEquals(testCase.Item3))
                {
                    sb.AppendLine(string.Format("Unexpected file location conversion for key: '{0}', parentKey: '{1}'.", testCase.Item1, testCase.Item2));
                }
            }
            sb.Length.Should().Be(0, because: "all file locations conversions should succeed but the following cases failed." + Environment.NewLine + sb.ToString());
        }
    }
}
