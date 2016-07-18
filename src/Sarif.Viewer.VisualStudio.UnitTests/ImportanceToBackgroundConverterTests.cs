// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Converters;
using Microsoft.Sarif.Viewer.Models;
using Xunit;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class ImportanceToBackgroundConverterTests
    {
        [Fact]
        public void ImportanceToBackgroundConverter_HandlesUnimportant()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Call,
                    Callee = "my_function",
                    Importance = AnnotatedCodeLocationImportance.Unimportant,
                    PhysicalLocation = new PhysicalLocation
                    {
                        Region = new CodeAnalysis.Sarif.Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, Color.White);
        }

        [Fact]
        public void ImportanceToBackgroundConverter_HandlesImportant()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Call,
                    Callee = "my_function",
                    Importance = AnnotatedCodeLocationImportance.Important,
                    PhysicalLocation = new PhysicalLocation
                    {
                        Region = new CodeAnalysis.Sarif.Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, Color.Yellow);
        }

        [Fact]
        public void ImportanceToBackgroundConverter_HandlesEssential()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Call,
                    Callee = "my_function",
                    Importance = AnnotatedCodeLocationImportance.Essential,
                    PhysicalLocation = new PhysicalLocation
                    {
                        Region = new CodeAnalysis.Sarif.Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, Color.White);
        }

        [Fact]
        public void ImportanceToBackgroundConverter_HandlesDefault()
        {
            var callTreeNode = new CallTreeNode
            {
                Location = new AnnotatedCodeLocation
                {
                    Kind = AnnotatedCodeLocationKind.Call,
                    Callee = "my_function",
                    PhysicalLocation = new PhysicalLocation
                    {
                        Region = new CodeAnalysis.Sarif.Region
                        {
                            StartLine = 42
                        }
                    }
                }
            };

            VerifyConversion(callTreeNode, Color.White);
        }

        private static void VerifyConversion(CallTreeNode callTreeNode, Color expectedColor)
        {
            var converter = new ImportanceToForegroundConverter();

            Color color = (Color)converter.Convert(callTreeNode, typeof(Color), null, CultureInfo.CurrentCulture);

            color.Should().Be(expectedColor);
        }

    }
}
