// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows;

namespace Microsoft.Sarif.Viewer
{
    public interface IKeyboard
    {
        IInputElement FocusedElement { get; }
    }
}
