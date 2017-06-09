// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.CodeAnalysis.Sarif
{
    [TestClass]
    public class ToolFormatTests
    {
        private static List<FieldInfo> publicFields;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            publicFields = Utilities.GetToolFormatFields();
        }

        [TestMethod]
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

            Assert.AreEqual(0, duplicates.Count, "Duplicate ToolFormat strings: " + string.Join(", ", duplicates));
        }

        [TestMethod]
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

            Assert.AreEqual(0, mismatches.Count, "ToolFormat fields with mismatched values: " + string.Join(", ", mismatches));
        }
    }
}
