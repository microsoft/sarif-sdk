// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Driver;

using Xunit;
using Xunit.Abstractions;

namespace Test.UnitTests.Sarif.Driver
{
    public class PathExtensionsTests
    {
        private static readonly int randomSeed = (new Random()).Next();
        private static readonly Random random = new Random(randomSeed);

        private readonly ITestOutputHelper _outputHelper;

        public PathExtensionsTests(ITestOutputHelper outputHelper)
        {
            this._outputHelper = outputHelper;
            outputHelper.WriteLine($"TestName: {nameof(PathExtensionsTests)} has random seed {randomSeed}");
        }

        [Fact]
        public void ContainsInvalidPathChar_test()
        {
            string validPath = "test.md";
            validPath.ContainsInvalidPathChar().Should().BeFalse();

            string invalidPath = "test|.md";
            invalidPath.ContainsInvalidPathChar().Should().BeTrue();
        }

        [Fact]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void ReplaceInvalidCharInFileName_ShouldCorrectFilePath()
        {
            string[][] testCases = new[]
            {
                // fileName | replacement | expectedFileName
                new string[] { null, ".", null },
                new string[] { "file.cpp", null, null },
                new string[] { "cppfile", "?", null },
                new string[] { "", ".", "" },
                new string[] { "cppfile", ".", "cppfile" },
                new string[] { "file/cpp", "_", "file_cpp" },
                new string[] { "?file*test*cpp?", "+", "+file+test+cpp+" },
                new string[] { "?file*test*cpp?", "", "filetestcpp" },
            };

            foreach (string[] test in testCases)
            {
                VerifyReplaceInvalidCharInFileName(test[0], test[1], test[2]);
            }
        }

        [Fact]
        public void ReplaceInvalidCharInFileName_ShouldCorrectFilePath_Comprehensive()
        {
            foreach (string[] test in new ComprehensiveTestDataGenerator())
            {
                VerifyReplaceInvalidCharInFileName(test[0], test[1], test[2]);
            }
        }

        private void VerifyReplaceInvalidCharInFileName(string fileName, string replacement, string expectFileName)
        {
            if (fileName == null)
            {
                Action action = () => fileName.ReplaceInvalidCharInFileName(replacement);
                action.Should().Throw<ArgumentNullException>().WithMessage($"*{nameof(fileName)}*");
                return;
            }

            if (replacement == null)
            {
                Action action = () => fileName.ReplaceInvalidCharInFileName(replacement);
                action.Should().Throw<ArgumentNullException>().WithMessage($"*{nameof(replacement)}*");
                return;
            }

            if (replacement.Any() && Path.GetInvalidFileNameChars().ToList().Contains(replacement[0]))
            {
                Action action = () => fileName.ReplaceInvalidCharInFileName(replacement);
                action.Should().Throw<ArgumentException>().WithMessage($"*{nameof(replacement)}*");
                return;
            }

            string actual = fileName.ReplaceInvalidCharInFileName(replacement);
            actual.Should().Be(expectFileName, $"file name: {fileName} | replacement: {replacement} | expected: {expectFileName} | actual: {actual}");
        }

        internal class ComprehensiveTestDataGenerator : IEnumerable<string[]>
        {
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IEnumerator<string[]> GetEnumerator()
            {
                char[] invalidChars = Path.GetInvalidFileNameChars();
                char[] validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._-+".ToCharArray();
                string replacement = "_";
                var hitmap = invalidChars.ToHashSet();

                while (hitmap.Count > 0)
                {
                    char[] invalidCharSet = PickupRandomChars(invalidChars, random.Next(1, 4));
                    char[] validCharSet = PickupRandomChars(validChars, random.Next(4, 10));

                    ConstructRandomFileName(
                        invalidCharSet,
                        validCharSet,
                        replacement,
                        out string fileNameWithInvalidChar,
                        out string replacedFileName);

                    RemoveItems(hitmap, invalidCharSet);

                    yield return new string[] { fileNameWithInvalidChar, replacement, replacedFileName };
                }
            }

            private static char[] PickupRandomChars(char[] source, int numberOfChar)
            {
                char[] result = new char[numberOfChar];
                for (int i = 0; i < numberOfChar; i++)
                {
                    result[i] = source[random.Next(0, source.Length)];
                }

                return result;
            }

            private static void ConstructRandomFileName(char[] invalidSet, char[] validSet, string replacement, out string invalidFileName, out string replacedFileName)
            {
                int length = invalidSet.Length + validSet.Length;
                var invalidStr = new StringBuilder(length);
                var replacedStr = new StringBuilder(length);
                int invalidSetIndex = 0;
                int validSetIndex = 0;

                for (int i = 0; i < length; i++)
                {
                    if (invalidSetIndex < invalidSet.Length && validSetIndex < validSet.Length)
                    {
                        int choice = random.Next(0, 2);
                        if (choice == 0)
                        {
                            invalidStr.Append(invalidSet[invalidSetIndex++]);
                            replacedStr.Append(replacement);
                        }
                        else
                        {
                            char validChar = validSet[validSetIndex++];
                            invalidStr.Append(validChar);
                            replacedStr.Append(validChar);
                        }
                    }
                    else if (invalidSetIndex < invalidSet.Length)
                    {
                        invalidStr.Append(invalidSet[invalidSetIndex++]);
                        replacedStr.Append(replacement);
                    }
                    else
                    {
                        char validChar = validSet[validSetIndex++];
                        invalidStr.Append(validChar);
                        replacedStr.Append(validChar);
                    }
                }

                invalidFileName = invalidStr.ToString();
                replacedFileName = replacedStr.ToString();
            }

            private static void RemoveItems(HashSet<char> set, char[] items)
            {
                foreach (char item in items)
                {
                    set.Remove(item);
                }
            }
        }
    }
}
