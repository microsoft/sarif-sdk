// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    public class RemoveOptionalDataVisitorTests
    {
        private SarifLog SampleLog = new SarifLog()
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
            SarifLog log = SampleLog.DeepClone();

            RemoveOptionalDataVisitor v = new RemoveOptionalDataVisitor(OptionallyEmittedData.None);
            v.Visit(log);
            Assert.NotNull(log.Runs[0].Results[0].Guid);

            v = new RemoveOptionalDataVisitor(OptionallyEmittedData.Guids);
            v.Visit(log);
            Assert.Null(log.Runs[0].Results[0].Guid);
        }
    }
}