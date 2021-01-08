// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using FluentAssertions;

using Microsoft.CodeAnalysis.Sarif;

using Xunit;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Core
{
    public class ReportingDescriptorTests
    {
        [Fact]
        public void ShouldSerializeShortDescription_CorrectlyHandlesNullAndEmptyValues()
        {
            ReportingDescriptor reportingDescriptor = new ReportingDescriptor()
            {
                ShortDescription = null,
                FullDescription = null
            };

            //  both null
            Assert.False(reportingDescriptor.ShouldSerializeShortDescription());

            reportingDescriptor.ShortDescription = new MultiformatMessageString() { Text = "" };

            //  short empty, full null
            Assert.False(reportingDescriptor.ShouldSerializeShortDescription());

            reportingDescriptor.FullDescription = new MultiformatMessageString() { Text = "" };

            //  short empty, full empty
            Assert.False(reportingDescriptor.ShouldSerializeShortDescription());

            reportingDescriptor.ShortDescription = null;

            //  short null, full empty
            Assert.False(reportingDescriptor.ShouldSerializeShortDescription());

            reportingDescriptor.FullDescription = new MultiformatMessageString() { Text = " " };

            //  short null, full space
            Assert.False(reportingDescriptor.ShouldSerializeShortDescription());

            reportingDescriptor.ShortDescription = new MultiformatMessageString() { Text = " " };

            //  short space, full space
            Assert.False(reportingDescriptor.ShouldSerializeShortDescription());

            reportingDescriptor.FullDescription = null;

            //  short space, full null
            Assert.False(reportingDescriptor.ShouldSerializeShortDescription());
        }

        [Fact]
        public void ShouldSerializeShortDescription_FalseForIdenticalStrings()
        {
            ReportingDescriptor reportingDescriptor = new ReportingDescriptor()
            {
                ShortDescription = new MultiformatMessageString() { Text = "EasyFalse1" },
                FullDescription = new MultiformatMessageString() { Text = "EasyFalse1" }
            };

            Assert.False(reportingDescriptor.ShouldSerializeShortDescription());

            reportingDescriptor.ShortDescription = new MultiformatMessageString() { Text = "EasyTrue1" };
            reportingDescriptor.FullDescription = new MultiformatMessageString() { Text = "EasyTrue1.  Lorem ipsum etc" };

            Assert.True(reportingDescriptor.ShouldSerializeShortDescription());

            reportingDescriptor.ShortDescription = new MultiformatMessageString() { Text = "CASESENSITIVITY1" };
            reportingDescriptor.FullDescription = new MultiformatMessageString() { Text = "casesensitivity1" };

            Assert.False(reportingDescriptor.ShouldSerializeShortDescription());

            reportingDescriptor.ShortDescription = new MultiformatMessageString() { Text = "    extraspace1" };
            reportingDescriptor.FullDescription = new MultiformatMessageString() { Text = "extraspace1      \t" + Environment.NewLine };

            Assert.False(reportingDescriptor.ShouldSerializeShortDescription());
        }

        //  TODO: Add unit tests for remaining ShouldSerialize methods
    }
}
