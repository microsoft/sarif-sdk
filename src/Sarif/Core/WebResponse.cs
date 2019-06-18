// Copyright (c) Microsoft.  All Rights Reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace Microsoft.CodeAnalysis.Sarif
{
    public partial class WebResponse
    {
        public static WebResponse Parse(string responseString)
        {
            var webResponse = new WebResponse();

            string[] lines = responseString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            int lineIndex = 0;
            if (lines.Length > 0)
            {
                string requestLine = lines[0];
                string[] fields = requestLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (fields.Length > 0)
                {
                    webResponse.Protocol = fields[0];
                }

                if (fields.Length > 1 && int.TryParse(fields[1], NumberStyles.None, CultureInfo.InvariantCulture, out int statusCode))
                {
                    webResponse.StatusCode = statusCode;
                }

                ++lineIndex;
            }

            return webResponse;
        }
    }
}
