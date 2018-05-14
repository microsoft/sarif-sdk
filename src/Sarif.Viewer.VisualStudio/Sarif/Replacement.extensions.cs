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

            model.DeletedLength = replacement.DeletedLength;

            if (!String.IsNullOrEmpty(replacement.InsertedBytes))
            {
                model.InsertedString = Encoding.UTF8.GetString(Convert.FromBase64String(replacement.InsertedBytes));
                model.InsertedBytes = Encoding.UTF8.GetBytes(model.InsertedString);
            }

            model.Offset = replacement.Offset;

            return model;
        }
    }
}
