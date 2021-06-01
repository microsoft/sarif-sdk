// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.CodeAnalysis.Sarif.Writers;

namespace Microsoft.CodeAnalysis.Test.UnitTests.Sarif.Writers
{
    public class BaseLoggerTestConcrete : BaseLogger
    {
        public BaseLoggerTestConcrete(IList<FailureLevel> failureLevels, IList<ResultKind> resultKinds)
            : base(failureLevels, resultKinds) { }
    }
}
