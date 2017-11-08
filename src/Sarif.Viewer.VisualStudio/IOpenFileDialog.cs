// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Sarif.Viewer
{
    public interface IOpenFileDialog
    {
        string Title { get; set; }

        string Filter { get; set; }

        bool RestoreDirectory { get; set; }

        string FileName { get; }

        bool? ShowDialog();
    }
}
