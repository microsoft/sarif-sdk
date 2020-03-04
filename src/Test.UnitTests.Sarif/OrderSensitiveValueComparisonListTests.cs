// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.UnitTests
{
    /// <summary>
    /// A comparer, analagous to the internal .NET ObjectEqualityComparer, that
    /// provides comparison semantics roughly equivalent to the default .NET
    /// behavior provided by Object.Equals and friends (this functionality is
    /// what's invoked when calling List.Contains). Our comparer only exists
    /// for testing, because ObjectEqualityComparer is an internal type. We
    /// don't cover all possible cases to make the types equivalent; the 
    /// implementation covers enough for core validation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DefaultObjectComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return object.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            if (obj == null) { return 0; }
            return obj.GetHashCode();
        }
    }

    public class OrderSensitiveValueComparisonListTests
    {
        [Fact]
        public void OrderSensitiveValueComparisonListTests_DefaultObjectComparer()
        {
            var equalityComparer = new DefaultObjectComparer<ArtifactChange>();

            OrderSensitiveValueComparisonList<ArtifactChange> listOne = CreateTestList(equalityComparer);

            // Populate the second list with references from the first.
            var listTwo = new OrderSensitiveValueComparisonList<ArtifactChange>(equalityComparer);
            for (int i = 0; i < listOne.Count; i++)
            {
                listTwo.Add(listOne[i]);
            }

            // Every list s/be equal to itself.
            listOne.Equals(listOne).Should().Be(true);

            // Two lists with shared objects, by reference, in the same
            // order should be regarded as equivalent.
            listOne.Equals(listTwo).Should().Be(true);

            ArtifactChange toSwap = listTwo[0];
            listTwo[0] = listTwo[1];
            listTwo[1] = toSwap;

            // We have reordered two objects that are themselves identical.
            // The comparison should fail as the order of references changed.
            listOne.Equals(listTwo).Should().Be(false);
        }

        [Fact]
        public void OrderSensitiveValueComparisonListTests_ValueComparer()
        {
            // Two identical lists with elements that are 
            // distinct objects, by reference.
            OrderSensitiveValueComparisonList<ArtifactChange> listOne = CreateTestList(ArtifactChange.ValueComparer);
            OrderSensitiveValueComparisonList<ArtifactChange> listTwo = CreateTestList(ArtifactChange.ValueComparer);

            // Every list s/be equal to itself
            listOne.Equals(listOne).Should().Be(true);

            // As initialized, these objects are different, due
            // to a unique GUID property on each list
            listOne.Equals(listTwo).Should().Be(false);

            // Make the two lists equivalent, by value
            listTwo[2].SetProperty(DIFFERENTIATING_PROPERTY_NAME, listOne[2].GetProperty<Guid>(DIFFERENTIATING_PROPERTY_NAME));
            listOne.Equals(listTwo).Should().Be(true);

            ArtifactChange toSwap = listTwo[0];
            listTwo[0] = listTwo[1];
            listTwo[1] = toSwap;

            // We have reordered two objects that are themselves identical.
            // by value. The comparison should still succeed.
            listOne.Equals(listTwo).Should().Be(true);
        }

        private const string DIFFERENTIATING_PROPERTY_NAME = nameof(DIFFERENTIATING_PROPERTY_NAME);

        private OrderSensitiveValueComparisonList<ArtifactChange> CreateTestList(IEqualityComparer<ArtifactChange> equalityComparer)
        {
            // Test list. First two elements are identical. The third element is unique.
            var fileChangeOne = new ArtifactChange();
            var fileChangeTwo = new ArtifactChange();

            var fileChangeThree = new ArtifactChange();
            Guid differentiatingProperty = Guid.NewGuid();
            fileChangeThree.SetProperty(DIFFERENTIATING_PROPERTY_NAME, differentiatingProperty);

            var list = new OrderSensitiveValueComparisonList<ArtifactChange>(equalityComparer);
            list.AddRange(new[] { fileChangeOne, fileChangeTwo, fileChangeThree });

            return list;
        }
    }
}