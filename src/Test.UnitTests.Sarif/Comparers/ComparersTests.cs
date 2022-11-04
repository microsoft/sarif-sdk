// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Test.Utilities.Sarif;

using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Comparers
{
    public class ComparersTests
    {
        private readonly ITestOutputHelper output;

        public ComparersTests(ITestOutputHelper outputHelper)
        {
            this.output = outputHelper;
        }

        [Fact]
        public void CompareList_Shuffle_Tests()
        {
            Random random = RandomSarifLogGenerator.GenerateRandomAndLog(this.output);

            IList<int> originalList = Enumerable.Range(-100, 200).ToList();

            IList<int> shuffledList = originalList.ToList().Shuffle(random);

            int result = 0, newResult = 0;

            result = originalList.ListCompares(shuffledList);
            result.Should().NotBe(0);

            newResult = shuffledList.ListCompares(originalList);
            newResult.Should().Be(result * -1);

            IList<int> sortedList = shuffledList.OrderBy(i => i).ToList();

            result = originalList.ListCompares(sortedList);
            result.Should().Be(0);

            newResult = originalList.ListCompares(sortedList);
            newResult.Should().Be(0);
        }

        [Fact]
        public void CompareList_BothNull_Tests()
        {
            IList<int> list1 = null;
            IList<int> list2 = null;

            list1.ListCompares(list2).Should().Be(0);
            list2.ListCompares(list1).Should().Be(0);
        }

        [Fact]
        public void CompareList_CompareNullToNotNull_Tests()
        {
            IList<int> list1 = null;
            IList<int> list2 = Enumerable.Range(-10, 20).ToList();

            list1.ListCompares(list2).Should().Be(-1);
            list2.ListCompares(list1).Should().Be(1);
        }

        [Fact]
        public void CompareList_DifferentCount_Tests()
        {
            IList<int> list1 = Enumerable.Range(0, 11).ToList();
            IList<int> list2 = Enumerable.Range(0, 10).ToList();

            list1.ListCompares(list2).Should().Be(1);
            list2.ListCompares(list1).Should().Be(-1);
        }

        [Fact]
        public void CompareList_SameCountDifferentElement_Tests()
        {
            IList<int> list1 = Enumerable.Range(0, 10).ToList();
            IList<int> list2 = Enumerable.Range(1, 10).ToList();

            list1.ListCompares(list2).Should().Be(-1);
            list2.ListCompares(list1).Should().Be(1);
        }

        [Fact]
        public void CompareList_WithNullComparer_Tests()
        {
            var tool = new ToolComponent { Guid = Guid.Empty };

            IList<Run> runs1 = new[] { new Run { Tool = new Tool { Driver = tool } } };
            IList<Run> runs2 = Array.Empty<Run>();

            Action act = () => runs1.ListCompares(runs2, comparer: null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CompareList_WithComparer_Tests()
        {
            IList<Run> run1 = new List<Run>
            {
                null,
                new Run { Tool = new Tool { Driver = new ToolComponent { Guid = Guid.NewGuid() } } }
            };

            IList<Run> run2 = new List<Run>
            {
                new Run { Tool = new Tool { Driver = new ToolComponent { Guid = Guid.NewGuid() } } },
                null
            };

            int result = run1.ListCompares(run2, RunComparer.Instance);
            result.Should().Be(-1);

            result = run2.ListCompares(run1, RunComparer.Instance);
            result.Should().Be(1);
        }

        [Fact]
        public void CompareDictionary_Shuffle_Tests()
        {
            var dict1 = new Dictionary<string, string>
            {
                { "b", "b" },
                { "c", "c" },
                { "d", "d" }
            };

            var dict2 = new Dictionary<string, string>
            {
                { "d", "d" },
                { "c", "c" },
                { "b", "b" }
            };

            int result = dict1.DictionaryCompares(dict2);
            result.Should().Be(-1);

            result = dict2.DictionaryCompares(dict1);
            result.Should().Be(1);

            dict2 = new Dictionary<string, string>
            {
                { "a", "a" },
                { "b", "b" },
                { "c", "c" }
            };

            result = dict1.DictionaryCompares(dict2);
            result.Should().Be(1);

            result = dict2.DictionaryCompares(dict1);
            result.Should().Be(-1);

            dict2 = new Dictionary<string, string>
            {
                { "b", "a" },
                { "c", "c" },
                { "d", "d" }
            };

            result = dict1.DictionaryCompares(dict2);
            result.Should().Be(1);

            result = dict2.DictionaryCompares(dict1);
            result.Should().Be(-1);
        }

        [Fact]
        public void CompareDictionary_BothNull_Tests()
        {
            IDictionary<string, string> dict1 = null;
            IDictionary<string, string> dict2 = null;

            dict1.DictionaryCompares(dict2).Should().Be(0);
            dict2.DictionaryCompares(dict1).Should().Be(0);
        }

        [Fact]
        public void CompareDictionary_CompareNullToNotNull_Tests()
        {
            IDictionary<string, string> dict1 = null;
            IDictionary<string, string> dict2 = new Dictionary<string, string>() { { "a", "a" } };

            dict1.DictionaryCompares(dict2).Should().Be(-1);
            dict2.DictionaryCompares(dict1).Should().Be(1);
        }

        [Fact]
        public void CompareDictionary_DifferentCount_Tests()
        {
            var dict1 = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" } };
            var dict2 = new Dictionary<string, string>() { { "c", "c" } };

            dict1.DictionaryCompares(dict2).Should().Be(1);
            dict2.DictionaryCompares(dict1).Should().Be(-1);
        }

        [Fact]
        public void CompareDictionary_SameCountDifferentElement_Tests()
        {
            var dict1 = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" } };
            var dict2 = new Dictionary<string, string>() { { "c", "c" }, { "d", "d" } };

            dict1.DictionaryCompares(dict2).Should().Be(-1);
            dict2.DictionaryCompares(dict1).Should().Be(1);

            dict1 = new Dictionary<string, string>() { { "a", "a" }, { "b", "b" }, { "c", "c" } };
            dict2 = new Dictionary<string, string>() { { "c", "c" }, { "b", "b" }, { "a", "a" } };

            dict1.DictionaryCompares(dict2).Should().Be(-1);
            dict2.DictionaryCompares(dict1).Should().Be(1);
        }

        [Fact]
        public void CompareDictionary_WithNullComparer_Tests()
        {
            var loc1 = new Dictionary<string, Location>();
            var loc2 = new Dictionary<string, Location>();

            Action act = () => loc1.DictionaryCompares(loc2, comparer: null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CompareDictionary_WithComparer_Tests()
        {
            var loc1 = new Dictionary<string, Location>
            {
                { "1", null },
                { "2", new Location { Id = 2 } }
            };

            var loc2 = new Dictionary<string, Location>
            {
                { "1", new Location { Id = 1 } },
                { "2", new Location { Id = 2 } }
            };

            int result = loc1.DictionaryCompares(loc2, LocationComparer.Instance);
            result.Should().Be(-1);

            result = loc2.DictionaryCompares(loc1, LocationComparer.Instance);
            result.Should().Be(1);
        }

        [Fact]
        public void ArtifactContentComparer_Tests()
        {
            var list1 = new List<ArtifactContent>();
            var list2 = new List<ArtifactContent>();

            list1.Add(null);
            list2.Add(null);
            list1.ListCompares(list2, ArtifactContentComparer.Instance).Should().Be(0);

            list1.Add(new ArtifactContent() { Text = "content 1" });
            list2.Add(new ArtifactContent() { Text = "content 2" });

            list1.ListCompares(list2, ArtifactContentComparer.Instance).Should().Be(-1);
            list2.ListCompares(list1, ArtifactContentComparer.Instance).Should().Be(1);
            list1.Clear();
            list2.Clear();

            list1.Add(new ArtifactContent() { Binary = "WUJDMTIz" });
            list2.Add(new ArtifactContent() { Binary = "QUJDMTIz" });

            list1.ListCompares(list2, ArtifactContentComparer.Instance).Should().Be(1);
            list2.ListCompares(list1, ArtifactContentComparer.Instance).Should().Be(-1);
            list1.Clear();
            list2.Clear();

            list1.Add(new ArtifactContent() { Text = "content 1", Rendered = new MultiformatMessageString { Markdown = "`markdown`" } });
            list2.Add(new ArtifactContent() { Text = "content 1", Rendered = new MultiformatMessageString { Markdown = "title" } });
            list1.ListCompares(list2, ArtifactContentComparer.Instance).Should().Be(-1);
            list2.ListCompares(list1, ArtifactContentComparer.Instance).Should().Be(1);
        }

        [Fact]
        public void ReportingConfigurationComparer_Tests()
        {
            var rules1 = new List<ReportingConfiguration>();
            var rules2 = new List<ReportingConfiguration>();

            rules1.Add(null);
            rules2.Add(null);

            rules1.ListCompares(rules2, ReportingConfigurationComparer.Instance).Should().Be(0);

            rules1.Add(new ReportingConfiguration() { Rank = 26.648d });
            rules2.Add(new ReportingConfiguration() { Rank = 87.1d });

            rules1.ListCompares(rules2, ReportingConfigurationComparer.Instance).Should().Be(-1);
            rules2.ListCompares(rules1, ReportingConfigurationComparer.Instance).Should().Be(1);

            rules1.Insert(0, new ReportingConfiguration() { Level = FailureLevel.Error });
            rules2.Insert(0, new ReportingConfiguration() { Level = FailureLevel.Warning });

            rules1.ListCompares(rules2, ReportingConfigurationComparer.Instance).Should().Be(1);
            rules2.ListCompares(rules1, ReportingConfigurationComparer.Instance).Should().Be(-1);

            rules1.Insert(0, new ReportingConfiguration() { Enabled = false, Rank = 80d });
            rules2.Insert(0, new ReportingConfiguration() { Enabled = true, Rank = 80d });

            rules1.ListCompares(rules2, ReportingConfigurationComparer.Instance).Should().Be(-1);
            rules2.ListCompares(rules1, ReportingConfigurationComparer.Instance).Should().Be(1);
        }

        [Fact]
        public void ToolComponentComparer_Tests()
        {
            var list1 = new List<ToolComponent>();
            var list2 = new List<ToolComponent>();

            list1.Add(null);
            list2.Add(null);

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(0);

            list1.Insert(0, new ToolComponent() { Guid = Guid.Empty });
            list2.Insert(0, new ToolComponent() { Guid = Guid.NewGuid() });

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(-1);
            list2.ListCompares(list1, ToolComponentComparer.Instance).Should().Be(1);

            list1.Insert(0, new ToolComponent() { Name = "scan tool" });
            list2.Insert(0, new ToolComponent() { Name = "code scan tool" });

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(1);
            list2.ListCompares(list1, ToolComponentComparer.Instance).Should().Be(-1);

            list1.Insert(0, new ToolComponent() { Organization = "MS", Name = "scan tool" });
            list2.Insert(0, new ToolComponent() { Organization = "Microsoft", Name = "scan tool" });

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(1);
            list2.ListCompares(list1, ToolComponentComparer.Instance).Should().Be(-1);

            list1.Insert(0, new ToolComponent() { Product = "PREfast", Name = "scan tool" });
            list2.Insert(0, new ToolComponent() { Product = "prefast", Name = "scan tool" });

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(1);
            list2.ListCompares(list1, ToolComponentComparer.Instance).Should().Be(-1);

            list1.Insert(0, new ToolComponent() { FullName = "Analysis Linter", Name = "scan tool" });
            list2.Insert(0, new ToolComponent() { FullName = "Analysis Linter Tool", Name = "scan tool" });

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(-1);
            list2.ListCompares(list1, ToolComponentComparer.Instance).Should().Be(1);

            list1.Insert(0, new ToolComponent() { Version = "CWR-2022-01", Name = "scan tool" });
            list2.Insert(0, new ToolComponent() { Version = "CWR-2021-12", Name = "scan tool" });

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(1);
            list2.ListCompares(list1, ToolComponentComparer.Instance).Should().Be(-1);

            list1.Insert(0, new ToolComponent() { SemanticVersion = "1.0.1", Name = "scan tool" });
            list2.Insert(0, new ToolComponent() { SemanticVersion = "1.0.3", Name = "scan tool" });

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(-1);
            list2.ListCompares(list1, ToolComponentComparer.Instance).Should().Be(1);

            list1.Insert(0, new ToolComponent() { ReleaseDateUtc = "2/8/2022", Name = "scan tool" });
            list2.Insert(0, new ToolComponent() { ReleaseDateUtc = "1/1/2022", Name = "scan tool" });

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(1);
            list2.ListCompares(list1, ToolComponentComparer.Instance).Should().Be(-1);

            list1.Insert(0, new ToolComponent() { DownloadUri = new Uri("https://example/download/v1"), Name = "scan tool" });
            list2.Insert(0, new ToolComponent() { DownloadUri = new Uri("https://example/download/v2"), Name = "scan tool" });

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(-1);
            list2.ListCompares(list1, ToolComponentComparer.Instance).Should().Be(1);

            list1.Insert(0, new ToolComponent() { Rules = new ReportingDescriptor[] { new ReportingDescriptor { Id = "TESTRULE001" } }, Name = "scan tool" });
            list2.Insert(0, new ToolComponent() { Rules = new ReportingDescriptor[] { new ReportingDescriptor { Id = "TESTRULE002" } }, Name = "scan tool" });

            list1.ListCompares(list2, ToolComponentComparer.Instance).Should().Be(-1);
            list2.ListCompares(list1, ToolComponentComparer.Instance).Should().Be(1);
        }

        [Fact]
        public void ReportingDescriptorComparer_Tests()
        {
            var rules1 = new List<ReportingDescriptor>();
            var rules2 = new List<ReportingDescriptor>();

            rules1.Add(null);
            rules2.Add(null);

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(0);

            rules1.Insert(0, new ReportingDescriptor() { Id = "TestRule1" });
            rules2.Insert(0, new ReportingDescriptor() { Id = "TestRule2" });

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(-1);
            rules2.ListCompares(rules1, ReportingDescriptorComparer.Instance).Should().Be(1);

            rules1.Insert(0, new ReportingDescriptor() { DeprecatedIds = new string[] { "OldRuleId3" }, Id = "TestRule1" });
            rules2.Insert(0, new ReportingDescriptor() { DeprecatedIds = new string[] { "OldRuleId2" }, Id = "TestRule1" });

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(1);
            rules2.ListCompares(rules1, ReportingDescriptorComparer.Instance).Should().Be(-1);

            rules1.Insert(0, new ReportingDescriptor() { Guid = Guid.NewGuid(), Id = "TestRule1" });
            rules2.Insert(0, new ReportingDescriptor() { Guid = Guid.Empty, Id = "TestRule1" });

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(1);
            rules2.ListCompares(rules1, ReportingDescriptorComparer.Instance).Should().Be(-1);

            rules1.Insert(0, new ReportingDescriptor() { DeprecatedIds = new string[] { Guid.Empty.ToString() }, Id = "TestRule1" });
            rules2.Insert(0, new ReportingDescriptor() { DeprecatedIds = new string[] { Guid.NewGuid().ToString() }, Id = "TestRule1" });

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(-1);
            rules2.ListCompares(rules1, ReportingDescriptorComparer.Instance).Should().Be(1);

            rules1.Insert(0, new ReportingDescriptor() { Name = "UnusedVariable", Id = "TestRule1" });
            rules2.Insert(0, new ReportingDescriptor() { Name = "", Id = "TestRule1" });

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(1);
            rules2.ListCompares(rules1, ReportingDescriptorComparer.Instance).Should().Be(-1);

            rules1.Insert(0, new ReportingDescriptor() { ShortDescription = new MultiformatMessageString { Text = "Remove unused variable" }, Id = "TestRule1" });
            rules2.Insert(0, new ReportingDescriptor() { ShortDescription = new MultiformatMessageString { Text = "Wrong description" }, Id = "TestRule1" });

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(-1);
            rules2.ListCompares(rules1, ReportingDescriptorComparer.Instance).Should().Be(1);

            rules1.Insert(0, new ReportingDescriptor() { FullDescription = new MultiformatMessageString { Text = "Remove unused variable" }, Id = "TestRule1" });
            rules2.Insert(0, new ReportingDescriptor() { FullDescription = new MultiformatMessageString { Text = "Wrong description" }, Id = "TestRule1" });

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(-1);
            rules2.ListCompares(rules1, ReportingDescriptorComparer.Instance).Should().Be(1);

            rules1.Insert(0, new ReportingDescriptor() { DefaultConfiguration = new ReportingConfiguration { Level = FailureLevel.Note }, Id = "TestRule1" });
            rules2.Insert(0, new ReportingDescriptor() { DefaultConfiguration = new ReportingConfiguration { Level = FailureLevel.Error }, Id = "TestRule1" });

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(-1);
            rules2.ListCompares(rules1, ReportingDescriptorComparer.Instance).Should().Be(1);

            rules1.Insert(0, new ReportingDescriptor() { HelpUri = new Uri("http://example.net/rule/id"), Id = "TestRule1" });
            rules2.Insert(0, new ReportingDescriptor() { HelpUri = new Uri("http://example.net"), Id = "TestRule1" });

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(1);
            rules2.ListCompares(rules1, ReportingDescriptorComparer.Instance).Should().Be(-1);

            rules1.Insert(0, new ReportingDescriptor() { Help = new MultiformatMessageString { Text = "Helping texts." }, Id = "TestRule1" });
            rules2.Insert(0, new ReportingDescriptor() { Help = new MultiformatMessageString { Text = "For customers." }, Id = "TestRule1" });

            rules1.ListCompares(rules2, ReportingDescriptorComparer.Instance).Should().Be(1);
            rules2.ListCompares(rules1, ReportingDescriptorComparer.Instance).Should().Be(-1);
        }

        [Fact]
        public void RegionComparer_Tests()
        {
            var regions1 = new List<Region>();
            var regions2 = new List<Region>();

            regions1.Add(null);
            regions2.Add(null);

            regions1.ListCompares(regions2, RegionComparer.Instance).Should().Be(0);

            regions1.Insert(0, new Region() { StartLine = 0, StartColumn = 0 });
            regions2.Insert(0, new Region() { StartLine = 1, StartColumn = 0 });

            regions1.ListCompares(regions2, RegionComparer.Instance).Should().Be(-1);
            regions2.ListCompares(regions1, RegionComparer.Instance).Should().Be(1);

            regions1.Insert(0, new Region() { StartLine = 0, StartColumn = 1 });
            regions2.Insert(0, new Region() { StartLine = 0, StartColumn = 0 });

            regions1.ListCompares(regions2, RegionComparer.Instance).Should().Be(1);
            regions2.ListCompares(regions1, RegionComparer.Instance).Should().Be(-1);

            regions1.Insert(0, new Region() { StartLine = 10, EndLine = 11, StartColumn = 0 });
            regions2.Insert(0, new Region() { StartLine = 10, EndLine = 10, StartColumn = 0 });

            regions1.ListCompares(regions2, RegionComparer.Instance).Should().Be(1);
            regions2.ListCompares(regions1, RegionComparer.Instance).Should().Be(-1);

            regions1.Insert(0, new Region() { StartLine = 10, EndLine = 10, StartColumn = 5, EndColumn = 23 });
            regions2.Insert(0, new Region() { StartLine = 10, EndLine = 10, StartColumn = 5, EndColumn = 7 });

            regions1.ListCompares(regions2, RegionComparer.Instance).Should().Be(1);
            regions2.ListCompares(regions1, RegionComparer.Instance).Should().Be(-1);

            regions1.Insert(0, new Region() { CharOffset = 100, CharLength = 30 });
            regions2.Insert(0, new Region() { CharOffset = 36, CharLength = 30 });

            regions1.ListCompares(regions2, RegionComparer.Instance).Should().Be(1);
            regions2.ListCompares(regions1, RegionComparer.Instance).Should().Be(-1);

            regions1.Insert(0, new Region() { CharOffset = 100, CharLength = 47 });
            regions2.Insert(0, new Region() { CharOffset = 100, CharLength = 326 });

            regions1.ListCompares(regions2, RegionComparer.Instance).Should().Be(-1);
            regions2.ListCompares(regions1, RegionComparer.Instance).Should().Be(1);

            regions1.Insert(0, new Region() { ByteOffset = 226, ByteLength = 11 });
            regions2.Insert(0, new Region() { ByteOffset = 1623, ByteLength = 11 });

            regions1.ListCompares(regions2, RegionComparer.Instance).Should().Be(-1);
            regions2.ListCompares(regions1, RegionComparer.Instance).Should().Be(1);

            regions1.Insert(0, new Region() { ByteOffset = 67, ByteLength = 9 });
            regions2.Insert(0, new Region() { ByteOffset = 67, ByteLength = 11 });

            regions1.ListCompares(regions2, RegionComparer.Instance).Should().Be(-1);
            regions2.ListCompares(regions1, RegionComparer.Instance).Should().Be(1);
        }

        [Fact]
        public void AddressComparer_Tests()
        {
            var address1 = new List<Address>();
            var address2 = new List<Address>();

            address1.Add(null);
            address2.Add(null);

            address1.ListCompares(address2, AddressComparer.Instance).Should().Be(0);

            address1.Insert(0, new Address() { AbsoluteAddress = 0xAAA });
            address2.Insert(0, new Address() { AbsoluteAddress = 0xBBB });

            address1.ListCompares(address2, AddressComparer.Instance).Should().Be(-1);
            address2.ListCompares(address1, AddressComparer.Instance).Should().Be(1);

            address1.Insert(0, new Address() { RelativeAddress = null, AbsoluteAddress = 0xAAA });
            address2.Insert(0, new Address() { RelativeAddress = 0x0, AbsoluteAddress = 0xAAA });

            address1.ListCompares(address2, AddressComparer.Instance).Should().Be(-1);
            address2.ListCompares(address1, AddressComparer.Instance).Should().Be(1);

            address1.Insert(0, new Address() { Length = 10, RelativeAddress = 0x101, AbsoluteAddress = 0xAAA });
            address2.Insert(0, new Address() { Length = null, RelativeAddress = 0x101, AbsoluteAddress = 0xAAA });

            address1.ListCompares(address2, AddressComparer.Instance).Should().Be(1);
            address2.ListCompares(address1, AddressComparer.Instance).Should().Be(-1);

            address1.Insert(0, new Address() { OffsetFromParent = 0x255, Length = 10, RelativeAddress = 0x101, AbsoluteAddress = 0xAAA });
            address2.Insert(0, new Address() { OffsetFromParent = 0x0, Length = 10, RelativeAddress = 0x101, AbsoluteAddress = 0xAAA });

            address1.Insert(0, new Address() { Name = "GetFunctionName()", OffsetFromParent = 0x255, Length = 10, RelativeAddress = 0x101, AbsoluteAddress = 0xAAA });
            address2.Insert(0, new Address() { Name = "VariableName", OffsetFromParent = 0x255, Length = 10, RelativeAddress = 0x101, AbsoluteAddress = 0xAAA });

            address1.ListCompares(address2, AddressComparer.Instance).Should().Be(-1);
            address2.ListCompares(address1, AddressComparer.Instance).Should().Be(1);

            address1.Insert(0, new Address() { Kind = "function", Name = "GetFunctionName()", OffsetFromParent = 0x255, Length = 10, RelativeAddress = 0x101, AbsoluteAddress = 0xAAA });
            address2.Insert(0, new Address() { Kind = "system", Name = "GetFunctionName()", OffsetFromParent = 0x255, Length = 10, RelativeAddress = 0x101, AbsoluteAddress = 0xAAA });

            address1.ListCompares(address2, AddressComparer.Instance).Should().Be(-1);
            address2.ListCompares(address1, AddressComparer.Instance).Should().Be(1);

            address1.Insert(0, new Address() { FullyQualifiedName = "namespace::GetFunctionName", Kind = "function", Name = "GetFunctionName()", OffsetFromParent = 0x255, Length = 10, RelativeAddress = 0x101, AbsoluteAddress = 0xAAA });
            address2.Insert(0, new Address() { FullyQualifiedName = null, Kind = "function", Name = "GetFunctionName()", OffsetFromParent = 0x255, Length = 10, RelativeAddress = 0x101, AbsoluteAddress = 0xAAA });

            address1.ListCompares(address2, AddressComparer.Instance).Should().Be(1);
            address2.ListCompares(address1, AddressComparer.Instance).Should().Be(-1);
        }

        [Fact]
        public void ArtifactComparer_Tests()
        {
            var artifacts1 = new List<Artifact>();
            var artifacts2 = new List<Artifact>();

            artifacts1.Add(null);
            artifacts2.Add(null);

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(0);

            artifacts1.Insert(0, new Artifact() { Description = new Message { Text = "Represents for an artifact" } });
            artifacts2.Insert(0, new Artifact() { Description = new Message { Text = "A source file artifact" } });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(-1);

            artifacts1.Insert(0, new Artifact() { Location = new ArtifactLocation { Index = 0 } });
            artifacts2.Insert(0, new Artifact() { Location = new ArtifactLocation { Index = 1 } });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(-1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(1);

            artifacts1.Insert(0, new Artifact() { ParentIndex = 0 });
            artifacts2.Insert(0, new Artifact() { ParentIndex = 1 });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(-1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(1);

            artifacts1.Insert(0, new Artifact() { Offset = 2 });
            artifacts2.Insert(0, new Artifact() { Offset = 1 });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(-1);

            artifacts1.Insert(0, new Artifact() { Length = 102542 });
            artifacts2.Insert(0, new Artifact() { Length = -1 });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(-1);

            artifacts1.Insert(0, new Artifact() { Roles = ArtifactRoles.AnalysisTarget | ArtifactRoles.Attachment });
            artifacts2.Insert(0, new Artifact() { Roles = ArtifactRoles.Policy });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(-1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(1);

            artifacts1.Insert(0, new Artifact() { MimeType = "text" });
            artifacts2.Insert(0, new Artifact() { MimeType = "video" });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(-1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(1);

            artifacts1.Insert(0, new Artifact() { Contents = new ArtifactContent { Text = "\"string\"" } });
            artifacts2.Insert(0, new Artifact() { Contents = new ArtifactContent { Text = "var result = 0;" } });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(-1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(1);

            artifacts1.Insert(0, new Artifact() { Encoding = "UTF-16BE" });
            artifacts2.Insert(0, new Artifact() { Encoding = "UTF-16LE" });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(-1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(1);

            artifacts1.Insert(0, new Artifact() { SourceLanguage = "html" });
            artifacts2.Insert(0, new Artifact() { SourceLanguage = "csharp/7" });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(-1);

            artifacts1.Insert(0, new Artifact() { Hashes = new Dictionary<string, string> { { "sha-256", "..." } } });
            artifacts2.Insert(0, new Artifact() { Hashes = new Dictionary<string, string> { { "sha-512", "..." } } });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(-1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(1);

            artifacts1.Insert(0, new Artifact() { LastModifiedTimeUtc = DateTime.UtcNow });
            artifacts2.Insert(0, new Artifact() { LastModifiedTimeUtc = DateTime.UtcNow.AddDays(-1) });

            artifacts1.ListCompares(artifacts2, ArtifactComparer.Instance).Should().Be(1);
            artifacts2.ListCompares(artifacts1, ArtifactComparer.Instance).Should().Be(-1);
        }

        [Fact]
        public void ThreadFlowComparer_Tests()
        {
            var threadFlow1 = new List<ThreadFlow>();
            var threadFlow2 = new List<ThreadFlow>();

            threadFlow1.Add(null);
            threadFlow2.Add(null);

            threadFlow1.ListCompares(threadFlow2, ThreadFlowComparer.Instance).Should().Be(0);

            threadFlow1.Insert(0, new ThreadFlow() { Id = "threadFlow1" });
            threadFlow2.Insert(0, new ThreadFlow() { Id = "threadFlow2" });

            threadFlow1.ListCompares(threadFlow2, ThreadFlowComparer.Instance).Should().Be(-1);
            threadFlow2.ListCompares(threadFlow1, ThreadFlowComparer.Instance).Should().Be(1);

            threadFlow1.Insert(0, new ThreadFlow() { Message = new Message { Id = "arg1" } });
            threadFlow2.Insert(0, new ThreadFlow() { Message = new Message { Id = "fileArg" } });

            threadFlow1.ListCompares(threadFlow2, ThreadFlowComparer.Instance).Should().Be(-1);
            threadFlow2.ListCompares(threadFlow1, ThreadFlowComparer.Instance).Should().Be(1);

            var loc1 = new Location
            {
                PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = new Uri("path/to/file1.c", UriKind.Relative)
                    }
                }
            };

            var loc2 = new Location
            {
                PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = new Uri("path/to/file2.c", UriKind.Relative)
                    }
                }
            };

            threadFlow1.Insert(0, new ThreadFlow() { Locations = new ThreadFlowLocation[] { new ThreadFlowLocation { Location = loc1 } } });
            threadFlow2.Insert(0, new ThreadFlow() { Locations = new ThreadFlowLocation[] { new ThreadFlowLocation { Location = loc2 } } });

            threadFlow1.ListCompares(threadFlow2, ThreadFlowComparer.Instance).Should().Be(-1);
            threadFlow2.ListCompares(threadFlow1, ThreadFlowComparer.Instance).Should().Be(1);
        }

        [Fact]
        public void ThreadFlowLocationComparer_Tests()
        {
            var locations1 = new List<ThreadFlowLocation>();
            var locations2 = new List<ThreadFlowLocation>();

            locations1.Add(null);
            locations2.Add(null);

            locations1.ListCompares(locations2, ThreadFlowLocationComparer.Instance).Should().Be(0);

            var loc1 = new Location
            {
                PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = new Uri("path/to/file1.c", UriKind.Relative)
                    }
                }
            };

            var loc2 = new Location
            {
                PhysicalLocation = new PhysicalLocation
                {
                    ArtifactLocation = new ArtifactLocation
                    {
                        Uri = new Uri("path/to/file2.c", UriKind.Relative)
                    }
                }
            };

            locations1.Insert(0, new ThreadFlowLocation() { Location = loc1 });
            locations2.Insert(0, new ThreadFlowLocation() { Location = loc2 });

            locations1.ListCompares(locations2, ThreadFlowLocationComparer.Instance).Should().Be(-1);
            locations2.ListCompares(locations1, ThreadFlowLocationComparer.Instance).Should().Be(1);

            locations1.Insert(0, new ThreadFlowLocation() { Index = 2, Location = loc1 });
            locations2.Insert(0, new ThreadFlowLocation() { Index = 1, Location = loc2 });

            locations1.ListCompares(locations2, ThreadFlowLocationComparer.Instance).Should().Be(1);
            locations2.ListCompares(locations1, ThreadFlowLocationComparer.Instance).Should().Be(-1);

            locations1.Insert(0, new ThreadFlowLocation() { Kinds = new string[] { "memory" }, Location = loc1 });
            locations2.Insert(0, new ThreadFlowLocation() { Kinds = new string[] { "call", "branch" }, Location = loc2 });

            locations1.ListCompares(locations2, ThreadFlowLocationComparer.Instance).Should().Be(-1);
            locations2.ListCompares(locations1, ThreadFlowLocationComparer.Instance).Should().Be(1);

            locations1.Insert(0, new ThreadFlowLocation() { NestingLevel = 3 });
            locations2.Insert(0, new ThreadFlowLocation() { NestingLevel = 2 });

            locations1.ListCompares(locations2, ThreadFlowLocationComparer.Instance).Should().Be(1);
            locations2.ListCompares(locations1, ThreadFlowLocationComparer.Instance).Should().Be(-1);

            locations1.Insert(0, new ThreadFlowLocation() { ExecutionOrder = 2 });
            locations2.Insert(0, new ThreadFlowLocation() { ExecutionOrder = 1 });

            locations1.ListCompares(locations2, ThreadFlowLocationComparer.Instance).Should().Be(1);
            locations2.ListCompares(locations1, ThreadFlowLocationComparer.Instance).Should().Be(-1);

            locations1.Insert(0, new ThreadFlowLocation() { ExecutionTimeUtc = DateTime.UtcNow });
            locations2.Insert(0, new ThreadFlowLocation() { ExecutionTimeUtc = DateTime.UtcNow.AddHours(-2) });

            locations1.ListCompares(locations2, ThreadFlowLocationComparer.Instance).Should().Be(1);
            locations2.ListCompares(locations1, ThreadFlowLocationComparer.Instance).Should().Be(-1);

            locations1.Insert(0, new ThreadFlowLocation() { Importance = ThreadFlowLocationImportance.Essential });
            locations2.Insert(0, new ThreadFlowLocation() { Importance = ThreadFlowLocationImportance.Unimportant });

            locations1.ListCompares(locations2, ThreadFlowLocationComparer.Instance).Should().Be(-1);
            locations2.ListCompares(locations1, ThreadFlowLocationComparer.Instance).Should().Be(1);
        }

        [Fact]
        public void RunComparer_Tests()
        {
            var runs1 = new List<Run>();
            var runs2 = new List<Run>();

            runs1.Add(null);
            runs2.Add(null);

            runs1.ListCompares(runs2, RunComparer.Instance).Should().Be(0);

            runs1.Insert(0, new Run() { Artifacts = new Artifact[] { new Artifact { Description = new Message { Text = "artifact 1" } } } });
            runs2.Insert(0, new Run() { Artifacts = new Artifact[] { new Artifact { Description = new Message { Text = "artifact 2" } } } });

            runs1.ListCompares(runs2, RunComparer.Instance).Should().Be(-1);
            runs2.ListCompares(runs1, RunComparer.Instance).Should().Be(1);

            var tool1 = new Tool { Driver = new ToolComponent { Name = "PREFast", Version = "1.0" } };
            var tool2 = new Tool { Driver = new ToolComponent { Name = "PREFast", Version = "1.3" } };

            runs1.Insert(0, new Run() { Tool = tool1 });
            runs2.Insert(0, new Run() { Tool = tool2 });

            runs1.ListCompares(runs2, RunComparer.Instance).Should().Be(-1);
            runs2.ListCompares(runs1, RunComparer.Instance).Should().Be(1);

            var result1 = new Result { RuleId = "CS001", Message = new Message { Text = "Issue of C# code" } };
            var result2 = new Result { RuleId = "IDE692", Message = new Message { Text = "Issue by IDE" } };

            runs1.Insert(0, new Run() { Results = new Result[] { result1 } });
            runs2.Insert(0, new Run() { Results = new Result[] { result2 } });

            runs1.ListCompares(runs2, RunComparer.Instance).Should().Be(-1);
            runs2.ListCompares(runs1, RunComparer.Instance).Should().Be(1);
        }

        [Fact]
        public void ComparerHelp_CompareUir_Tests()
        {
            var testUris = new List<(Uri, int)>()
            {
                (null, -1),
                (null, 0),
                (new Uri(@"", UriKind.RelativeOrAbsolute), 1),
                (new Uri(string.Empty, UriKind.RelativeOrAbsolute), 0),
                (new Uri(@"file.ext", UriKind.RelativeOrAbsolute), 1),
                (new Uri(@"C:\path\file.ext", UriKind.RelativeOrAbsolute), -1),
                (new Uri(@"\\hostname\path\file.ext", UriKind.RelativeOrAbsolute), -1),
                (new Uri(@"file:///C:/path/file.ext", UriKind.RelativeOrAbsolute), 1),
                (new Uri(@"\\hostname\c:\path\file.ext", UriKind.RelativeOrAbsolute), -1),
                (new Uri(@"/home/username/path/file.ext", UriKind.RelativeOrAbsolute), -1),
                (new Uri(@"nfs://servername/folder/file.ext", UriKind.RelativeOrAbsolute), 1),
                (new Uri(@"file://hostname/C:/path/file.ext", UriKind.RelativeOrAbsolute), -1),
                (new Uri(@"file:///home/username/path/file.ext", UriKind.RelativeOrAbsolute), -1),
                (new Uri(@"ftp://ftp.example.com/folder/file.ext", UriKind.RelativeOrAbsolute), 1),
                (new Uri(@"smb://servername/Share/folder/file.ext", UriKind.RelativeOrAbsolute), 1),
                (new Uri(@"dav://example.hostname.com/folder/file.ext", UriKind.RelativeOrAbsolute), -1),
                (new Uri(@"file://hostname/home/username/path/file.ext", UriKind.RelativeOrAbsolute), 1),
                (new Uri(@"ftp://username@ftp.example.com/folder/file.ext", UriKind.RelativeOrAbsolute), 1),
                (new Uri(@"scheme://servername.example.com/folder/file.ext", UriKind.RelativeOrAbsolute), 1),
                (new Uri(@"https://github.com/microsoft/sarif-sdk/file.ext", UriKind.RelativeOrAbsolute), -1),
                (new Uri(@"ssh://username@servername.example.com/folder/file.ext", UriKind.RelativeOrAbsolute), 1),
                (new Uri(@"scheme://username@servername.example.com/folder/file.ext", UriKind.RelativeOrAbsolute), -1),
                (new Uri(@"https://github.com/microsoft/sarif-sdk/file.ext?some-query-string", UriKind.RelativeOrAbsolute), -1),
            };

            for (int i = 1; i < testUris.Count; i++)
            {
                int result = testUris[i].Item1.UriCompares(testUris[i - 1].Item1);
                result.Should().Be(testUris[i].Item2);
            }
        }
    }
}
