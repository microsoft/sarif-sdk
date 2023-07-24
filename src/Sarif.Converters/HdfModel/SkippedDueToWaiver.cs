// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif.Converters.HdfModel
{
    public partial struct SkippedDueToWaiver
    {
        public bool? Bool;
        public string String;

        public static implicit operator SkippedDueToWaiver(bool Bool)
        {
            return new SkippedDueToWaiver { Bool = Bool };
        }

        public static implicit operator SkippedDueToWaiver(string String)
        {
            return new SkippedDueToWaiver { String = String };
        }
    }
}
