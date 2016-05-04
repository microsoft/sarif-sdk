// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Sarif.Viewer
{
    public class CodeLocationsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CodeLocationsTemplate
        { get; set; }

        public DataTemplate CategoryTemplate
        { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ContentPresenter cp = container as ContentPresenter;

            if (cp != null)
            {
                CollectionViewGroup cvg = cp.Content as CollectionViewGroup;

                if (!cvg.IsBottomLevel)
                {
                    return CategoryTemplate;
                }
                return CodeLocationsTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
