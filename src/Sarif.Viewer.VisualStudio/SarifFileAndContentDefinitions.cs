// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Utilities;

namespace SarifViewer
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
