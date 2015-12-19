// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif.Sdk;

namespace Microsoft.CodeAnalysis.Sarif.Driver.Sdk
{
    public static class NoteDescriptors
    {
        private const string MSG1001 = "MSG1001";
        private const string MSG1002 = "MSG1002";

        public static IRuleDescriptor GeneralMessage = new RuleDescriptor()
        {
            // A file is being analyzed.
            Id = MSG1001,
            Name = nameof(GeneralMessage),
            FullDescription = SdkResources.MSG1001_GeneralMessage_Description,
            FormatSpecifiers = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.MSG1001_Default),
                    nameof(SdkResources.MSG1001_AnalyzingTarget),
                }, MSG1001)
        };

        public static IRuleDescriptor InvalidTarget = new RuleDescriptor()
        {
            // A file was skipped as it does not appear to be a valid target for analysis.
            Id = MSG1002,
            Name = nameof(InvalidTarget),
            FullDescription = SdkResources.MSG1002_InvalidTarget_Description,
            FormatSpecifiers = RuleUtilities.BuildDictionary(SdkResources.ResourceManager,
                new string[] {
                    nameof(SdkResources.MSG1002_InvalidFileType),
                    nameof(SdkResources.MSG1002_InvalidMetadata)
                }, MSG1002)
        };
    }
}