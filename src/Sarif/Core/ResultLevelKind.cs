// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.CodeAnalysis.Sarif
{
    public class ResultLevelKind
    {
        private ResultKind? resultKind;

        public ResultKind Kind
        {
            get
            {
                // If kind is absent, it SHALL default to "fail".
                if (!resultKind.HasValue)
                {
                    return ResultKind.Fail;
                }

                // If level has any value other than "none" and kind is present, then kind SHALL have the value "fail".
                if (Level != FailureLevel.None)
                {
                    return ResultKind.Fail;
                }

                return resultKind.Value;
            }
            set { resultKind = value; }
        }

        public FailureLevel Level { get; set; }
    }
}
