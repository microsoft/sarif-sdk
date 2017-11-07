// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Input;

namespace Microsoft.Sarif.Viewer
{
    internal class RealKeyboard : IKeyboard
    {
        public IInputElement FocusedElement => Keyboard.FocusedElement;
    }
}
