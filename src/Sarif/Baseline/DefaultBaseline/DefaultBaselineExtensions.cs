using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Baseline.DefaultBaseline
{
    public static class DefaultBaselineExtensions
    {
        public static int GetNullCheckedHashCode(this object obj)
        {
            if(obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }
    }
}
