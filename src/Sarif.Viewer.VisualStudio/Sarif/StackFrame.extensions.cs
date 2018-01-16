// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class StackFrameExtensions
    {
        public static StackFrameModel ToStackFrameModel(this StackFrame stackFrame)
        {
            StackFrameModel model = new StackFrameModel();

            model.Address = stackFrame.Address;
            model.FullyQualifiedLogicalName = stackFrame.FullyQualifiedLogicalName;
            model.Message = stackFrame.Message;
            model.Module = stackFrame.Module;
            model.Offset = stackFrame.Offset;

            PhysicalLocation physicalLocation = stackFrame.PhysicalLocation;
            if (physicalLocation != null)
            {
                model.FilePath = physicalLocation.Uri.ToPath();
                Region region = physicalLocation.Region;
                if (region != null)
                {
                    model.Line = region.StartLine;
                    model.Column = region.StartColumn;
                }
            }

            return model;
        }
    }
}
