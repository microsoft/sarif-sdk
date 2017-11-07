﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Sarif.Viewer
{
    public class CommonDialogFactory : ICommonDialogFactory
    {
        public IOpenFileDialog CreateOpenFileDialog()
        {
            return new RealOpenFileDialog();
        }
    }
}
