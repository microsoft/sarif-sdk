using System.IO;
using Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public interface ITSLintLoader
    {
        TSLintLog ReadLog(Stream input);
    }
}
