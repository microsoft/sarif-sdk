// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;

namespace Microsoft.CodeAnalysis.Sarif.Core
{
    public class FileKey
    {
        public FileKey()
        {
            ParentIndex = -1;
        }

        public Uri Uri;
        public string UriBaseId;
        public int ParentIndex;
    }
}
