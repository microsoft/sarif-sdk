using Microsoft.CodeAnalysis.Sarif;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
