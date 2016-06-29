// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 

using Microsoft.CodeAnalysis.Sarif;
using Microsoft.Sarif.Viewer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Sarif.Viewer.Sarif
{
    static class InvocationExtensions
    {
        public static InvocationModel ToInvocationModel(this Invocation invocation)
        {
            if (invocation == null)
            {
                return null;
            }

            InvocationModel model = new InvocationModel()
            {
                CommandLine = invocation.CommandLine,
                StartTime = invocation.StartTime,
                EndTime = invocation.EndTime,
                ProcessId = invocation.ProcessId,
                FileName = invocation.FileName,
                WorkingDirectory = invocation.WorkingDirectory,
            };

            return model;
        }
    }
}
