using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class ToolExtensions
    {
        public static ToolModel ToToolModel(this Tool tool)
        {
            if (tool == null)
            {
                return null;
            }

            ToolModel model = new ToolModel()
            {
                Name = !String.IsNullOrWhiteSpace(tool.FullName) ? tool.FullName : tool.Name,
                Version = !String.IsNullOrWhiteSpace(tool.SemanticVersion) ? tool.SemanticVersion : tool.Version,

                // TODO: Replace with real values
                //OwnerName = "John Doe",
                //OwnerUri = "mailto:johndoe@sarif.microsoft.com",
                //FeedbackUri = "mailto:toolfeedback@sarif.microsoft.com",
                //HelpUri = "https://microsoft.com/staticanalysis/tool",
                //Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
            };

            return model;
        }
    }
}
