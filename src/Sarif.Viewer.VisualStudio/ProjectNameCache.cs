using EnvDTE;
using System.Collections.Generic;

namespace Microsoft.Sarif.Viewer
{
    public sealed class ProjectNameCache
    {
        private Dictionary<string, string> projectNames = new Dictionary<string, string>();

        private readonly Solution solution;

        public ProjectNameCache(Solution solution)
        {
            this.solution = solution;
        }

        public string GetName(string fileName)
        {
            SetName(fileName);

            return projectNames[fileName];
        }

        private void SetName(string fileName)
        {
            if (projectNames.ContainsKey(fileName))
            {
                return;
            }

            var project = solution?.FindProjectItem(fileName);
            if (project?.ContainingProject != null)
            {
                projectNames[fileName] = project.ContainingProject.Name;
            }
            else
            {
                projectNames[fileName] = string.Empty;
            }
        }
    }
}
