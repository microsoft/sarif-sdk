// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class EqualityComparerTests
    {
        [Fact]
        public void EqualityComparer_ComputesTheSameHashCodeForDistinctEquivalentObjects()
        {
            var result1 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = new Message { Text = "Same" }
                    }
                }
            };

            var result2 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = new Message { Text = "Same" }
                    }
                }
            };

            Assert.NotSame(result1, result2);

            Assert.Equal(
                Result.ValueComparer.GetHashCode(result1),
                Result.ValueComparer.GetHashCode(result2));
        }

        [Fact]
        public void EqualityComparer_ComputesDifferentHashCodesForDistinctNonEquivalentObjects()
        {
            var result1 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = new Message { Text = "Same" }
                    }
                }
            };

            var result2 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = new Message { Text = "Different" }
                    }
                }
            };

            Assert.NotSame(result1, result2);

            Assert.NotEqual(
                Result.ValueComparer.GetHashCode(result1),
                Result.ValueComparer.GetHashCode(result2));
        }

        [Fact]
        public void EqualityComparer_DecidesThatDistinctEquivalentObjectsAreEqual()
        {
            var result1 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = new Message { Text = "Same" }
                    }
                }
            };

            var result2 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = new Message { Text = "Same" }
                    }
                }
            };

            Assert.NotSame(result1, result2);
            Assert.NotEqual(result1, result2);
            Assert.True(result1.ValueEquals(result2));
        }

        [Fact]
        public void EqualityComparer_DecidesThatDistinctNonEquivalentObjectsAreNotEqual()
        {
            var result1 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = new Message { Text = "Same" }
                    }
                }
            };

            var result2 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = new Message { Text = "Different" }
                    }
                }
            };

            Assert.NotSame(result1, result2);
            Assert.NotEqual(result1, result2);
            Assert.False(result1.ValueEquals(result2));
        }
    }
}
