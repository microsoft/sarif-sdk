// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class TagsTests
    {
        private readonly TagsTestClass _testObject = new TagsTestClass();

        private class TagsTestClass : PropertyBagHolder
        {
            internal override IDictionary<string, SerializedPropertyInfo> Properties { get; set; }
        }

        private void InitializeTags(params string[] tags)
        {
            _testObject.SetProperty(TagsCollection.TagsPropertyName, tags);
        }

        [Fact]
        public void Tags_IsInitiallyEmpty()
        {
            _testObject.Tags.Should().BeEmpty();
            _testObject.Tags.Count.Should().Be(0);
            _testObject.Tags.Contains("x").Should().BeFalse();
        }

        [Fact]
        public void Tags_Add_AddsTag()
        {
            bool wasAdded = _testObject.Tags.Add("x");

            wasAdded.Should().BeTrue();
            _testObject.Tags.Count.Should().Be(1);
            _testObject.Tags.Contains("x").Should().BeTrue();
        }

        [Fact]
        public void Tags_Add_AddsTagsOnlyOnce()
        {
            InitializeTags("x");

            bool wasAdded = _testObject.Tags.Add("x");

            wasAdded.Should().BeFalse();
            _testObject.Tags.Count.Should().Be(1);
            _testObject.Tags.Contains("x").Should().BeTrue();
        }

        [Fact]
        public void Tags_Add_AddsMultipleTags()
        {
            InitializeTags("x", "y");

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Contains("x").Should().BeTrue();
            _testObject.Tags.Contains("y").Should().BeTrue();
        }

        [Fact]
        public void Tags_Clear_ClearsTags()
        {
            InitializeTags("x", "y");

            _testObject.Tags.Clear();

            _testObject.Tags.Count.Should().Be(0);
            _testObject.Tags.Contains("x").Should().BeFalse();
            _testObject.Tags.Contains("y").Should().BeFalse();
        }

        [Fact]
        public void Tags_Clear_WorksOnEmptyTags()
        {
            _testObject.Tags.Clear();

            _testObject.Tags.Count.Should().Be(0);
        }

        [Fact]
        public void Tags_CopyTo_CopiesToArray()
        {
            InitializeTags("x", "y");
            string[] array = new string[] { "a", "b", "c", "d" };

            _testObject.Tags.CopyTo(array, 1);

            array.Should().ContainInOrder("a", "x", "y", "d");
        }

        [Fact]
        public void Tags_CopyTo_WorksOnEmptyTags()
        {
            string[] array = new string[] { "a", "b", "c", "d" };

            _testObject.Tags.CopyTo(array, 1);

            array.Should().ContainInOrder("a", "b", "c", "d");
        }

        [Fact]
        public void Tags_ExceptWith_RemovesSpecifiedElements()
        {
            InitializeTags("a", "b", "c", "d");

            _testObject.Tags.ExceptWith(new[] { "b", "c", "e" });

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Should().ContainInOrder("a", "d");
        }

        [Fact]
        public void Tags_ExceptWith_WorksWithEmptyTags()
        {
            _testObject.Tags.ExceptWith(new[] { "b", "c", "e" });

            _testObject.Tags.Count.Should().Be(0);
        }

        [Fact]
        public void Tags_IntersectWith_ReturnsCommonElements()
        {
            InitializeTags("a", "b", "c", "d");

            _testObject.Tags.IntersectWith(new[] { "b", "c", "e" });

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Should().ContainInOrder("b", "c");
        }

        [Fact]
        public void Tags_IntersectWith_WorksWithEmptyTags()
        {
            _testObject.Tags.IntersectWith(new[] { "b", "c", "e" });

            _testObject.Tags.Count.Should().Be(0);
        }

        [Fact]
        public void Tags_IsProperSubsetOf_ReturnsFalseWhenBothAreEmpty()
        {
            _testObject.Tags.IsProperSubsetOf(new string[0]).Should().BeFalse();
        }

        [Fact]
        public void Tags_IsProperSubsetOf_ReturnsTrueWhenEmptyAndOtherIsNonEmpty()
        {
            _testObject.Tags.IsProperSubsetOf(new[] { "x" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsProperSubsetOf_ReturnsFalseWhenHaveSameElements()
        {
            InitializeTags("x", "y");

            _testObject.Tags.IsProperSubsetOf(new[] { "x", "y" }).Should().BeFalse();
        }

        [Fact]
        public void Tags_IsProperSubsetOf_ReturnsTrueWhenNonEmptyAndProperSubsetOfOther()
        {
            InitializeTags("x", "y");

            _testObject.Tags.IsProperSubsetOf(new[] { "z", "y", "x" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsProperSubsetOf_ReturnsFalseWhenEachSideHasSomeDifferentElements()
        {
            InitializeTags("x", "y", "z");

            _testObject.Tags.IsProperSubsetOf(new[] { "y", "z", "q" }).Should().BeFalse();
        }

        [Fact]
        public void Tags_IsProperSupersetOf_ReturnsFalseWhenBothAreEmpty()
        {
            _testObject.Tags.IsProperSupersetOf(new string[0]).Should().BeFalse();
        }

        [Fact]
        public void Tags_IsProperSupersetOf_ReturnsTrueWhenNonEmptyAndOtherIsEmpty()
        {
            InitializeTags("x");

            _testObject.Tags.IsProperSupersetOf(new string[0]).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsProperSupersetOf_ReturnsFalseWhenHaveSameElements()
        {
            InitializeTags("x", "y");

            _testObject.Tags.IsProperSupersetOf(new[] { "x", "y" }).Should().BeFalse();
        }

        [Fact]
        public void Tags_IsProperSupersetOf_ReturnsTrueWhenNonEmptyAndProperSupersetOfOther()
        {
            InitializeTags("x", "y", "z");

            _testObject.Tags.IsProperSupersetOf(new[] { "y", "x" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsProperSupersetOf_ReturnsFalseWhenEachSideHasSomeDifferentElements()
        {
            InitializeTags("x", "y", "z");

            _testObject.Tags.IsProperSupersetOf(new[] { "y", "z", "q" }).Should().BeFalse();
        }

        [Fact]
        public void Tags_IsSubsetOf_ReturnsTrueWhenBothAreEmpty()
        {
            _testObject.Tags.IsSubsetOf(new string[0]).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsSubsetOf_ReturnsTrueWhenEmptyAndOtherIsNonEmpty()
        {
            _testObject.Tags.IsSubsetOf(new[] { "x" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsSubsetOf_ReturnsTrueWhenHaveSameElements()
        {
            InitializeTags("x", "y");

            _testObject.Tags.IsSubsetOf(new[] { "x", "y" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsSubsetOf_ReturnsTrueWhenNonEmptyAndProperSubsetOfOther()
        {
            InitializeTags("x", "y");

            _testObject.Tags.IsSubsetOf(new[] { "z", "y", "x" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsSubsetOf_ReturnsFalseWhenEachSideHasSomeDifferentElements()
        {
            InitializeTags("x", "y", "z");

            _testObject.Tags.IsSubsetOf(new[] { "y", "z", "q" }).Should().BeFalse();
        }

        [Fact]
        public void Tags_IsSupersetOf_ReturnsTrueWhenBothAreEmpty()
        {
            _testObject.Tags.IsSupersetOf(new string[0]).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsSupersetOf_ReturnsTrueWhenNonEmptyAndOtherIsEmpty()
        {
            InitializeTags("x");

            _testObject.Tags.IsSupersetOf(new string[0]).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsSupersetOf_ReturnsTrueWhenHaveSameElements()
        {
            InitializeTags("x", "y");

            _testObject.Tags.IsSupersetOf(new[] { "x", "y" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsSupersetOf_ReturnsTrueWhenNonEmptyAndProperSupersetOfOther()
        {
            InitializeTags("x", "y", "z");

            _testObject.Tags.IsSupersetOf(new[] { "y", "x" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_IsSupersetOf_ReturnsFalseWhenEachSideHasSomeDifferentElements()
        {
            InitializeTags("x", "y", "z");

            _testObject.Tags.IsSupersetOf(new[] { "y", "z", "q" }).Should().BeFalse();
        }

        [Fact]
        public void Tags_Overlaps_ReturnsFalseWhenBothAreEmpty()
        {
            _testObject.Tags.Overlaps(new string[0]).Should().BeFalse();
        }

        [Fact]
        public void Tags_Overlaps_ReturnsFalseWhenEmptyAndOtherIsNonEmpty()
        {
            _testObject.Tags.Overlaps(new[] { "x" }).Should().BeFalse();
        }

        [Fact]
        public void Tags_Overlaps_ReturnsFalseWhenNonEmptyAndOtherIsEmpty()
        {
            InitializeTags("x");

            _testObject.Tags.Overlaps(new string[0]).Should().BeFalse();
        }

        [Fact]
        public void Tags_Overlaps_ReturnsFalseWhenDisjoint()
        {
            InitializeTags("x", "y");

            _testObject.Tags.Overlaps(new[] { "a", "b" }).Should().BeFalse();
        }

        [Fact]
        public void Tags_Overlaps_ReturnsTrueWhenNonDisjoint()
        {
            InitializeTags("x", "y", "q");

            _testObject.Tags.Overlaps(new[] { "a", "b", "q" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_Remove_RemovesSpecifiedItem()
        {
            InitializeTags("x", "y");

            bool wasRemoved = _testObject.Tags.Remove("x");

            wasRemoved.Should().BeTrue();
            _testObject.Tags.Count.Should().Be(1);
            _testObject.Tags.Should().Contain("y");
        }

        [Fact]
        public void Tags_Remove_WorksWhenItemIsNotPresent()
        {
            InitializeTags("y");

            bool wasRemoved = _testObject.Tags.Remove("x");

            wasRemoved.Should().BeFalse();
            _testObject.Tags.Count.Should().Be(1);
            _testObject.Tags.Should().Contain("y");
        }

        [Fact]
        public void Tags_Remove_WorksOnEmptyTags()
        {
            bool wasRemoved = _testObject.Tags.Remove("x");

            wasRemoved.Should().BeFalse();
            _testObject.Tags.Should().BeEmpty();
        }

        [Fact]
        public void Tags_SetEquals_ReturnsTrueWhenBothAreEmpty()
        {
            _testObject.Tags.SetEquals(new string[0]).Should().BeTrue();
        }

        [Fact]
        public void Tags_SetEquals_ReturnsFalseWhenEmptyAndOtherIsNonEmpty()
        {
            _testObject.Tags.SetEquals(new[] { "x" }).Should().BeFalse();
        }

        [Fact]
        public void Tags_SetEqualsReturnsFalseWhenNonEmptyAndOtherIsEmpty()
        {
            InitializeTags("x");

            _testObject.Tags.SetEquals(new string[0]).Should().BeFalse();
        }

        [Fact]
        public void Tags_SetEquals_ReturnsTrueWhenHaveSameElements()
        {
            InitializeTags("x", "y");

            _testObject.Tags.SetEquals(new[] { "x", "y" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_SetEquals_ReturnsTrueWhenHaveSameElementsRegardlessOfOrder()
        {
            InitializeTags("x", "y");

            _testObject.Tags.SetEquals(new[] { "y", "x" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_SetEquals_ReturnsTrueWhenElementsAreRepeated()
        {
            InitializeTags("x", "y");

            _testObject.Tags.SetEquals(new[] { "y", "x", "x", "y", "y" }).Should().BeTrue();
        }

        [Fact]
        public void Tags_SetEquals_ReturnsTrueWhenHaveDifferentElements()
        {
            InitializeTags("x", "y");

            _testObject.Tags.SetEquals(new[] { "x", "z" }).Should().BeFalse();
        }

        [Fact]
        public void Tags_SymmetricExceptWith_EmptyWhenBothAreEmpty()
        {
            _testObject.Tags.SymmetricExceptWith(new string[0]);

            _testObject.Tags.Should().BeEmpty();
        }

        [Fact]
        public void Tags_SymmetricExceptWith_OriginalElementsWhenOtherIsEmpty()
        {
            InitializeTags("x", "y");

            _testObject.Tags.SymmetricExceptWith(new string[0]);

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Should().ContainInOrder("x", "y");
        }

        [Fact]
        public void Tags_SymmetricExceptWith_OtherElementsWhenEmpty()
        {
            _testObject.Tags.SymmetricExceptWith(new[] { "x", "y" });

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Should().ContainInOrder("x", "y");
        }

        [Fact]
        public void Tags_SymmetricExceptWith_ExcludesCommonElements()
        {
            InitializeTags("x", "y");

            _testObject.Tags.SymmetricExceptWith(new[] { "z", "y" });

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Should().ContainInOrder("x", "z");
        }

        [Fact]
        public void Tags_UnionWith_EmptyWhenBothAreEmpty()
        {
            _testObject.Tags.UnionWith(new string[0]);

            _testObject.Tags.Count.Should().Be(0);
        }

        [Fact]
        public void Tags_UnionWith_OriginalElementsWhenOtherIsEmpty()
        {
            InitializeTags("x", "y");

            _testObject.Tags.UnionWith(new string[0]);

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Should().ContainInOrder("x", "y");
        }

        [Fact]
        public void Tags_UnionWith_OtherElementsWhenEmpty()
        {
            _testObject.Tags.UnionWith(new[] { "x", "y" });

            _testObject.Tags.Count.Should().Be(2);
            _testObject.Tags.Should().ContainInOrder("x", "y");
        }

        [Fact]
        public void Tags_UnionWith_CombinationOfElementsWhenBothAreNonEmpty()
        {
            InitializeTags("a", "b", "c", "d");

            _testObject.Tags.UnionWith(new[] { "b", "c", "e", "f" });

            _testObject.Tags.Count.Should().Be(6);
            _testObject.Tags.Should().ContainInOrder("a", "b", "c", "d", "e", "f");
        }
    }
}
