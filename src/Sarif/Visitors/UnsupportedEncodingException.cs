// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    internal class UnsupportedEncodingException : Exception
    {
        public string EncodingName { get; private set; }

        public UnsupportedEncodingException(string encodingName)
        {
            EncodingName = encodingName;
        }
    }
}