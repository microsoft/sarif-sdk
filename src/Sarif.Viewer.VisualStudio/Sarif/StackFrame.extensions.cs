// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class StackFrameExtensions
    {
        public static StackFrameModel ToStackFrameModel(this StackFrame stackFrame)
        {
            StackFrameModel model = new StackFrameModel();

            model.Address = stackFrame.Address;
            model.Column = stackFrame.Column;
            model.FullyQualifiedLogicalName = stackFrame.FullyQualifiedLogicalName;
            model.Line = stackFrame.Line;
            model.Message = stackFrame.Message;
            model.Module = stackFrame.Module;
            model.Offset = stackFrame.Offset;
            model.FilePath = stackFrame.Uri.ToPath();

            return model;
        }
    }
}
