// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

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
    }
}
