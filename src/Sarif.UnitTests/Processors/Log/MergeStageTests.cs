// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT        
// license. See LICENSE file in the project root for full license information. 

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.CodeAnalysis.Sarif.Processors
{
    public class MergeStageTests
    {
        GenericReduceAction<SarifLog> Merge = (GenericReduceAction<SarifLog>)SarifLogProcessorFactory.GetActionStage(SarifLogAction.Merge);


        [Fact]
        public void MergeStage_SingleFile_ReturnedUnchanged()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void MergeStage_MultipleFiles_MergeCorrectly()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void MergeStage_SingleFileWithNoRuns_ReturnsUnchanged()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void MergeStage_MultipleFilesSomeWithNoRuns_MergeCorrectly()
        {
            throw new NotImplementedException();
        }
    }
}
