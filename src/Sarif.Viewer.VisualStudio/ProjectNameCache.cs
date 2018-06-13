// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System.Collections.Generic;
using EnvDTE;

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
