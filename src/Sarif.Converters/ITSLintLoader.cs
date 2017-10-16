using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public interface ITSLintLoader
    {
        TSLintLog ReadLog(Stream input);
    }
}
