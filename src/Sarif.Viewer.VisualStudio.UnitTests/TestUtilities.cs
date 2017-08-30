
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.ErrorList;

namespace Microsoft.Sarif.Viewer.VisualStudio.UnitTests
{
    public static class TestUtilities
    {
        public static void InitializeTestEnvironment()
        {
            // Creating a SarifViewerPackage object has the side effect of setting a static
            // variable in that class, without which the subsequent tests will fail. In production
            // code, the package object is always created before any SarifErrorListItem objects.
            new SarifViewerPackage();
        }

        public static void InitializeTestEnvironment(SarifLog sarifLog)
        {
            InitializeTestEnvironment();

            ErrorListService.ProcessSarifLog(sarifLog, "TestPath.sarif", null);
        }
    }
}
