// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using System;
using System.Text;
using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class ReplacementExtensions
    {
        public static ReplacementModel ToReplacementModel(this Replacement replacement)
        {
            if (replacement == null)
            {
                return null;
            }

            ReplacementModel model = new ReplacementModel();

            model.DeletedLength = replacement.DeletedRegion.Length;
            model.Offset = replacement.DeletedRegion.Offset;

            if (!string.IsNullOrWhiteSpace(replacement.InsertedContent?.Text))
            {
                model.InsertedString = replacement.InsertedContent.Text;
            }
            else if (replacement.InsertedContent?.Binary != null)
            {
                model.InsertedBytes = Convert.FromBase64String(replacement.InsertedContent.Binary);
                model.InsertedString = Encoding.UTF8.GetString(model.InsertedBytes);
            }

            return model;
        }
    }
}
