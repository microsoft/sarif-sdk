using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class LocationExtensions
    {
        public static Microsoft.Sarif.Viewer.Models.AnnotatedCodeLocationModel ToAnnotatedCodeLocationModel(this Location location)
        {
            Microsoft.Sarif.Viewer.Models.AnnotatedCodeLocationModel model = new Models.AnnotatedCodeLocationModel();

            if (location.AnalysisTarget != null)
            {
                model.Region = location.AnalysisTarget.Region;

                if (location.AnalysisTarget.Uri != null)
                {
                    string path = location.AnalysisTarget.Uri.LocalPath;
                    if (!Path.IsPathRooted(path))
                    {
                        path = location.AnalysisTarget.Uri.AbsoluteUri;
                    }

                    model.FilePath = path;
                }
            }

            model.LogicalLocation = location.FullyQualifiedLogicalName;

            return model;
        }
    }
}
