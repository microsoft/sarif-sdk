// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif.Visitors
{
    internal class UnknownEncodingException : Exception
    {
        public string EncodingName { get; }

        public UnknownEncodingException(string encodingName)
        {
            EncodingName = encodingName;
        }
    }
}