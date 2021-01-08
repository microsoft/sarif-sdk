// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using Microsoft.CodeAnalysis.Sarif;

using Newtonsoft.Json;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class ReportingDescriptorTests
    {
        private const string ShortDescription = "ShortDescription";

        [Fact]
        public void ShouldSerializeShortDescription_CorrectlyHandlesNullAndEmptyValues()
        {
            ReportingDescriptor reportingDescriptor = new ReportingDescriptor()
            {
                ShortDescription = null,
                FullDescription = null
            };

            //  both null
            AssertShouldSerializeShortDescription(reportingDescriptor);

            reportingDescriptor.ShortDescription = new MultiformatMessageString() { Text = "" };

            //  short empty, full null
            AssertShouldSerializeShortDescription(reportingDescriptor);

            reportingDescriptor.FullDescription = new MultiformatMessageString() { Text = "" };

            //  short empty, full empty
            AssertShouldSerializeShortDescription(reportingDescriptor);

            reportingDescriptor.ShortDescription = null;

            //  short null, full empty
            AssertShouldSerializeShortDescription(reportingDescriptor);

            reportingDescriptor.FullDescription = new MultiformatMessageString() { Text = " " };

            //  short null, full space
            AssertShouldSerializeShortDescription(reportingDescriptor);

            reportingDescriptor.ShortDescription = new MultiformatMessageString() { Text = " " };

            //  short space, full space
            AssertShouldSerializeShortDescription(reportingDescriptor);

            reportingDescriptor.FullDescription = null;

            //  short space, full null
            AssertShouldSerializeShortDescription(reportingDescriptor);
        }

        [Fact]
        public void ShouldSerializeShortDescription_FalseForIdenticalStrings()
        {
            ReportingDescriptor reportingDescriptor = new ReportingDescriptor()
            {
                ShortDescription = new MultiformatMessageString() { Text = "EasyFalse1" },
                FullDescription = new MultiformatMessageString() { Text = "EasyFalse1" }
            };
            AssertShouldSerializeShortDescription(reportingDescriptor);

            reportingDescriptor.ShortDescription = new MultiformatMessageString() { Text = "EasyTrue1" };
            reportingDescriptor.FullDescription = new MultiformatMessageString() { Text = "EasyTrue1.  Lorem ipsum etc" };

            AssertShouldSerializeShortDescription(reportingDescriptor, true);

            reportingDescriptor.ShortDescription = new MultiformatMessageString() { Text = "CASESENSITIVITY1" };
            reportingDescriptor.FullDescription = new MultiformatMessageString() { Text = "casesensitivity1" };

            AssertShouldSerializeShortDescription(reportingDescriptor);

            reportingDescriptor.ShortDescription = new MultiformatMessageString() { Text = "    extraspace1" };
            reportingDescriptor.FullDescription = new MultiformatMessageString() { Text = "extraspace1      \t" + Environment.NewLine };

            AssertShouldSerializeShortDescription(reportingDescriptor);
        }

        private void AssertShouldSerializeShortDescription(ReportingDescriptor reportingDescriptor, bool should = false)
        {
            Assert.False(should ^ reportingDescriptor.ShouldSerializeShortDescription());
            string testSerializedString = JsonConvert.SerializeObject(reportingDescriptor);
            Assert.False(should ^ testSerializedString.Contains(ShortDescription, StringComparison.InvariantCultureIgnoreCase));
        }

        //  TODO: Add unit tests for remaining ShouldSerialize methods
    }
}
