// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;

namespace Microsoft.Sarif.Viewer
{
    internal sealed partial class Guids
    {
        public const string GuidVSPackageString = "b97edb99-282e-444c-8f53-7de237f2ec5e";
        public const string GuidSarifEditorFactoryString = "260721a9-9e3f-40a3-a696-149fe9672194";

        public static readonly Guid GuidVSPackage = new Guid(GuidVSPackageString);
        public static readonly Guid GuidSarifEditorFactory = new Guid(GuidSarifEditorFactoryString);
    }
}