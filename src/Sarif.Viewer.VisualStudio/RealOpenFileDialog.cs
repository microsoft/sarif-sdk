// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Win32;

namespace Microsoft.Sarif.Viewer
{
    internal class RealOpenFileDialog : IOpenFileDialog
    {
        private readonly OpenFileDialog _dialog;

        internal RealOpenFileDialog()
        {
            _dialog = new OpenFileDialog();
        }

        public string Title
        {
            get => _dialog.Title;

            set { _dialog.Title = value; }
        }

        public string Filter
        {
            get => _dialog.Filter;

            set { _dialog.Filter = value; }
        }

        public bool RestoreDirectory
        {
            get => _dialog.RestoreDirectory;

            set { _dialog.RestoreDirectory = value; }
        }

        public string FileName => _dialog.FileName;

        public bool? ShowDialog()
        {
            return _dialog.ShowDialog();
        }
    }
}