// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    internal class CodeFlowLocationBaselineEquals : IEqualityComparer<CodeFlowLocation>
    {
        internal static readonly CodeFlowLocationBaselineEquals DefaultInstance = new CodeFlowLocationBaselineEquals();
        
        public bool Equals(CodeFlowLocation x, CodeFlowLocation y)
        {
            if (!object.ReferenceEquals(x, y))
            {
                if (x.Importance != y.Importance)
                {
                    return false;
                }

                if (x.Module != y.Module)
                {
                    return false;
                }

                if (!LocationBaselineEquals.Instance.Equals(x.Location, y.Location))
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(CodeFlowLocation obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return 0;
            }
            else
            {
                int hs = 0;
                
                hs = hs ^ obj.NestingLevel ^ LocationBaselineEquals.Instance.GetHashCode(obj.Location);

                return hs;
            }
        }
    }
}