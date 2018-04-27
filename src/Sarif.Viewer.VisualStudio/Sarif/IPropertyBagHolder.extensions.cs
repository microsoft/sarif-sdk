// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using Microsoft.CodeAnalysis.Sarif;

namespace Microsoft.Sarif.Viewer.Models
{
    static class IPropertyBagHolderExtensions
    {
        const string CATEGORY = "category";

        public static string GetCategory(this IPropertyBagHolder item)
        {
            if (item == null)
            {
                return null;
            }

            string category = null;

            try
            {
                if (item.PropertyNames != null && item.PropertyNames.Contains(CATEGORY))
                {
                    category = item.GetProperty(CATEGORY);
                }
            }
            catch (NullReferenceException)
            {
            }

            return category;
        }
    }
}
