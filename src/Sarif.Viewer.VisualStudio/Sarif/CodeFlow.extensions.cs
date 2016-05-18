using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class CodeFlowExtensions
    {
        public static AnnotatedCodeLocationCollection ToAnnotatedCodeLocationCollection(this CodeFlow codeFlow)
        {
            if (codeFlow == null)
            {
                return null;
            }

            AnnotatedCodeLocationCollection model = new AnnotatedCodeLocationCollection(codeFlow.Message);

            if (codeFlow.Locations != null)
            {
                foreach (AnnotatedCodeLocation location in codeFlow.Locations)
                {
                    model.Add(location.ToAnnotatedCodeLocationModel());
                }
            }

            return model;
        }
    }
}
