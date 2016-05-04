using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer
{
    internal static class ExtensionMethods
    {
        public static bool Include(this Region region, Region location)
        {
            int currentEndLine = region.EndLine == -1 ? region.StartLine: region.EndLine; ;
            int locationEndLine = location.EndLine == -1 ? location.StartLine : location.EndLine; ;

            int currentEndColumn = region.EndColumn == -1 ? region.StartColumn : region.EndColumn; ;
            int locationEndColumn = location.EndColumn == -1 ? location.EndColumn : location.EndColumn; ;

            return region.StartLine <= location.StartLine
                && currentEndLine >= locationEndLine
                && region.StartColumn <= location.StartColumn
                && currentEndColumn >= locationEndColumn;
        }
    }
}
