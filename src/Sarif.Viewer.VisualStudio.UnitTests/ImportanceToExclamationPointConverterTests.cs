// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Converters;
using Microsoft.Sarif.Viewer.Models;
using System.Drawing;
using System.Globalization;
using Xunit;
using System.Windows;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public class ImportanceToExclamationPointConverterTests
    {
        [Fact]
        public void ImportanceToExclamationPointConverter_HandlesUnimportant()
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

            VerifyConversion(callTreeNode, Visibility.Collapsed);
        }

        [Fact]
        public void ImportanceToExclamationPointConverter_HandlesImportant()
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

            VerifyConversion(callTreeNode, Visibility.Collapsed);
        }

        [Fact]
        public void ImportanceToExclamationPointConverter_HandlesEssential()
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

            VerifyConversion(callTreeNode, Visibility.Visible);
        }

        [Fact]
        public void ImportanceToExclamationPointConverter_HandlesDefault()
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

            VerifyConversion(callTreeNode, Visibility.Collapsed);
        }

        private static void VerifyConversion(CallTreeNode callTreeNode, Visibility expectedVisibility)
        {
            var converter = new ImportanceToExclamationPointConverter();

            Visibility visibility = (Visibility)converter.Convert(callTreeNode, typeof(Visibility), null, CultureInfo.CurrentCulture);

            visibility.Should().Be(expectedVisibility);
        }

    }
}
