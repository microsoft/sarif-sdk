// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Sarif.Viewer
{
    public class CodeLocationsStyleSelector : StyleSelector
    {
        public Style CodeLocationsStyle
        { get; set; }

        public Style CategoryStyle
        { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            CollectionViewGroup cvg = item as CollectionViewGroup;

            if (cvg != null)
            {
                if (!cvg.IsBottomLevel)
                {
                    return CategoryStyle;
                }

                // TODO create two styles, for second-level nesting
                // of stacks vs. execution flows that rename nodes
                // from 1,2,3 to Stack 1, Stack 2, etc.
                return CodeLocationsStyle;
            }
            return base.SelectStyle(item, container);
        }
    }
}
