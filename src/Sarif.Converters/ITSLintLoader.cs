using Microsoft.CodeAnalysis.Sarif.Converters.TSLintObjectModel;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Converters
{
    public interface ITSLintLoader
    {
        TSLintLog ReadLog(Stream input);
    }
}
