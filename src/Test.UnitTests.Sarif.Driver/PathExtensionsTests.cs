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

        [Theory]
        [MemberData(nameof(TestData))]
        [Trait(TestTraits.WindowsOnly, "true")]
        public void ReplaceInvalidCharInFileName_ShouldCorrectFilePath(string fileName, string replacement, string expectFileName)
        {
            VerifyReplaceInvalidCharInFileName(fileName, replacement, expectFileName);
        }

        [Theory]
        [ClassData(typeof(ComprehensiveTestDataGenerator))]
        public void ReplaceInvalidCharInFileName_ShouldCorrectFilePath_Comprehensive(string fileName, string replacement, string expectFileName)
        {
            VerifyReplaceInvalidCharInFileName(fileName, replacement, expectFileName);
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

        public static IEnumerable<object[]> TestData()
        {
            // fileName | replacement | expectedFileName
            yield return new object[] { null, ".", null };
            yield return new object[] { "file.cpp", null, null };
            yield return new object[] { "cppfile", "?", null };
            yield return new object[] { "", ".", "" };
            yield return new object[] { "cppfile", ".", "cppfile" };
            yield return new object[] { "file/cpp", "_", "file_cpp" };
            yield return new object[] { "?file*test*cpp?", "+", "+file+test+cpp+" };
            yield return new object[] { "?file*test*cpp?", "", "filetestcpp" };
        }

        internal class ComprehensiveTestDataGenerator : IEnumerable<object[]>
        {
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IEnumerator<object[]> GetEnumerator()
            {
                char[] invalidChars = Path.GetInvalidFileNameChars();
                char[] validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._-+".ToCharArray();
                char replacement = '_';
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

                    yield return new object[] { fileNameWithInvalidChar, replacement, replacedFileName };
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

            private static void ConstructRandomFileName(char[] invalidSet, char[] validSet, char replacement, out string invalidFileName, out string replacedFileName)
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
