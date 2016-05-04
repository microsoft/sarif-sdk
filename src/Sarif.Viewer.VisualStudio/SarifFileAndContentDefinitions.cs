using System.ComponentModel.Composition;
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
