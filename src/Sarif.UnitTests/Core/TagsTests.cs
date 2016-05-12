// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif.Readers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif.Core
{
    [TestClass]
    public class TagsTests
    {
        private readonly TagsTestClass _testObject = new TagsTestClass();

        private class TagsTestClass : PropertyBagHolder
        {
            internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }
        }

        [TestMethod]
        public void Tags_IsInitiallyEmpty()
        {
            _testObject.Tags.Should().BeEmpty();
            _testObject.Tags.Count.Should().Be(0);
            _testObject.Tags.Contains("x").Should().BeFalse();
        }

        [TestMethod]
        public void Tags_Add_AddsTag()
        {
            _testObject.Tags.Add("x");

            _testObject.Tags.Count.Should().Be(1);
            _testObject.Tags.Contains("x").Should().BeTrue();
        }

        [TestMethod]
        public void Tags_Add_AddsTagsOnlyOnce()
        {
            _testObject.Tags.Add("x");
            _testObject.Tags.Add("x");

            _testObject.Tags.Count.Should().Be(1);
            _testObject.Tags.Contains("x").Should().BeTrue();
        }

        [TestMethod]
        public void Tags_Add_AddsMultipleTags()
        {
            _testObject.Tags.Add("x");
            _testObject.Tags.Add("y");

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Contains("x").Should().BeTrue();
            _testObject.Tags.Contains("y").Should().BeTrue();
        }

        [TestMethod]
        public void Tags_Clear_ClearsTags()
        {
            _testObject.Tags.Add("x");
            _testObject.Tags.Add("y");

            _testObject.Tags.Clear();

            _testObject.Tags.Count.Should().Be(0);
            _testObject.Tags.Contains("x").Should().BeFalse();
            _testObject.Tags.Contains("y").Should().BeFalse();
        }

        [TestMethod]
        public void Tags_Clear_WorksOnEmptyTags()
        {
            _testObject.Tags.Clear();

            _testObject.Tags.Count.Should().Be(0);
        }

        [TestMethod]
        public void Tags_CopyTo_CopiesToArray()
        {
            _testObject.Tags.Add("x");
            _testObject.Tags.Add("y");
            var array = new string[] { "a", "b", "c", "d" };

            _testObject.Tags.CopyTo(array, 1);

            array.Should().ContainInOrder("a", "x", "y", "d");
        }

        [TestMethod]
        public void Tags_CopyTo_WorksOnEmptyTags()
        {
            var array = new string[] { "a", "b", "c", "d" };

            _testObject.Tags.CopyTo(array, 1);

            array.Should().ContainInOrder("a", "b", "c", "d");
        }

        [TestMethod]
        public void Tags_ExceptWith_RemovesSpecifiedElements()
        {
            _testObject.Tags.Add("a");
            _testObject.Tags.Add("b");
            _testObject.Tags.Add("c");
            _testObject.Tags.Add("d");

            _testObject.Tags.ExceptWith(new[] { "b", "c", "e" });

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Should().ContainInOrder("a", "d");
        }

        [TestMethod]
        public void Tags_ExceptWith_WorksWithEmptyTags()
        {
            _testObject.Tags.ExceptWith(new[] { "b", "c", "e" });

            _testObject.Tags.Count.Should().Be(0);
        }

        [TestMethod]
        public void Tags_IntersectWith_ReturnsCommonElements()
        {
            _testObject.Tags.Add("a");
            _testObject.Tags.Add("b");
            _testObject.Tags.Add("c");
            _testObject.Tags.Add("d");

            _testObject.Tags.IntersectWith(new[] { "b", "c", "e" });

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Should().ContainInOrder("b", "c");
        }

        [TestMethod]
        public void Tags_IntersectWith_WorksWithEmptyTags()
        {
            _testObject.Tags.IntersectWith(new[] { "b", "c", "e" });

            _testObject.Tags.Count.Should().Be(0);
        }
    }
}
