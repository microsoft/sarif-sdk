// Copyright (c) Microsoft Corporation.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.CodeAnalysis.Test.Utilities.Sarif
{
    internal static class TestData
    {
        public const string TestRuleId = "TST0001";
        public const string TestMessageStringId = "testMessageStringId";
        public const string TestAnalysisTarget = @"C:\dir\file";

        public static readonly ReportingDescriptor TestRule = new ReportingDescriptor
        {
            Id = TestRuleId,
            Name = "ThisIsATest",
            ShortDescription = new MultiformatMessageString { Text = "short description" },
            FullDescription = new MultiformatMessageString { Text = "full description" },
            MessageStrings = new Dictionary<string, MultiformatMessageString>
            {
                [TestMessageStringId] = new MultiformatMessageString { Text = "First: {0}, Second: {1}" }
            }
        };

        internal static Result CreateResult(FailureLevel level, ResultKind kind, Region region, string path)
        {
            return new Result
            {
                RuleId = TestRuleId,
                Level = level,
                Kind = kind,
                Locations = new List<Location>
                {
                    new Location
                    {
                        PhysicalLocation = new PhysicalLocation
                        {
                            ArtifactLocation = new ArtifactLocation
                            {
                                Uri = new Uri(path, UriKind.RelativeOrAbsolute)
                            },
                            Region = region
                        }
                    }
                },
                Message = new Message
                {
                    Arguments = new List<string>
                    {
                        "42",
                        "54"
                    },
                    Id = TestMessageStringId
                }
            };
        }
    }
}
