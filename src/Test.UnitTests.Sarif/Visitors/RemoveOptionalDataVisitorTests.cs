// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class RemoveOptionalDataVisitorTests
    {
        private readonly SarifLog _sampleLog = new SarifLog()
        {
            Runs = new List<Run>()
            {
                new Run()
                {
                    Results = new List<Result>()
                    {
                        new Result()
                        {
                            Guid = Guid.NewGuid().ToString(SarifConstants.GuidFormat)
                        }
                    }
                }
            }
        };

        [Fact]
        public void RemoveGuids()
        {
            SarifLog log = _sampleLog.DeepClone();

            RemoveOptionalDataVisitor v = new RemoveOptionalDataVisitor(OptionallyEmittedData.None);
            v.Visit(log);
            log.Runs[0].Results[0].Guid.Should().NotBeNull();

            v = new RemoveOptionalDataVisitor(OptionallyEmittedData.Guids);
            v.Visit(log);
            log.Runs[0].Results[0].Guid.Should().BeNull();
        }
    }
}