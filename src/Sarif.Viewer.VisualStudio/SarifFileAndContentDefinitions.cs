using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Sarif.Viewer
{
    internal class SarifFileAndContentDefinitions
    {
        [Export]
        [Name("sarif")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition sarifContentTypeDefinition = new ContentTypeDefinition();

        [Export]
        [FileExtension(".sarif")]
        [BaseDefinition("sarif")]
        internal static FileExtensionToContentTypeDefinition sarifFileExtensionToContentTypeDefinition = new FileExtensionToContentTypeDefinition();

        [Export]
        [FileExtension(".sarif.json")]
        [BaseDefinition("sarif")]
        internal static FileExtensionToContentTypeDefinition sarifJsonFileExtensionToContentTypeDefinition = new FileExtensionToContentTypeDefinition();

    }
}
