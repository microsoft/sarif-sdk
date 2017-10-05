// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ToolFormatTests
    {
        private static List<FieldInfo> publicFields;

        public ToolFormatTests()
        {
            publicFields = Utilities.GetToolFormatFields();
        }

        [Fact]
        public void ToolFormat_ContainsNoCaseInsensitiveDuplicateValues()
        {
            var alreadySeen = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var duplicates = new List<string>();

            foreach (FieldInfo field in publicFields)
            {
                string value = field.GetRawConstantValue() as string;
                if (alreadySeen.Contains(value))
                {
                    duplicates.Add(value);
                }
                else
                {
                    alreadySeen.Add(value);
                }
            }

            Assert.True(0 == duplicates.Count, "Duplicate ToolFormat strings: " + string.Join(", ", duplicates));
        }

        [Fact]
        public void ToolFormat_MemberNamesMatchStringValues()
        {
            var mismatches = new List<string>();

            foreach (FieldInfo field in publicFields)
            {
                string value = field.GetRawConstantValue() as string;
                if (!value.Equals(field.Name, StringComparison.Ordinal))
                {
                    mismatches.Add(field.Name);
                }
            }

            Assert.True(0 == mismatches.Count, "ToolFormat fields with mismatched values: " + string.Join(", ", mismatches));
        }
    }
}
