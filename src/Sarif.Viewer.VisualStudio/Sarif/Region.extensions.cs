using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class RegionExtensions
    {
        public static bool Include(this Region region, Region location)
        {
            int currentEndLine = region.EndLine == -1 ? region.StartLine : region.EndLine; ;
            int locationEndLine = location.EndLine == -1 ? location.StartLine : location.EndLine; ;

            int currentEndColumn = region.EndColumn == -1 ? region.StartColumn : region.EndColumn; ;
            int locationEndColumn = location.EndColumn == -1 ? location.EndColumn : location.EndColumn; ;

            return region.StartLine <= location.StartLine
                && currentEndLine >= locationEndLine
                && region.StartColumn <= location.StartColumn
                && currentEndColumn >= locationEndColumn;
        }

        /// <summary>
        /// Completely populate all Region property members. Missing data
        /// is computed based on the values that are already present.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="newLineIndex"></param>
        public static void Populate(this Region region, NewLineIndex newLineIndex)
        {
            // TODO: we need charOffset and byteOffset to be expressed as
            // nullable types in order to differentiate between text
            // and binary file regions. For text files, we need to populate
            // startLine, etc. based on document offset. For now, we'll 
            // assume we're always looking at text files

            if (region.StartLine == 0)
            {
                OffsetInfo offsetInfo = newLineIndex.GetOffsetInfoForOffset(region.Offset);
                region.StartLine = offsetInfo.LineNumber;
                region.StartColumn = offsetInfo.ColumnNumber;

                offsetInfo = newLineIndex.GetOffsetInfoForOffset(region.Offset + region.Length);
                region.StartLine = offsetInfo.LineNumber;
                region.EndColumn = offsetInfo.ColumnNumber;
            }
            else
            {
                // Make endColumn and endLine explicit, if not expressed
                if (region.EndLine == 0) { region.EndLine = region.StartLine; }
                if (region.EndColumn == 0) { region.EndColumn = region.StartColumn; }

                LineInfo lineInfo = newLineIndex.GetLineInfoForLine(region.StartLine);
                region.Offset = lineInfo.StartOffset + (region.StartColumn - 1);

                lineInfo = newLineIndex.GetLineInfoForLine(region.EndLine);
                region.Length = lineInfo.StartOffset + (region.EndColumn - 1) - region.Offset;
            }
        }
    }
}
