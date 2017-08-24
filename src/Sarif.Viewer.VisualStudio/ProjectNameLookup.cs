using System.Collections.Generic;

namespace Microsoft.Sarif.Viewer
{
    public sealed class ProjectNameLookup
    {
        private readonly static ProjectNameLookup _instance = new ProjectNameLookup();
        private Dictionary<string, string> _projectNames = new Dictionary<string, string>();

        static ProjectNameLookup() { }
        private ProjectNameLookup() { }

        public static ProjectNameLookup Instance => _instance;

        public void SetName(string fileName)
        {
            if (_projectNames.ContainsKey(fileName))
                return;

            var project = new { Properties = "", ContainingProject = new { Name = "Test" } };
            //var project = SarifViewerPackage.Dte.Solution.FindProjectItem(fileName);
            if (project?.Properties != null && project?.ContainingProject != null)
                _projectNames[fileName] = project.ContainingProject.Name;
            else
                _projectNames[fileName] = string.Empty;
        }

        public string GetName(string fileName)
        {
            SetName(fileName);
            return _projectNames[fileName];
        }
    }
}
