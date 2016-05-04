// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class EqualityComparerTests
    {
        [TestMethod]
        public void EqualityComparer_ComputesTheSameHashCodeForDistinctEquivalentObjects()
        {
            var result1 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = "Same"
                    }
                }
            };

            var result2 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = "Same"
                    }
                }
            };

            Assert.AreNotSame(result1, result2);

            Assert.AreEqual(
                Result.ValueComparer.GetHashCode(result1),
                Result.ValueComparer.GetHashCode(result2));
        }

        [TestMethod]
        public void EqualityComparer_ComputesDifferentHashCodesForDistinctNonEquivalentObjects()
        {
            var result1 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = "Same"
                    }
                }
            };

            var result2 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = "Different"
                    }
                }
            };

            Assert.AreNotSame(result1, result2);

            Assert.AreNotEqual(
                Result.ValueComparer.GetHashCode(result1),
                Result.ValueComparer.GetHashCode(result2));
        }

        [TestMethod]
        public void EqualityComparer_DecidesThatDistinctEquivalentObjectsAreEqual()
        {
            var result1 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = "Same"
                    }
                }
            };

            var result2 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = "Same"
                    }
                }
            };

            Assert.AreNotSame(result1, result2);
            Assert.AreNotEqual(result1, result2);
            Assert.IsTrue(result1.ValueEquals(result2));
        }

        [TestMethod]
        public void EqualityComparer_DecidesThatDistinctNonEquivalentObjectsAreNotEqual()
        {
            var result1 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = "Same"
                    }
                }
            };

            var result2 = new Result
            {
                Stacks = new[]
                {
                    new Stack
                    {
                        Message = "Different"
                    }
                }
            };

            Assert.AreNotSame(result1, result2);
            Assert.AreNotEqual(result1, result2);
            Assert.IsFalse(result1.ValueEquals(result2));
        }
    }
}
