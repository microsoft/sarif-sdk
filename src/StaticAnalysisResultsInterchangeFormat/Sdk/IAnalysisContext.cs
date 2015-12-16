// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.using System;

using System;

namespace Microsoft.CodeAnalysis.Sarif.Sdk
{
    public interface IAnalysisContext
    {
        Uri Uri { get;  }

        IRuleDescriptor Rule { get;  }       
    }
}
