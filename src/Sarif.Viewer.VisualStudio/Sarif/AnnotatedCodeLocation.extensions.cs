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
    static class AnnotatedCodeLocationExtensions
    {
        public static Microsoft.Sarif.Viewer.Models.AnnotatedCodeLocationModel ToAnnotatedCodeLocationModel(this AnnotatedCodeLocation location)
        {
            AnnotatedCodeLocationModel model = new AnnotatedCodeLocationModel();

            if (location.PhysicalLocation != null)
            {
                model.Region = location.PhysicalLocation.Region;

                if (location.PhysicalLocation.Uri != null)
                {
                    string path = location.PhysicalLocation.Uri.LocalPath;
                    if (!Path.IsPathRooted(path))
                    {
                        path = location.PhysicalLocation.Uri.AbsoluteUri;
                    }

                    model.FilePath = path;
                }
            }

            model.Message = location.Message;
            model.Kind = location.Kind.ToString();
            model.LogicalLocation = location.FullyQualifiedLogicalName;
            model.IsEssential = location.Essential;

            return model;
        }
    }
}
