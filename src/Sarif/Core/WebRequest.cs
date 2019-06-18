// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebRequest
    {
        public static WebRequest Parse(string requestString)
        {
            var webRequest = new WebRequest();

            string[] lines = requestString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            int lineIndex = 0;
            if (lines.Length > 0)
            {
                string requestLine = lines[0];
                string[] fields = requestLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length > 0)
                {
                    webRequest.Method = fields[0];
                }

                ++lineIndex;
            }

            return webRequest;
        }
    }
}
