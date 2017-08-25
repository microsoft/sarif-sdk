using System.Collections.Generic;

namespace Microsoft.Sarif.Viewer
{
    public sealed class ProjectNameCache
    {
        private readonly static ProjectNameCache instance = new ProjectNameCache();
        private Dictionary<string, string> projectNames = new Dictionary<string, string>();

        static ProjectNameCache() { }
        private ProjectNameCache() { }

        public static ProjectNameCache Instance => instance;

        public void SetName(string fileName)
        {
            if (projectNames.ContainsKey(fileName))
            {
                return;
            }

            var project = SarifViewerPackage.Dte.Solution.FindProjectItem(fileName);
            if (project?.ContainingProject != null)
            {
                projectNames[fileName] = project.ContainingProject.Name;
            }
            else
            {
                projectNames[fileName] = string.Empty;
            }
        }

        public string GetName(string fileName)
        {
            SetName(fileName);
            return projectNames[fileName];
        }
    }
}
